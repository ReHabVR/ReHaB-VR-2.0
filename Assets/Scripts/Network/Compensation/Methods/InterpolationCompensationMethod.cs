using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolationCompensationMethod : ICompensationMethod
{
    private readonly PlayerPoseBuffer poseBuffer;

    public InterpolationCompensationMethod(PlayerPoseBuffer poseBuffer)
    {
        this.poseBuffer = poseBuffer;
    }

    public PoseData Compensate(PoseData networkPose, float renderTime)
    {
        if (poseBuffer.Samples.Count < 2)
        {
            return networkPose;
        }

        float oldestTimestamp = poseBuffer.Samples[0].timestamp;
        float latestTimestamp = poseBuffer.GetLastSample().timestamp;

        if (renderTime <= oldestTimestamp)
        {
            return poseBuffer.Samples[0].pose;
        }

        if (renderTime >= latestTimestamp)
        {
            return poseBuffer.GetLastSample().pose;
        }

        for (int i = poseBuffer.GetBufferSize() - 2; i >= 0; i--) // reverse search
        {
            PoseSample sampleA = poseBuffer.Samples[i];
            PoseSample sampleB = poseBuffer.Samples[i + 1];
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

                    gripL = Mathf.Lerp(posA.gripL, posB.gripL, t),
                    gripR = Mathf.Lerp(posA.gripR, posB.gripR, t)
                };
            }
        }

        // Fallback for unexpected edge cases
        return poseBuffer.GetLastSample().pose;
    }
}
