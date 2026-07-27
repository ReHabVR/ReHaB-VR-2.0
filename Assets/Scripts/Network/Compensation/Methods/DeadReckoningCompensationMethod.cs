using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadReckoningCompensationMethod : ICompensationMethod
{
    private const float BLEND_DURATION = 0.1f;

    private readonly PlayerPoseBuffer _poseBuffer;

    private PoseSample _lastProcessedSample;

    private PoseData _renderedPose; // final estimated pose that is actually rendered
    private PoseData _predictedPose; // latest state received from the network
    private PoseData _blendPose; // pose being displayed until new update arrives
    
    private Vector3 _headVel;
    private Vector3 _lHandVel;
    private Vector3 _rHandVel;

    private Vector3 _headAngVel;
    private Vector3 _lHandAngVel;
    private Vector3 _rHandAngVel;

    private float _lastEstimationTime = -1f;
    private float _blendTimer = 0.0f;
    private bool _isBlending = false;

    public DeadReckoningCompensationMethod(PlayerPoseBuffer poseBuffer)
    {
        _poseBuffer = poseBuffer;
        Reset();
    }

    public PoseData Compensate(PoseData networkPose, float renderTime)
    {
        if (_poseBuffer.Samples.Count == 0)
        {
            return networkPose;
        }

        PoseSample latestSample = _poseBuffer.GetLastSample();
        if (_poseBuffer.Samples.Count < 2)
        {
            Initialize(latestSample);
            return latestSample.pose;
        }

        if (_lastEstimationTime < 0.0f)
        {
            _lastEstimationTime = renderTime;
            Initialize(latestSample);
            return _renderedPose;
        }

        float deltaRenderTime = Mathf.Max(renderTime - _lastEstimationTime, 0.001f);
        _lastEstimationTime = renderTime;

        // Check if new networked sample has arrived (was added to buffer)
        if (!Mathf.Approximately(_lastProcessedSample.timestamp, latestSample.timestamp))
        {
            _blendPose = _renderedPose;
            float dtNet = latestSample.timestamp - _lastProcessedSample.timestamp;
            if (dtNet > 0.0f)
            {
                _headVel = (latestSample.pose.headPos - _lastProcessedSample.pose.headPos) / dtNet;
                _lHandVel = (latestSample.pose.lhandPos - _lastProcessedSample.pose.lhandPos) / dtNet;
                _rHandVel = (latestSample.pose.rhandPos - _lastProcessedSample.pose.rhandPos) / dtNet;

                _headAngVel = PoseMathHelpers.CalculateAngularVelocity(
                    _lastProcessedSample.pose.headRot, latestSample.pose.headRot, dtNet);
                _lHandAngVel = PoseMathHelpers.CalculateAngularVelocity(
                    _lastProcessedSample.pose.lhandRot, latestSample.pose.lhandRot, dtNet);
                _rHandAngVel = PoseMathHelpers.CalculateAngularVelocity(
                    _lastProcessedSample.pose.rhandRot, latestSample.pose.rhandRot, dtNet);
            }

            _predictedPose = latestSample.pose;
            _lastProcessedSample = latestSample;
            _blendTimer = 0f;
            _isBlending = true;
        }

        ExtrapolatePose(ref _predictedPose, deltaRenderTime);
        if (_isBlending)
        {
            _blendTimer += deltaRenderTime;
            float t = Mathf.Clamp01(_blendTimer / BLEND_DURATION);
            if (t >= 1.0f)
            {
                _isBlending = false;
                _renderedPose = _predictedPose;
            }
            else
            {
                ExtrapolatePose(ref _blendPose, deltaRenderTime);

                _renderedPose.headPos = Vector3.Lerp(_blendPose.headPos, _predictedPose.headPos, t);
                _renderedPose.lhandPos = Vector3.Lerp(_blendPose.lhandPos, _predictedPose.lhandPos, t);
                _renderedPose.rhandPos = Vector3.Lerp(_blendPose.rhandPos, _predictedPose.rhandPos, t);

                _renderedPose.headRot = Quaternion.Slerp(_blendPose.headRot, _predictedPose.headRot, t);
                _renderedPose.lhandRot = Quaternion.Slerp(_blendPose.lhandRot, _predictedPose.lhandRot, t);
                _renderedPose.rhandRot = Quaternion.Slerp(_blendPose.rhandRot, _predictedPose.rhandRot, t);
                
                _renderedPose.gripL = Mathf.Lerp(_blendPose.gripL, _predictedPose.gripL, t);
                _renderedPose.gripR = Mathf.Lerp(_blendPose.gripR, _predictedPose.gripR, t);
            }
        }
        else
        {
            _renderedPose = _predictedPose;
        }

        return _renderedPose;
    }

    private void Initialize(PoseSample sample)
    {
        _lastProcessedSample = sample;
        _renderedPose = sample.pose;
        _predictedPose = sample.pose;
        _blendPose = sample.pose;
    }

    private void ExtrapolatePose(ref PoseData pose, float dt)
    {
        pose.headPos += _headVel * dt;
        pose.lhandPos += _lHandVel * dt;
        pose.rhandPos += _rHandVel * dt;

        pose.headRot = ApplyAngularVelocity(pose.headRot, _headAngVel, dt);
        pose.lhandRot = ApplyAngularVelocity(pose.lhandRot, _lHandAngVel, dt);
        pose.rhandRot = ApplyAngularVelocity(pose.rhandRot, _rHandAngVel, dt);
    }

    private Quaternion ApplyAngularVelocity(Quaternion currentRot, Vector3 angVel, float dt)
    {
        float angleRad = angVel.magnitude * dt;
        if (Mathf.Approximately(angleRad, 0f))
        {
            return currentRot;
        }

        Quaternion delta = Quaternion.AngleAxis(angleRad * Mathf.Rad2Deg, angVel.normalized);
        return delta * currentRot;
    }

    public void Reset()
    {
        _lastProcessedSample = default;

        _renderedPose = default;
        _predictedPose = default;
        _blendPose = default;

        _lastEstimationTime = -1f;
        _isBlending = false;

        _headVel = Vector3.zero;
        _lHandVel = Vector3.zero;
        _rHandVel = Vector3.zero;
        _headAngVel = Vector3.zero;
        _lHandAngVel = Vector3.zero;
        _rHandAngVel = Vector3.zero;
    }
}
