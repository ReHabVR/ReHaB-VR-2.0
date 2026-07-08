using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadReckoningCompensationMethod : ICompensationMethod
{
    private readonly PlayerPoseBuffer _poseBuffer;

    public DeadReckoningCompensationMethod(PlayerPoseBuffer poseBuffer)
    {
        _poseBuffer = poseBuffer;
    }

    public PoseData Compensate(PoseData networkPose, float renderTime)
    {
        return networkPose;
    }
}
