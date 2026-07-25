using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class KalmanCompensationMethod : ICompensationMethod
{
    private readonly PlayerPoseBuffer _poseBuffer;
    private PoseSample _lastProcessedSample;

    private readonly PoseKalmanFilter _headPosFilter = new();
    private readonly PoseKalmanFilter _lhandPosFilter = new();
    private readonly PoseKalmanFilter _rhandPosFilter = new();

    private readonly PoseKalmanFilter _headRotFilter = new();
    private readonly PoseKalmanFilter _lhandRotFilter = new();
    private readonly PoseKalmanFilter _rhandRotFilter = new();

    private float _lastRenderTime = -1f;
    private bool _initialized = false;

    public KalmanCompensationMethod(PlayerPoseBuffer poseBuffer)
    {
        _poseBuffer = poseBuffer;
    }

    public PoseData Compensate(PoseData networkPose, float renderTime)
    {
        PoseSample latestSample = _poseBuffer.GetLastSample();
        if (_poseBuffer.Samples.Count < 2)
        {
            return networkPose;
        }

        if (!_initialized || _lastRenderTime < 0f)
        {
            _lastRenderTime = renderTime;
            _lastProcessedSample = latestSample;
            Initialize(latestSample.pose);
            _initialized = true;
            return latestSample.pose;
        }

        float deltaRenderTime = Mathf.Max(renderTime - _lastRenderTime, 0.001f);
        _lastRenderTime = renderTime;

        // Correct when new sample arrives
        if (!Mathf.Approximately(_lastProcessedSample.timestamp, latestSample.timestamp))
        {
            Vector3 headRotVector = PoseMathHelpers.QuaternionToRotationVector(latestSample.pose.headRot);
            Vector3 lHandRotVector = PoseMathHelpers.QuaternionToRotationVector(latestSample.pose.lhandRot);
            Vector3 rHandRotVector = PoseMathHelpers.QuaternionToRotationVector(latestSample.pose.rhandRot);

            _headPosFilter.Correct(latestSample.pose.headPos);
            _headRotFilter.Correct(headRotVector);

            _lhandPosFilter.Correct(latestSample.pose.lhandPos);
            _lhandRotFilter.Correct(lHandRotVector);

            _rhandPosFilter.Correct(latestSample.pose.rhandPos);
            _rhandRotFilter.Correct(rHandRotVector);

            _lastProcessedSample = latestSample;
        }

        // Predict
        _headPosFilter.Predict(deltaRenderTime);
        _headRotFilter.Predict(deltaRenderTime);

        _lhandPosFilter.Predict(deltaRenderTime);
        _lhandRotFilter.Predict(deltaRenderTime);

        _rhandPosFilter.Predict(deltaRenderTime);
        _rhandRotFilter.Predict(deltaRenderTime);

        return new()
        {
            headPos = _headPosFilter.GetPosition(),
            headRot = PoseMathHelpers.RotationVectorToQuaternion(_headRotFilter.GetPosition()),

            lhandPos = _lhandPosFilter.GetPosition(),
            lhandRot = PoseMathHelpers.RotationVectorToQuaternion(_lhandRotFilter.GetPosition()),

            rhandPos = _rhandPosFilter.GetPosition(),
            rhandRot = PoseMathHelpers.RotationVectorToQuaternion(_rhandRotFilter.GetPosition()),

            gripL = latestSample.pose.gripL,
            gripR = latestSample.pose.gripR
        };
    }

    private void Initialize(PoseData startPose)
    {
        _headPosFilter.Reset(startPose.headPos);
        _lhandPosFilter.Reset(startPose.lhandPos);
        _rhandPosFilter.Reset(startPose.rhandPos);

        _headRotFilter.Reset(PoseMathHelpers.QuaternionToRotationVector(startPose.headRot));
        _lhandRotFilter.Reset(PoseMathHelpers.QuaternionToRotationVector(startPose.lhandRot));
        _rhandRotFilter.Reset(PoseMathHelpers.QuaternionToRotationVector(startPose.rhandRot));
    }

    public void Reset()
    {
        _lastRenderTime = -1f;
        _initialized = false;
        _lastProcessedSample = default;
    }
}
