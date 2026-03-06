using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class XRTrackingSetup : MonoBehaviour
{
    [SerializeField]
    private TrackingOriginModeFlags trackingMode = TrackingOriginModeFlags.Floor;

    [SerializeField]
    private Transform cameraOffset;

    private static readonly WaitForSeconds _waitHalfSecond = new(0.5f);

    IEnumerator Start()
    {
        yield return _waitHalfSecond;

        List<XRInputSubsystem> subsystems = new();
        SubsystemManager.GetSubsystems(subsystems);

        bool xrRunning = subsystems.Exists(s => s.running);
        if (!xrRunning)
        {
            // No XR subsystem, assume PC build
            yield break;
        }

        XRLoader loader = XRGeneralSettings.Instance.Manager.activeLoader;
        if (loader == null)
        {
            Debug.LogError("Failed to get XRLoader!");
            yield break;
        }

        XRInputSubsystem xrInput = loader.GetLoadedSubsystem<XRInputSubsystem>();
        if (xrInput == null)
        {
            Debug.LogError("Failed to get XRInputSubsystem!");
            yield break;
        }

        xrInput.TrySetTrackingOriginMode(trackingMode);
        //cameraOffset.localPosition = Vector3.zero;
    }
}
