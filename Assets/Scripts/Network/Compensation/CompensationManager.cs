using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerPoseBuffer))]
public class CompensationManager : MonoBehaviour
{
    [SerializeField]
    private PlayerPoseBuffer poseBuffer;

    private InterpolationCompensationMethod _interpolation;

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
    }

    public PoseData ApplyCompensation(PoseData networkPose)
    {
        ECompensationMode compensationMode = FusionSessionManager.Instance.CurrentCompensationMode;
        float timestamp = poseBuffer.GetLastTimestamp();

        return compensationMode switch
        {
            ECompensationMode.Interpolation => _interpolation.Compensate(networkPose),
            //ECompensationMode.KalmanFilter => _kalman.Compensate(networkPose),
            _ => networkPose,
        };
    }
}
