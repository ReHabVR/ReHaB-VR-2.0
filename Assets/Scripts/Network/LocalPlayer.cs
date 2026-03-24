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
        #if CLIENT_PC
            xrOrigin.enabled = false;
            if (cameraOffset != null)
            {
                cameraOffset.localPosition = new(0, xrOrigin.CameraYOffset, 0);
            }
        #else
            xrOrigin.enabled = isLocal;
        #endif
        }

        foreach (XRBaseInteractor interactor in xrInteractors)
        {
        #if CLIENT_PC
            interactor.enabled = false;
        #else
            interactor.enabled = isLocal;
        #endif
        } 

        foreach (XRBaseController controller in xrControllers)
        {
        #if CLIENT_PC
            controller.enabled = false;
        #else
            controller.enabled = isLocal;
        #endif
        }
    }
}
