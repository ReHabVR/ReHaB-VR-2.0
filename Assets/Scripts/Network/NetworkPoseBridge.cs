using System;
using System.Collections;
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
}

public struct PoseInput : INetworkInput
{
    public PoseData pose;
}

public class NetworkPoseBridge : NetworkBehaviour
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
    private PlayerRef _playerRef;

    private bool _xrActive;

    private readonly WaitForSeconds _waitForThreeSeconds = new(3);

    [Networked]
    private PoseData NetworkPose { get => default; set {} }

    public bool IsReady { get; private set; }

    private bool IsXRTrackingValid => 
        XRLeftHand.position.sqrMagnitude > 0.0001f &&
        XRRightHand.position.sqrMagnitude > 0.0001f;
    
    private void Start()
    {
        StartCoroutine(QueryXRState());
    }

    public override void Spawned()
    {
        if (HasInputAuthority && Object.InputAuthority == Runner.LocalPlayer)
        {
            FusionSessionManager.Instance.SetLocalBridge(this);
            _localPose = GetPose();
        }

        if (HasStateAuthority)
        {
            NetworkPose = GetPose();
        }

        foreach (ExternalPoseProvider provider in GetComponentsInChildren<ExternalPoseProvider>(true))
        {
            provider.OnSpawned();
        }

        IsReady = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !IsReady)
        { 
            return;
        }

        if (Runner.TryGetInputForPlayer<PoseInput>(Object.InputAuthority, out var input))
        {
            NetworkPose = input.pose;
        }
    }

    public override void Render()
    {
        // _localPose = local motion for the player
        // NetworkPose = pose replicated to other peers (what the server "sees")
        // renderedPose = what the client sees in VR
        // For local movement, use _localPose - for the other player, renderedPose is NetworkPose with compensation

        PoseData renderedPose = HasInputAuthority ? _localPose : ApplyCompensation(NetworkPose);
        
        bridgeHead.SetPositionAndRotation(renderedPose.headPos, renderedPose.headRot);
        bridgeLeftHand.SetPositionAndRotation(renderedPose.lhandPos, renderedPose.lhandRot);
        bridgeRightHand.SetPositionAndRotation(renderedPose.rhandPos, renderedPose.rhandRot);
    }

    public PoseData GetLocalPose() => _localPose;
    public void SetLocalPose(PoseData pose) { _localPose = pose; }
    public void SetPlayerRef(PlayerRef player) { _playerRef = player; }

    private PoseData ApplyCompensation(PoseData networkPose)
    {
        // TODO: Prediction model
        return networkPose;
    }

    private IEnumerator QueryXRState()
    {
        while(true)
        {
            List<XRInputSubsystem> subsystems = new();
            SubsystemManager.GetSubsystems(subsystems);

            _xrActive = subsystems.Exists(s => s.running);

            yield return _waitForThreeSeconds;
        }
    }

    public PoseData GetPose()
    {
        if (HasInputAuthority && _xrActive && IsXRTrackingValid)
        {
            return CaptureXR();
        }

        return CaptureFallback();
    }

    private PoseData CaptureXR() => new() {
        headPos = XRHead.position,
        headRot = XRHead.rotation,
        lhandPos = XRLeftHand.position,
        lhandRot = XRLeftHand.rotation,
        rhandPos = XRRightHand.position,
        rhandRot = XRRightHand.rotation
    };

    private PoseData CaptureFallback() => new() {
        headPos = fallbackHead.position,
        headRot = fallbackHead.rotation,
        lhandPos = fallbackLeftHand.position,
        lhandRot = fallbackLeftHand.rotation,
        rhandPos = fallbackRightHand.position,
        rhandRot = fallbackRightHand.rotation
    };
}
