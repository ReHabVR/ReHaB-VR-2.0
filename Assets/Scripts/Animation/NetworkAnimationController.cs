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
[RequireComponent(typeof(PythonAdapter))]
[RequireComponent(typeof(PythonSocket))]
public class NetworkAnimationController : NetworkBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField] 
    private AnimationCommandSource currentAdapter;

    [Networked] 
    public int ClipHash { get; set; }

    [Networked] 
    public int PlayId { get; set; }

    private int _lastPlayedId;
    private int _cachedClipHash;
    private int _cachedPlayId;

    void Awake()
    {
        if (!TryGetComponent(out PythonAdapter adapter))
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
        if (!Object || !Object.IsValid || Runner == null || !Object.HasStateAuthority)
        {
            return;
        }

        if (currentAdapter && currentAdapter.TryGetCommand(out var cmd))
        {
            Debug.Log($"Command from adapter: {cmd.ClipKey}");
            RequestPlay(cmd.ClipKey);
        }

        if (!animator.HasState(0, ClipHash))
        {
            //Debug.LogWarning($"Animator missing state for hash {ClipHash}");
            return;
        }

        if (ClipHash == 0 || PlayId == _lastPlayedId)
        {
            return;
        }

        _cachedPlayId = PlayId;
        _cachedClipHash = ClipHash;
    }

    public override void Render()
    {
        if (!Object || !Object.IsValid || Runner == null)
           return;

        if (!animator || !animator.isActiveAndEnabled)
            return;

        if (_cachedClipHash == 0)
            return;

        if (_cachedPlayId == _lastPlayedId)
            return;

        if (!animator.HasState(0, _cachedClipHash))
            return;

        animator.Play(_cachedClipHash, 0, 0f);
        _lastPlayedId = _cachedPlayId;
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
        if (Object && Object.IsValid)
        {
            ClipHash = hash;
            PlayId++;
        }
    }
}
