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

        return new()
        {
            headPos = _headPosFilter.Update(networkPose.headPos, dt), 
            headRot = Quaternion.Euler(
                _headRotFilter.Update(networkPose.headRot.eulerAngles, dt)),
            lhandPos = _lhandPosFilter.Update(networkPose.lhandPos, dt), 
            lhandRot = Quaternion.Euler(
                _lhandRotFilter.Update(networkPose.lhandRot.eulerAngles, dt)),
            rhandPos = _rhandPosFilter.Update(networkPose.rhandPos, dt), 
            rhandRot = Quaternion.Euler(
                _rhandRotFilter.Update(networkPose.rhandRot.eulerAngles, dt)),
            gripL = networkPose.gripL,
            gripR = networkPose.gripR
        };
    }
}
