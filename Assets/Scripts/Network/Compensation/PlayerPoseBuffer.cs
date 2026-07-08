using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPoseBuffer : MonoBehaviour
{
    [SerializeField, Min(0)]
    private int bufferSize = 60;

    private readonly List<PoseSample> _poseBuffer = new();

    public IReadOnlyList<PoseSample> Samples { get => _poseBuffer; }

    public void AddSample(PoseData pose, float timestamp)
    {
        if (Samples.Count > 0 && GetLastSample().timestamp >= timestamp)
        {
            return; 
        }

        _poseBuffer.Add(new(pose, timestamp));

        if (_poseBuffer.Count > bufferSize)
        {
            _poseBuffer.RemoveAt(0);
        }
    }

    public PoseSample GetLastSample() => _poseBuffer.Count > 0 ? _poseBuffer[^1] : default;
    public PoseSample GetPreviousSample() => _poseBuffer.Count > 0 ? _poseBuffer[^2] : default;

    public int GetBufferSize() => _poseBuffer.Count;
    public int GetMaxBufferSize() => bufferSize;
    public bool IsBufferEmpty() => _poseBuffer.Count == 0;

    
    public static bool PoseEqualsApprox(in PoseData a, in PoseData b, float posEps = 0.0001f, float rotEps = 0.001f) =>
            Vector3.SqrMagnitude(a.lhandPos - b.lhandPos) < posEps * posEps && 
            Quaternion.Dot(a.lhandRot, b.lhandRot) > 1f - rotEps && 
            Vector3.SqrMagnitude(a.rhandPos - b.rhandPos) < posEps * posEps && 
            Quaternion.Dot(a.rhandRot, b.rhandRot) > 1f - rotEps &&
            Vector3.SqrMagnitude(a.headPos - b.headPos) < posEps * posEps && 
            Quaternion.Dot(a.headRot, b.headRot) > 1f - rotEps;

    public static Vector3 CalculateVelocity(Vector3 currentPos, Vector3 previousPos, 
            float currentTimestamp, float previousTimestamp)
    {
        float dt = currentTimestamp - previousTimestamp;
        return dt > 0f ? (currentPos - previousPos) / dt : Vector3.zero;
    }
}
