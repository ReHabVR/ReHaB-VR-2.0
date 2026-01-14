using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class LocalPlayer : NetworkBehaviour
{
    public static PlayerRole LocalRole;

    [Networked]
    public PlayerRole NetworkRole { get; private set; }

    [SerializeField] 
    private Camera playerCamera;
    [SerializeField] 
    private AudioListener audioListener;

    public override void Spawned()
    {
        bool isLocal = HasInputAuthority;

        if (playerCamera != null)
            playerCamera.enabled = isLocal;

        if (audioListener != null)
            audioListener.enabled = isLocal;

        if (isLocal)
        {
            RPC_SetRole(LocalRole);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetRole(PlayerRole role)
    {
        NetworkRole = role;
    }
}
