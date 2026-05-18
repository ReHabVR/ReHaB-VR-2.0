using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkHand : MonoBehaviour
{
    [SerializeField]
    private EHandType handType;

    public EHandType HandType => handType;
}
