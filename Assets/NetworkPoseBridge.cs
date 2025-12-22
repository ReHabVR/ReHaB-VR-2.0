using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkPoseBridge : NetworkBehaviour
{
    [SerializeField] 
    private Transform XRHead;
    [SerializeField] 
    private Transform XRLeftHand;
    [SerializeField] 
    private Transform XRRightHand;

    [SerializeField] 
    private Transform bridgeHead;
    [SerializeField] 
    private Transform bridgeLeftHand;
    [SerializeField] 
    private Transform bridgeRightHand;

    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority)
        {
            bridgeHead.SetPositionAndRotation(
                XRHead.position,
                XRHead.rotation
            );

            bridgeLeftHand.SetPositionAndRotation(
                XRLeftHand.position,
                XRLeftHand.rotation
            );

            bridgeRightHand.SetPositionAndRotation(
                XRRightHand.position,
                XRRightHand.rotation
            );
        }
    }
}
