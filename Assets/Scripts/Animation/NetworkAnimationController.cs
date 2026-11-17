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

[RequireComponent(typeof(PythonAdapter))]
[RequireComponent(typeof(PythonSocket))]
public class NetworkAnimationController : NetworkBehaviour
{
    const int DEFAULT_LAYER = 0;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private AnimationCommandSource currentAdapter;

    [Networked, HideInInspector]
    public string ClipName { get; set; }

    [Networked, HideInInspector] 
    public int PlayId { get; set; }

    private int _lastPlayedId = -1;

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

    private void OnGUI()
    {
        if (!Object || !Object.IsValid || !HasInputAuthority)
        {
            return;
        }

        GUILayout.BeginArea(new Rect(20, 20, 200, 200));

        GUILayout.Label("Animations");
        if (GUILayout.Button("Movement"))
        {
            RPC_RequestPlay("Movement");
        }

        if (GUILayout.Button("Rest"))
        {
            RPC_RequestPlay("Rest");
        }

        if (GUILayout.Button("Idle"))
        {
            RPC_RequestPlay("Idle");
        }

        GUILayout.EndArea();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object || !Object.IsValid || Runner == null)
        {
            return;
        }

        if (Object.HasStateAuthority)
        {
            if (currentAdapter && currentAdapter.TryGetCommand(out AnimationCommand cmd))
            {
                Debug.Log($"Command from adapter: {cmd.ClipKey}");
                RequestPlay(cmd.ClipKey);
            }
        }
    }

    public override void Render()
    {
        if (!Object || !Object.IsValid || Runner == null)
           return;

        if (!animator || !animator.isActiveAndEnabled)
            return;

        if (PlayId == _lastPlayedId)
            return;

        if (string.IsNullOrEmpty(ClipName))
            return;

        animator.Play(ClipName, DEFAULT_LAYER, 0.0f);
        _lastPlayedId = PlayId;
    }

    public void RequestPlay(string clipName)
    {
        if (Object.HasStateAuthority)
        {
            ClipName = clipName;
            PlayId++;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestPlay(string clipName)
    {
        RequestPlay(clipName);
    }
}
