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

    void OnGrab(SelectEnterEventArgs args)
    {
        // Prevent grabs by other player if object is held
        if (HoldingPlayer != PlayerRef.None && HoldingPlayer != Runner.LocalPlayer)
        {
            IXRSelectInteractable interactable = grab;
            grab.interactionManager.CancelInteractableSelection(interactable);
            return;
        }

        HoldingPlayer = Runner.LocalPlayer;
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (HoldingPlayer == Runner.LocalPlayer)
        {
            HoldingPlayer = PlayerRef.None;
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
