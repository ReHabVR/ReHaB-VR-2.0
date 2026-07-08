using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(PlayerPoseBuffer))]
public class CompensationManager : MonoBehaviour
{
    [SerializeField]
    private PlayerPoseBuffer poseBuffer;

    private InterpolationCompensationMethod _interp;
    private ExtrapolationCompensationMethod _extrap;
    private DeadReckoningCompensationMethod _deadreckon;
    private KalmanCompensationMethod _kalman;

    private void Awake()
    {
        if (poseBuffer == null)
        {
            poseBuffer = GetComponent<PlayerPoseBuffer>();
        }
    }

    private void Start()
    {
        _interp = new(poseBuffer);
        _extrap = new(poseBuffer);
        _deadreckon = new(poseBuffer);
        _kalman = new();
    }

    public PoseData ApplyCompensation(PoseData networkPose, float renderTime)
    {
        ECompensationMode compensationMode = FusionSessionManager.Instance.CurrentCompensationMode;

        return compensationMode switch
        {
            ECompensationMode.Interpolation => _interp.Compensate(networkPose, renderTime),
            ECompensationMode.Extrapolation => _extrap.Compensate(networkPose, renderTime),
            ECompensationMode.DeadReckoning => _deadreckon.Compensate(networkPose, renderTime),
            ECompensationMode.KalmanFilter => _kalman.Compensate(networkPose, renderTime),
            _ => networkPose,
        };
    }
}
