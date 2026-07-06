using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class KalmanCompensationMethod : ICompensationMethod
{
    private readonly PlayerPoseBuffer poseBuffer;

    private readonly PoseKalmanFilter _headFilter = new();
    private readonly PoseKalmanFilter _lhandFilter = new();
    private readonly PoseKalmanFilter _rhandFilter = new();

    public KalmanCompensationMethod(PlayerPoseBuffer poseBuffer)
    {
        this.poseBuffer = poseBuffer;
    }

    public PoseData Compensate(PoseData networkPose)
    {
        float dt = poseBuffer.GetLastSample().timestamp - poseBuffer.GetPreviousSample().timestamp;
        return new()
        {
            headPos = _headFilter.Update(networkPose.headPos, dt), 
            headRot = networkPose.headRot,
            lhandPos = _lhandFilter.Update(networkPose.lhandPos, dt), 
            lhandRot = networkPose.lhandRot,
            rhandPos = _rhandFilter.Update(networkPose.rhandPos, dt), 
            rhandRot = networkPose.rhandRot,
            gripL = networkPose.gripL,
            gripR = networkPose.gripR
        };
    }
}
