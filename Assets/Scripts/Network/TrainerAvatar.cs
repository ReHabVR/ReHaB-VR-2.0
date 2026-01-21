using Fusion;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NetworkAnimationController))]
public class TrainerAvatar : NetworkBehaviour
{
    public PlayerRef OwnerPlayer { get; private set; }

    public void Initialize(PlayerRef owner)
    {
        OwnerPlayer = owner;
    }

    public override void Spawned()
    {
    }
}