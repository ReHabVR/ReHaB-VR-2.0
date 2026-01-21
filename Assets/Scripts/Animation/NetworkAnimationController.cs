using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public struct AnimationCommand
{
    public string ClipKey;
    public float NormalizedStart;
    public float Intensity;
}

[RequireComponent(typeof(Animator))]
public class NetworkAnimationController : NetworkBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField] 
    private AnimationCommandSource currentAdapter;

    [Networked] private int ClipHash { get; set; }
    private int lastPlayedHash;

    public override void Spawned()
    {
    }

    public override void FixedUpdateNetwork()
    {
        if (currentAdapter && currentAdapter.TryGetCommand(out var cmd))
        {
            Debug.LogError($"Command from adapter: {cmd.ClipKey}");
            RequestPlay(cmd.ClipKey);
        }
    }

    public override void Render()
    {
        if (ClipHash == 0 || ClipHash == lastPlayedHash)
        {
            return;
        }

        if (!animator.HasState(0, ClipHash))
        {
            Debug.LogWarning($"Animator missing state for hash {ClipHash}");
            return;
        }

        animator.Play(ClipHash, 0, 0f);
        lastPlayedHash = ClipHash;
    }

    public void RequestPlay(string clipName)
    {
        int hash = Animator.StringToHash(clipName);

        if (Object.HasStateAuthority)
        {
            ClipHash = hash;
        }
        else
        {
            RPC_RequestPlay(hash);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestPlay(int hash)
    {
        ClipHash = hash;
    }
}
