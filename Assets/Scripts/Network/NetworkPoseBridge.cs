using System;
using System.Collections.Generic;
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

    public float gripL;
    public float gripR;
}

public struct PoseInput : INetworkInput
{
    public PoseData pose;
}

public class NetworkPoseBridge : NetworkBehaviour
{
    [SerializeField] 
    private Animator handAnimator;
    [SerializeField]
    private CompensationManager compensationManager;
    [SerializeField]
    private PlayerPoseBuffer poseBuffer;

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

    [Header("Hand Sockets")]
    [SerializeField]
    private Transform leftHandSocket;
    [SerializeField]
    private Transform rightHandSocket;

    [Header("XR Devices")]
    [SerializeField]
    private InputDeviceCharacteristics controllerL;
    [SerializeField]
    private InputDeviceCharacteristics controllerR;
    private static readonly List<InputDevice> _devices = new();
    private InputDevice _targetDeviceL;
    private InputDevice _targetDeviceR;
    private bool _targetDeviceDetected;

    private PoseData _localPose = new();

    [Networked, HideInInspector]
    public PoseData NetworkPose { get; set; }

    public bool IsReady { get; private set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            (NetworkHandResolver.Instance as NetworkHandResolver).RegisterBridge(Object.InputAuthority, this);
        }

        if (Object.InputAuthority == Runner.LocalPlayer)
        {
            FusionSessionManager.Instance.SetLocalBridge(this);
            if (HasInputAuthority)
            {
                _localPose = GetPose();
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

    public override void FixedUpdateNetwork()
    {
        if (Object == null || !Object.IsValid || !Runner.IsRunning)
        {
            return;
        }
        
        if (HasStateAuthority)
        {
            if (GetInput(out PoseInput input))
            {
                NetworkPose = input.pose;
            }
        }
        
        if (!HasInputAuthority)
        {
            poseBuffer.AddSample(NetworkPose, Runner.SimulationTime);
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
        else // Remote client
        {
            PoseData renderedPose = compensationManager.ApplyCompensation(NetworkPose);
            ApplyPose(renderedPose);
        }
    }

    public bool TryGetHandSocket(EHandType handType, out Transform socketTransform)
    {
        if (handType == EHandType.Right)
        {
            socketTransform = rightHandSocket.transform;
            return true;
        }
        if (handType == EHandType.Left)
        {
            socketTransform = leftHandSocket.transform;
            return true;
        }
        
        Debug.LogError("Invalid hand type!");
        socketTransform = default;
        return false;
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

    public PoseData GetLocalPose() => _localPose;
    public void SetLocalPose(PoseData pose) { _localPose = pose; }

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
            gripR = gripRight
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
        gripR = 0.0f
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
}
