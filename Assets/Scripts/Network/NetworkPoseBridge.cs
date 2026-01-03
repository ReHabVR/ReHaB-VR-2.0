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

    private PoseData _currentPose;
    private bool _xrActive;

    private readonly WaitForSeconds _waitForThreeSeconds = new(3);

    [Networked]
    private PoseData NetworkPose { get => default; set {} }

    private bool IsXRTrackingValid => 
        XRLeftHand.position.sqrMagnitude > 0.0001f &&
        XRRightHand.position.sqrMagnitude > 0.0001f;
    
    private void Start()
    {
        StartCoroutine(QueryXRState());
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            NetworkPose = CaptureFallback();
        }
    }

    public override void Render()
    {
        PoseData newPose = (HasInputAuthority && _xrActive && IsXRTrackingValid) ? _currentPose : CaptureFallback();

        bridgeHead.SetPositionAndRotation(newPose.headPos, newPose.headRot);
        bridgeLeftHand.SetPositionAndRotation(newPose.lhandPos, newPose.lhandRot);
        bridgeRightHand.SetPositionAndRotation(newPose.rhandPos, newPose.rhandRot);
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            _currentPose = GetPose();
            NetworkPose = _currentPose;
        }
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

    private PoseData GetPose()
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
