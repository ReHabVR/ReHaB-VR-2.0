using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SpatialTracking;

[RequireComponent(typeof(TrackedPoseDriver))]
public class PoseDriverToggle : MonoBehaviour
{
    [SerializeField]
    private TrackedPoseDriver _poseDriver;

    private readonly WaitForSeconds _waitASecond = new(1);

    private void Awake()
    {
        if (_poseDriver == null)
        {
            _poseDriver = GetComponent<TrackedPoseDriver>();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(QueryXRState());
    }

    private IEnumerator QueryXRState()
    {
        while (true)
        {
            _poseDriver.enabled = IsXRRunning();
            yield return _waitASecond;
        }
    }

    private bool IsXRRunning()
    {
        List<XRInputSubsystem> subsystems = new();
        SubsystemManager.GetSubsystems(subsystems);
        for (int i = 0; i < subsystems.Count; i++)
        {
            if (subsystems[i].running)
            {
                return true;
            }
        }

        return false;
    }
}
