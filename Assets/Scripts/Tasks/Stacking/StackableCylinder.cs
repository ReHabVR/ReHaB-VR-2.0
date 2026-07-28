using System.Collections.Generic;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(GrabbableObject))]
[RequireComponent(typeof(Rigidbody))]
public class StackableCylinder : NetworkBehaviour
{
    public GrabbableObject grab;
    public Rigidbody rb;
    public Collider col;

    private readonly HashSet<Collider> _contacts = new();

    private void Awake()
    {
        if (grab == null)
        {
            grab = GetComponent<GrabbableObject>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (col == null)
        {
            col = GetComponent<Collider>();
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        _contacts.Add(collision.collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        _contacts.Remove(collision.collider);
    }

    public bool IsTouching(StackableCylinder other)
    {
        return _contacts.Contains(other.col);
    }
}
