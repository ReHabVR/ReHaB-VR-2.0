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

    [Networked] 
    public string ClipName { get; set; }

    [Networked] 
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

    public override void FixedUpdateNetwork()
    {
        if (!Object || !Object.IsValid || Runner == null)
        {
            return;
        }

        if (Object.HasStateAuthority)
        {
            if (currentAdapter && currentAdapter.TryGetCommand(out var cmd))
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

        //animator.Play(_cachedClipHash, DEFAULT_LAYER, 0.0f);
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
}
