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

    private readonly WaitForSeconds _waitForThreeSeconds = new(3);

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
            bool xrRunning = IsXRRunning();

            if (_poseDriver.enabled != xrRunning)
            {
                _poseDriver.enabled = xrRunning;
            }

            yield return _waitForThreeSeconds;
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
