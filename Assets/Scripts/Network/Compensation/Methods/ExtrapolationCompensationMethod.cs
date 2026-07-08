using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtrapolationCompensationMethod : ICompensationMethod
{
    private readonly PlayerPoseBuffer _poseBuffer;

    public ExtrapolationCompensationMethod(PlayerPoseBuffer poseBuffer)
    {
        _poseBuffer = poseBuffer;
    }

    public PoseData Compensate(PoseData networkPose, float renderTime)
    {
        if (_poseBuffer.Samples.Count < 2)
        {
            return networkPose;
        }

        PoseSample previousSample = _poseBuffer.GetPreviousSample();
        PoseSample latestSample = _poseBuffer.GetLastSample();
    
        float predTime = Mathf.Clamp(renderTime - latestSample.timestamp, 0.0f, 0.2f);

        Vector3 headVel = PlayerPoseBuffer.CalculateVelocity(
            latestSample.pose.headPos, previousSample.pose.headPos, 
            latestSample.timestamp, previousSample.timestamp
        );
        Vector3 lHandVel = PlayerPoseBuffer.CalculateVelocity(
            latestSample.pose.lhandPos, previousSample.pose.lhandPos, 
            latestSample.timestamp, previousSample.timestamp
        );
        Vector3 rHandVel = PlayerPoseBuffer.CalculateVelocity(
            latestSample.pose.rhandPos, previousSample.pose.rhandPos, 
            latestSample.timestamp, previousSample.timestamp
        );

        return new()
        {
            headPos = latestSample.pose.headPos + (headVel * predTime),
            headRot = latestSample.pose.headRot,
            lhandPos = latestSample.pose.lhandPos + (lHandVel * predTime),
            lhandRot = latestSample.pose.lhandRot,
            rhandPos = latestSample.pose.rhandPos + (rHandVel * predTime),
            rhandRot = latestSample.pose.rhandRot,
            gripL = latestSample.pose.gripL,
            gripR = latestSample.pose.gripR
        };
    }
}
