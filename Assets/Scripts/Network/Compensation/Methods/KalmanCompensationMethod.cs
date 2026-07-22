using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class KalmanCompensationMethod : ICompensationMethod
{
    private readonly PoseKalmanFilter _headPosFilter = new();
    private readonly PoseKalmanFilter _lhandPosFilter = new();
    private readonly PoseKalmanFilter _rhandPosFilter = new();

    private readonly PoseKalmanFilter _headRotFilter = new();
    private readonly PoseKalmanFilter _lhandRotFilter = new();
    private readonly PoseKalmanFilter _rhandRotFilter = new();

    private float _lastRenderTime = -1f;

    public PoseData Compensate(PoseData networkPose, float renderTime)
    {
        if (_lastRenderTime < 0f)
        {
            _lastRenderTime = renderTime;
            return networkPose;
        }

        float dt = Mathf.Max(renderTime - _lastRenderTime, 0.001f);
        _lastRenderTime = renderTime;

        // Convert rotation quaternion to rotation vector
        Vector3 headRotVector = PoseMathHelpers.QuaternionToRotationVector(networkPose.headRot);
        Vector3 lHandRotVector = PoseMathHelpers.QuaternionToRotationVector(networkPose.lhandRot);
        Vector3 rHandRotVector = PoseMathHelpers.QuaternionToRotationVector(networkPose.rhandRot);

        return new()
        {
            headPos = _headPosFilter.Update(networkPose.headPos, dt), 
            headRot = PoseMathHelpers.RotationVectorToQuaternion(_headRotFilter.Update(headRotVector, dt)),

            lhandPos = _lhandPosFilter.Update(networkPose.lhandPos, dt), 
            lhandRot = PoseMathHelpers.RotationVectorToQuaternion(_lhandRotFilter.Update(lHandRotVector, dt)),

            rhandPos = _rhandPosFilter.Update(networkPose.rhandPos, dt), 
            rhandRot = PoseMathHelpers.RotationVectorToQuaternion(_rhandRotFilter.Update(rHandRotVector, dt)),

            gripL = networkPose.gripL,
            gripR = networkPose.gripR
        };
    }
}
