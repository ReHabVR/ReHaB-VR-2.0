using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class LocalPlayer : NetworkBehaviour
{
    [SerializeField] 
    private Camera playerCamera;

    [SerializeField] 
    private AudioListener audioListener;

    public override void Spawned()
    {
        bool isLocal = Object.InputAuthority == Runner.LocalPlayer;

        if (playerCamera != null)
        {
            playerCamera.enabled = isLocal;
        }

        if (audioListener != null)
        {
            audioListener.enabled = isLocal;
        }
    }
}
