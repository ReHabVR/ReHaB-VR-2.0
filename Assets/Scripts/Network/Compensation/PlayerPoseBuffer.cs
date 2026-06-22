using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPoseBuffer : MonoBehaviour
{
    [SerializeField, Min(0)]
    private int bufferSize = 8;

    [SerializeField, Min(0f)]
    private float compensationStep = 0.1f; // Time offset between samples for prediction methods

    private float _lastTimestamp;
    private readonly List<PoseSample> _poseBuffer = new();

    public IReadOnlyList<PoseSample> Samples { get => _poseBuffer; }

    public void AddSample(PoseData pose, float timestamp)
    {
        _poseBuffer.Add(
            new()
            {
                pose = pose,
                timestamp = timestamp
            }
        );

        _lastTimestamp = timestamp;

        if (_poseBuffer.Count > bufferSize)
        {
            _poseBuffer.RemoveAt(0);
        }
    }

    public PoseSample GetLastSample() => _poseBuffer[^1];
    public PoseSample GetPreviousSample() => _poseBuffer[^2];

    public int GetBufferSize() => _poseBuffer.Count;
    public int GetMaxBufferSize() => bufferSize;
    public bool IsBufferEmpty() => _poseBuffer.Count == 0;

    public float GetLastTimestamp() => _lastTimestamp;

    public float GetCompensationStep() => compensationStep;
    
    public static bool PoseEqualsApprox(in PoseData a, in PoseData b, float posEps = 0.0001f, float rotEps = 0.001f) =>
            Vector3.SqrMagnitude(a.lhandPos - b.lhandPos) < posEps * posEps && 
            Quaternion.Dot(a.lhandRot, b.lhandRot) > 1f - rotEps && 
            Vector3.SqrMagnitude(a.rhandPos - b.rhandPos) < posEps * posEps && 
            Quaternion.Dot(a.rhandRot, b.rhandRot) > 1f - rotEps;
}
