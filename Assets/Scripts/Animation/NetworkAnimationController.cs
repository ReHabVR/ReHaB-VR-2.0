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

    [Networked] 
    private int ClipHash { get; set; }

    int lastPlayedHash;

    public override void Spawned()
    {
        // TODO: replace with controller logic
        if (Object.HasInputAuthority)
        {
            ClipHash = Animator.StringToHash("TestAnimation");
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

        animator.SetTrigger("PlayTestAnim"); // Temporary workaround
        animator.Play(ClipHash, 0, 0f);
        lastPlayedHash = ClipHash;
    }
}
