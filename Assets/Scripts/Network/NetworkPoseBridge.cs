using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fusion;
using UnityEngine;
using UnityEngine.XR;

[Serializable]
public struct PoseData : INetworkStruct
{
    public Vector3 headPos;
    public Quaternion headRot;

    public Vector3 lhandPos;
    public Quaternion lhandRot;

    public Vector3 rhandPos;
    public Quaternion rhandRot;

    public FloatCompressed gripL;
    public FloatCompressed gripR;

    public bool isValid;
}

public struct PoseInput : INetworkInput
{
    public PoseData pose;
}

public struct PoseSample
{
    public PoseData pose;
    public float timestamp;
}

public class NetworkPoseBridge : NetworkBehaviour
{
    [SerializeField] 
    private Animator handAnimator;

    [Header("Interpolation Settings")]
    [SerializeField]
    private int interpolationBufferSize = 8;
    [SerializeField, Min(0f)]
    private float interpolationDelay = 0.1f;

    [Header("Hands")]
    [SerializeField] 
    private List<NetworkHand> networkHands;

    [Header("XR Controllers")]
    [SerializeField] 
    private Transform XRHead;
    [SerializeField] 
    private Transform XRLeftHand;
    [SerializeField] 
    private Transform XRRightHand;
    [Header("Bridge Targets")]
    [SerializeField] 
    private Transform bridgeHead;
    [SerializeField] 
    private Transform bridgeLeftHand;
    [SerializeField] 
    private Transform bridgeRightHand;

    [Header("Fallback Pose")]
    [SerializeField] 
    private Transform fallbackHead;
    [SerializeField] 
    private Transform fallbackLeftHand;
    [SerializeField] 
    private Transform fallbackRightHand;


    private PoseData _localPose = new()
    {
        isValid = false
    };
    
    private PoseData _lastNetworkPose = default;
    private readonly List<PoseSample> _poseBuffer = new();

    float _lastTimestamp = 0.0f;

    [Space(5), Header("XR Devices")]
    [SerializeField]
    private InputDeviceCharacteristics controllerL;
    [SerializeField]
    private InputDeviceCharacteristics controllerR;
    private static readonly List<InputDevice> _devices = new();
    private InputDevice _targetDeviceL;
    private InputDevice _targetDeviceR;
    private bool _targetDeviceDetected;


    [Networked, HideInInspector]
    public PoseData NetworkPose { get; set; }

    public bool IsReady { get; private set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            (NetworkHandResolver.Instance as NetworkHandResolver).RegisterBridge(Object.InputAuthority, this);
        }

        if (HasInputAuthority && Object.InputAuthority == Runner.LocalPlayer)
        {
            FusionSessionManager.Instance.SetLocalBridge(this);
            _localPose = GetPose();

            foreach (NetworkHand hand in networkHands)
            {
                hand.Owner = Runner.LocalPlayer;
            }
        }

        foreach (ExternalPoseProvider provider in GetComponentsInChildren<ExternalPoseProvider>(true))
        {
            provider.OnSpawned();
        }

    #if CLIENT_VR
        TryGetDevices();
    #endif

        IsReady = true;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Object.HasStateAuthority)
        {
            (NetworkHandResolver.Instance as NetworkHandResolver).UnregisterBridge(Object.InputAuthority);
        }
    }

    private void Update()
    {
        if (HasInputAuthority)
        {
            _localPose = GetPose();
        }
    }

    public PoseData GetLocalPose() => _localPose;

    public void SetExternalPose(PoseData pose)
    {
        _localPose = pose;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object == null || !Object.IsValid)
        {
            return;
        }
        
        _lastTimestamp = Runner.SimulationTime;

        if (HasStateAuthority)
        {
            if (Runner.TryGetInputForPlayer(Object.InputAuthority, out PoseInput input))
            {
                NetworkPose = input.pose;
            }
        }

        if (!HasInputAuthority)
        {
            _lastNetworkPose = NetworkPose;
        }
    }

    public override void Render()
    {
        if (!IsReady || Object == null || !Object.IsValid)
        {
            return;
        }

        // _localPose = local motion for the player
        // NetworkPose = pose replicated to other peers (what the server "sees")
        // renderedPose = what the client sees in VR
        // For local movement, use _localPose - for the other player, renderedPose is NetworkPose with compensation

        if (HasInputAuthority)
        {
            ApplyPose(_localPose);
        }
        else
        {
            PoseData renderedPose = ApplyCompensation(_lastNetworkPose);
            ApplyPose(renderedPose);
        }
    }

    private void TryGetDevices()
    {
        bool detected = GetFirstDevice(controllerL, out _targetDeviceL) && GetFirstDevice(controllerR, out _targetDeviceR);
        if (detected && !_targetDeviceDetected)
        {
            Debug.Log("Controller devices detected successfully.");
        }

        _targetDeviceDetected = detected;
    }

    private bool GetFirstDevice(InputDeviceCharacteristics devChar, out InputDevice inputDevice)
    {
        _devices.Clear();
        InputDevices.GetDevicesWithCharacteristics(devChar, _devices);

        if (_devices.Count < 1)
        {
            inputDevice = default;
            return false;
        }

        inputDevice = _devices[0];
        return true;
    }

    private void ApplyPose(PoseData finalPose)
    {
        bridgeHead.SetPositionAndRotation(finalPose.headPos, finalPose.headRot);
        bridgeLeftHand.SetPositionAndRotation(finalPose.lhandPos, finalPose.lhandRot);
        bridgeRightHand.SetPositionAndRotation(finalPose.rhandPos, finalPose.rhandRot);

        UpdateHandAnimation(finalPose);
    }

    private void UpdateHandAnimation(PoseData pose)
    {
        if (handAnimator)
        {
            handAnimator.SetFloat("GripL", pose.gripL);
            handAnimator.SetFloat("GripR", pose.gripR);
        }
    }

    public void SetLocalPose(PoseData pose) { _localPose = pose; }

    private PoseData ApplyCompensation(PoseData networkPose)
    {
        CompensationMode compensationMode = FusionSessionManager.Instance.CurrentCompensationMode;
        
        return compensationMode switch
        {
            CompensationMode.Interpolation => InterpolatePose(networkPose),
            CompensationMode.KalmanFilter => KalmanPose(networkPose),
            _ => networkPose,
        };
    }

    private void AddPoseSample(PoseData pose)
    {
        _poseBuffer.Add(
            new()
            {
                pose = pose,
                timestamp = _lastTimestamp
            }
        );

        if (_poseBuffer.Count > interpolationBufferSize)
        {
            _poseBuffer.RemoveAt(0);
        }
    }

    private PoseData InterpolatePose(PoseData networkPose)
    {
        if (_poseBuffer.Count == 0 || !PoseEqualsApprox(_lastNetworkPose, networkPose))
        {
            AddPoseSample(networkPose);
            //_lastNetworkPose = networkPose;
        }

        if (_poseBuffer.Count < 2)
        {
            return networkPose;
        }

        float renderTime = _lastTimestamp - interpolationDelay;
        for (int i = _poseBuffer.Count - 2; i >= 0; i--) // reverse search
        {
            PoseSample sampleA = _poseBuffer[i];
            PoseSample sampleB = _poseBuffer[i + 1];
            if (sampleA.timestamp <= renderTime && sampleB.timestamp >= renderTime)
            {
                float t = Mathf.Clamp01(Mathf.InverseLerp(sampleA.timestamp, sampleB.timestamp, renderTime));
                PoseData posA = sampleA.pose;
                PoseData posB = sampleB.pose;
                return new()
                {
                    headPos = Vector3.Lerp(posA.headPos, posB.headPos, t),
                    headRot = Quaternion.Slerp(posA.headRot, posB.headRot, t),

                    lhandPos = Vector3.Lerp(posA.lhandPos, posB.lhandPos, t),
                    lhandRot = Quaternion.Slerp(posA.lhandRot, posB.lhandRot, t),

                    rhandPos = Vector3.Lerp(posA.rhandPos, posB.rhandPos, t),
                    rhandRot = Quaternion.Slerp(posA.rhandRot, posB.rhandRot, t),

                    gripL = Mathf.Lerp((float)posA.gripL, (float)posB.gripL, t),
                    gripR = Mathf.Lerp((float)posA.gripR, (float)posB.gripR, t)
                };
            }
        }

        return _poseBuffer[^1].pose;
    }

    private PoseData KalmanPose(PoseData networkPose)
    {
        PoseData pose = networkPose;
        //TODO: kalman filter
        return pose;
    }

    private PoseData GetPose()
    {
    #if CLIENT_VR
        if (HasInputAuthority && _targetDeviceDetected)
        {
            return CaptureXR();
        }
    #endif
        return CaptureFallback();
    }

    private PoseData CaptureXR() 
    {
        // Return default pose if rig doesn't have input authority
        // This should not happen normally, but it prevents rare edge cases
        if (!HasInputAuthority)
        {
            return default;
        }

        float gripLeft = 0f;
        float gripRight = 0f;

        if (!_targetDeviceL.isValid || !_targetDeviceR.isValid)
        {
            _targetDeviceDetected = false;
        }
        
        if (_targetDeviceDetected)
        {
            if (_targetDeviceL.TryGetFeatureValue(CommonUsages.trigger, out float valueL))
            {
                gripLeft = valueL;
            }
            if (_targetDeviceR.TryGetFeatureValue(CommonUsages.trigger, out float valueR))
            {
                gripRight = valueR;
            }
        }
        else
        {
            TryGetDevices();
        }

        return new()
        {
            headPos = XRHead.position,
            headRot = XRHead.rotation,
            lhandPos = XRLeftHand.position,
            lhandRot = XRLeftHand.rotation,
            rhandPos = XRRightHand.position,
            rhandRot = XRRightHand.rotation,
            gripL = gripLeft,
            gripR = gripRight,
            isValid = true
        };
    }

    private PoseData CaptureFallback() => new() 
    {
        headPos = fallbackHead.position,
        headRot = fallbackHead.rotation,
        lhandPos = fallbackLeftHand.position,
        lhandRot = fallbackLeftHand.rotation,
        rhandPos = fallbackRightHand.position,
        rhandRot = fallbackRightHand.rotation,
        gripL = 0.0f,
        gripR = 0.0f,
        isValid = true
    };

    public float GetGripL()
    {
        if (Object == null || !Object.IsValid)
        {
            return 0f;
        }

        return HasInputAuthority ? (float)_localPose.gripL : (float)NetworkPose.gripL;
    }

    public float GetGripR()
    {
        if (Object == null || !Object.IsValid)
        {
            return 0f;
        }

        return HasInputAuthority ? (float)_localPose.gripR : (float)NetworkPose.gripR;
    }

    private bool PoseEqualsApprox(in PoseData a, in PoseData b, float posEps = 0.0001f, float rotEps = 0.001f) =>
            Vector3.SqrMagnitude(a.lhandPos - b.lhandPos) < posEps * posEps && 
            Quaternion.Dot(a.lhandRot, b.lhandRot) > 1f - rotEps && 
            Vector3.SqrMagnitude(a.rhandPos - b.rhandPos) < posEps * posEps && 
            Quaternion.Dot(a.rhandRot, b.rhandRot) > 1f - rotEps;
}
