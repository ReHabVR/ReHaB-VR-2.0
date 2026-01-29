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

    [Networked] 
    private int ClipHash { get; set; }
    [Networked] 
    private int PlayId { get; set; }

    private int _lastPlayedId;

    void Awake()
    {
        PythonAdapter adapter = GetComponent<PythonAdapter>();
        if (adapter == null)
        {
            return;
        }

        PythonSocket socket = GetComponent<PythonSocket>();
        adapter.Bind(socket);
    }

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

    public void LateUpdate()
    {
        if (PlayId == _lastPlayedId || ClipHash == 0)
        {
            return;
        }

        if (!animator.HasState(0, ClipHash))
        {
            Debug.LogWarning($"Animator missing state for hash {ClipHash}");
            return;
        }

        animator.Play(ClipHash, 0, 0f);
        _lastPlayedId = PlayId;
    }

    public void RequestPlay(string clipName)
    {
        int hash = Animator.StringToHash(clipName);
        if (Object.HasStateAuthority)
        {
            ClipHash = hash;
            PlayId++;
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
        PlayId++;
    }
}
