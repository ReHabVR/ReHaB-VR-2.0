using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;

public class LocalPlayer : NetworkBehaviour
{
    [SerializeField] 
    private Camera playerCamera;

    [SerializeField] 
    private AudioListener audioListener;

    [SerializeField]
    private XROrigin xrOrigin;

    [SerializeField]
    private Transform cameraOffset;

    [SerializeField]
    private List<XRBaseInteractor> xrInteractors;

    [SerializeField]
    private List<XRBaseController> xrControllers;

    public override void Spawned()
    {
        bool isLocal = HasInputAuthority;

        if (playerCamera != null)
        {
            playerCamera.enabled = isLocal;
        }

        if (audioListener != null)
        {
            audioListener.enabled = isLocal;
        }

        if (xrOrigin != null)
        {
            xrOrigin.enabled = isLocal;
        }

        if (cameraOffset != null && isLocal == false)
        {
            cameraOffset.position = new(0, xrOrigin.CameraYOffset, 0);
        }

        foreach (XRBaseInteractor interactor in xrInteractors)
        {
            interactor.enabled = isLocal;
        } 

        foreach (XRBaseController controller in xrControllers)
        {
            controller.enabled = isLocal;
        }
    }
}
