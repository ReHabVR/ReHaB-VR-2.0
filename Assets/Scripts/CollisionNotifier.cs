using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class CollisionNotifier : MonoBehaviour
{
    public int colliderId;
    public UnityEvent<GameObject, int> collisionDetected;
    
    void OnTriggerEnter(Collider collider)
    {
        GameObject go = collider.gameObject;

        // Detect only Grabbables
        if (go.layer != LayerMask.NameToLayer("Grabbable"))
        {
            return;
        }
        
        // Flip flag to avoid accidental detections
        SingleTrigger trigger = go.GetComponent<SingleTrigger>();
        if (trigger.Triggered)
        {
            return;
        }

        trigger.Triggered = true;
        collisionDetected?.Invoke(go, colliderId);
    }

    void OnTriggerExit(Collider collider)
    {
        GameObject go = collider.gameObject;
        if (go.layer != LayerMask.NameToLayer("Grabbable"))
        {
            return;
        }
        go.GetComponent<SingleTrigger>().Triggered = false; 
    }
}
