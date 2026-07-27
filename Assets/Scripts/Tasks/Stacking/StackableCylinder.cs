using Fusion;
using UnityEngine;

[RequireComponent(typeof(GrabbableObject))]
[RequireComponent(typeof(Rigidbody))]
public class StackableCylinder : NetworkBehaviour
{
    public GrabbableObject grab;
    public Rigidbody rb;

    void Awake()
    {
        if (grab == null)
        {
            grab = GetComponent<GrabbableObject>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }
}
