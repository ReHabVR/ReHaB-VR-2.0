using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkHand : MonoBehaviour
{
    [HideInInspector]
    public PlayerRef Owner {get; set;}

    public EHandType handType = EHandType.Left;
}
