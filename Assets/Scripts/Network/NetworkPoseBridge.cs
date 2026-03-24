using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using ReHaB.Core;
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
}

public struct PoseInput : INetworkInput
{
    public PoseData pose;
}

public class NetworkPoseBridge : NetworkBehaviour, IHandPoseSource
{
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

    private PoseData _localPose;

    private InputDeviceCharacteristics controllerL;
    private InputDeviceCharacteristics controllerR;
    private static readonly List<InputDevice> _devices = new();
    private InputDevice _targetDeviceL;
    private InputDevice _targetDeviceR;
    private bool _targetDeviceDetected;

    [Networked]
    public PoseData NetworkPose { get; set; }

    public bool IsReady { get; private set; }

    public override void Spawned()
    {

        if (HasInputAuthority && Object.InputAuthority == Runner.LocalPlayer)
        {
            FusionSessionManager.Instance.SetLocalBridge(this);
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

    public override void FixedUpdateNetwork()
    {
        if (!IsReady)
        { 
            return;
        }

    #if CLIENT_VR
        if (!_targetDeviceDetected)
        {
            TryGetDevices();
        }
    #endif

        if (HasStateAuthority && Runner.TryGetInputForPlayer(Object.InputAuthority, out PoseInput input))
        {
            NetworkPose = input.pose;
        }
    }

    public override void Render()
    {
        if (!IsReady || Object == null || !Object.IsValid)
        {
            return;
        }

        PoseData renderedPose;

        // _localPose = local motion for the player
        // NetworkPose = pose replicated to other peers (what the server "sees")
        // renderedPose = what the client sees in VR
        // For local movement, use _localPose - for the other player, renderedPose is NetworkPose with compensation

        if (HasInputAuthority)
        {
            _localPose = GetPose();
            renderedPose = _localPose;
        }
        else
        {
            renderedPose = ApplyCompensation(NetworkPose);
        }
        
        ApplyPose(renderedPose);
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
        bridgeHead.position = finalPose.headPos;
        bridgeHead.rotation = finalPose.headRot;

        bridgeLeftHand.position = finalPose.lhandPos;
        bridgeLeftHand.rotation = finalPose.lhandRot;

        bridgeRightHand.position = finalPose.rhandPos;
        bridgeRightHand.rotation = finalPose.rhandRot;
    }

    public void SetLocalPose(PoseData pose) { _localPose = pose; }

    private PoseData ApplyCompensation(PoseData networkPose)
    {
        PredictionMode predictionMode = FusionSessionManager.Instance.CurrentPredictionMode;
        
        return predictionMode switch
        {
            PredictionMode.Custom => networkPose, // TODO: Prediction model
            _ => networkPose,
        };
    }

    public PoseData GetPose()
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
        float gripLeft = 0f;
        if (_targetDeviceL.TryGetFeatureValue(CommonUsages.trigger, out float valueL)){
            gripLeft = valueL;
        }

        float gripRight = 0f;
        if (_targetDeviceR.TryGetFeatureValue(CommonUsages.trigger, out float valueR))
        {
            gripRight = valueR;
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

    private PoseData CaptureFallback() => new() {
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
