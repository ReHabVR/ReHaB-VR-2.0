using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkHand : MonoBehaviour
{
    [Networked]
    public PlayerRef Owner { get; set; }

    [SerializeField]
    private EHandType handType;

    public EHandType HandType => handType;
}
