using System;
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
}
