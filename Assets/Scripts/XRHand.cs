using UnityEngine;

public class XRHand : MonoBehaviour
{
    [SerializeField]
    private EHandType handType;

    [SerializeField]
    private Transform attachPoint;

    public EHandType HandType => handType;

    public Transform AttachPoint => attachPoint;
}
