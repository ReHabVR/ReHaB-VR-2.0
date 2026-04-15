using Fusion;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class GrabbableObject : NetworkBehaviour
{
    [Networked, HideInInspector]
    public PlayerRef HoldingPlayer { get; private set; } = PlayerRef.None;

    private Rigidbody rb;
    private XRGrabInteractable grab;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            // Apply correct physics state based on current network state
            bool isHeld = HoldingPlayer != PlayerRef.None;
            rb.isKinematic = isHeld;
            rb.useGravity = !isHeld;
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Prevent grabs by other player if object is held
        if (HoldingPlayer != PlayerRef.None && HoldingPlayer != Runner.LocalPlayer)
        {
            IXRSelectInteractable interactable = grab;
            grab.interactionManager.CancelInteractableSelection(interactable);
            return;
        }

        if (!Object.HasStateAuthority)
        {
            Object.RequestStateAuthority();
        }

        RPC_RequestGrab(Runner.LocalPlayer);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (HoldingPlayer == Runner.LocalPlayer)
        {
            RPC_RequestRelease();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestGrab(PlayerRef player)
    {
        if (HoldingPlayer == PlayerRef.None || HoldingPlayer == player)
        {
            HoldingPlayer = player;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestRelease()
    {
        HoldingPlayer = PlayerRef.None;
        Object.ReleaseStateAuthority();
    }
}
