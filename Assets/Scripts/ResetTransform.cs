using UnityEngine;

/// Because Fusion apparently interprets default transform as "world origin",
/// this script is meant to adjust GameObject's transform to match its parent.

public class ResetTransform : MonoBehaviour
{
    [SerializeField, Tooltip("Defaults to parent object if left unassigned.")]
    private Transform target;

    private void Awake()
    {
        if (target == null)
        {
            target = transform.parent;
        }

        if (target)
        {
            if (transform.parent == target)
            {
                gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            else
            {
                gameObject.transform.SetPositionAndRotation(target.position, target.rotation);
            }
        }
        else
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}
