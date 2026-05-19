using Fusion;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class GrabbableObject : NetworkBehaviour
{
    [Networked, HideInInspector]
    public PlayerRef HoldingPlayer { get; private set; } = PlayerRef.None;

    [Networked, HideInInspector]
    public EHandType HoldingHand { get; private set; }

    [SerializeField]
    private Rigidbody rb;
    
    [SerializeField]
    private XRGrabInteractable grab;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (grab == null)
            grab = GetComponent<XRGrabInteractable>();

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
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

            if (!isHeld)
            {
                return;
            }

            if (HandPoseResolver.Instance.TryGetHandPose(HoldingPlayer, HoldingHand, out Vector3 pos, out Quaternion rot))
            {
                Debug.Log($"[SERVER] Resolved hand pos: {pos}");
                rb.MovePosition(pos);
                rb.MoveRotation(rot);
            }
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        XRHand hand = args.interactorObject.transform.GetComponentInParent<XRHand>();
        if (!hand)
        {
            return;
        }

        NetworkObject playerNO = hand.GetComponentInParent<NetworkObject>();
        if (playerNO)
        {
            RPC_RequestGrab(playerNO.InputAuthority, hand.HandType);
        }
    }

    void OnRelease(SelectExitEventArgs args)
    {
        RPC_RequestRelease();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestGrab(PlayerRef player, EHandType hand)
    {
        Debug.Log($"Grab requested by {player}");
        if (HoldingPlayer == PlayerRef.None || HoldingPlayer == player)
        {
            HoldingPlayer = player;
            HoldingHand = hand;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_RequestRelease()
    {
        Debug.Log($"Release requested by {HoldingPlayer}");
        HoldingPlayer = PlayerRef.None;
    }
}
