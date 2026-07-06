using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(PlayerPoseBuffer))]
public class CompensationManager : MonoBehaviour
{
    [SerializeField]
    private PlayerPoseBuffer poseBuffer;

    private InterpolationCompensationMethod _interpolation;
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
        _interpolation = new(poseBuffer);
        _kalman = new(poseBuffer);
    }

    public PoseData ApplyCompensation(PoseData networkPose)
    {
        ECompensationMode compensationMode = FusionSessionManager.Instance.CurrentCompensationMode;

        return compensationMode switch
        {
            ECompensationMode.Interpolation => _interpolation.Compensate(networkPose),
            ECompensationMode.KalmanFilter => _kalman.Compensate(networkPose),
            _ => networkPose,
        };
    }
}
