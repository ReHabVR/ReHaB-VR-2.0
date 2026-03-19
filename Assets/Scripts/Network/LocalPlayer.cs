using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Unity.XR.CoreUtils;

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
    #if CLIENT_PC
        if (xrOrigin != null)
        {
            xrOrigin.enabled = false;
            if (cameraOffset != null && !isLocal)
            {
                cameraOffset.position = new(0, xrOrigin.CameraYOffset, 0);
            }
        }
    #endif
   }
}
