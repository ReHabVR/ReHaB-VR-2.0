public struct PoseSample
{
    public PoseData pose;
    public float timestamp;
}

public interface ICompensationMethod
{
    PoseData Compensate(PoseData networkPose);
}
