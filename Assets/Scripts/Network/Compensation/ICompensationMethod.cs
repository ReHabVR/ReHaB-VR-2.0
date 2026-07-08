using UnityEngine;

public struct PoseSample
{
    public PoseData pose;
    public float timestamp;

    public PoseSample(PoseData pose, float timestamp)
    {
        this.pose = pose;
        this.timestamp = timestamp;
    }
}

public interface ICompensationMethod
{
    PoseData Compensate(PoseData networkPose, float renderTime);
}
