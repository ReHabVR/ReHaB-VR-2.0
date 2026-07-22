using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadReckoningCompensationMethod : ICompensationMethod
{
    private readonly PlayerPoseBuffer _poseBuffer;

    private PoseSample _lastProcessedSample;
    private PoseData _estimatedPose;
    
    private Vector3 _headVel;
    private Vector3 _lHandVel;
    private Vector3 _rHandVel;

    private float _lastEstimationTime = -1.0f;

    public DeadReckoningCompensationMethod(PlayerPoseBuffer poseBuffer)
    {
        _poseBuffer = poseBuffer;
        _lastProcessedSample = _poseBuffer.GetLastSample();
    }

    public PoseData Compensate(PoseData networkPose, float renderTime)
    {
        PoseSample latestSample = _poseBuffer.GetLastSample();
        if (_poseBuffer.Samples.Count < 2)
        {
            UpdateState(latestSample);
            return networkPose;
        }

        // Check if new networked sample has arrived (was added to buffer)
        if (!Mathf.Approximately(_lastProcessedSample.timestamp, latestSample.timestamp))
        {
            _headVel = PoseMathHelpers.CalculateVelocity(
                latestSample.pose.headPos, _lastProcessedSample.pose.headPos, 
                latestSample.timestamp, _lastProcessedSample.timestamp
            );
            _lHandVel = PoseMathHelpers.CalculateVelocity(
                latestSample.pose.lhandPos, _lastProcessedSample.pose.lhandPos, 
                latestSample.timestamp, _lastProcessedSample.timestamp
            );
            _rHandVel = PoseMathHelpers.CalculateVelocity(
                latestSample.pose.rhandPos, _lastProcessedSample.pose.rhandPos, 
                latestSample.timestamp, _lastProcessedSample.timestamp
            );
            
            // TODO: calculate angular velocity
            UpdateState(latestSample);
        }

        // Update estimation
        float dt = Mathf.Clamp(renderTime - _lastEstimationTime, 0.0f, 0.2f);
        _estimatedPose.headPos += _headVel * dt;
        _estimatedPose.lhandPos += _lHandVel * dt;
        _estimatedPose.rhandPos += _rHandVel * dt;

        _lastEstimationTime = renderTime;
        return _estimatedPose;
    }

    public void Reset()
    {
        _lastProcessedSample = default;
        _lastEstimationTime = -1.0f;
        _estimatedPose = default;
    }

    private void UpdateState(PoseSample sample)
    {
        _lastProcessedSample = sample;
        _lastEstimationTime = sample.timestamp;
        _estimatedPose = sample.pose;
    }
}
