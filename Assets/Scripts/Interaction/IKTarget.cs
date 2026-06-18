using UnityEngine;

public class IKTarget : MonoBehaviour
{
    [SerializeField] 
    private Transform rootObject;

    [SerializeField]
    private Transform followObject;

    [SerializeField] 
    private Vector3 positionOffset;

    [SerializeField] 
    private Vector3 rotationOffset;

    [Header("Seated Spine Settings (Head Only)")]
    [SerializeField, Range(0.0f, 1.0f)] 
    private float leanInfluence = 0.45f;

    [SerializeField] 
    private float maxLeanDistance = 0.4f;

    private Vector3 _initialRootLocalPos;

    private void Start()
    {
        if (rootObject)
        {
            _initialRootLocalPos = rootObject.localPosition;
        }
    }

    private void LateUpdate()
    {
        if (followObject == null) 
        {
            return;
        }

        Vector3 pos = followObject.position + followObject.rotation * positionOffset;
        Quaternion rot = followObject.rotation * Quaternion.Euler(rotationOffset);
        transform.SetPositionAndRotation(pos, rot);

        if (rootObject)
        {
            Vector3 headHorizontalDist = transform.position - rootObject.position;
            headHorizontalDist.y = 0;
            Vector3 clampedLean = Vector3.ClampMagnitude(headHorizontalDist, maxLeanDistance);
            Vector3 targetRootWorldPos = 
                rootObject.parent.TransformPoint(_initialRootLocalPos) + (clampedLean * leanInfluence);
            
            targetRootWorldPos.y -= clampedLean.magnitude * 0.12f;
            rootObject.position = targetRootWorldPos;

            //Vector3 lookDir = Vector3.ProjectOnPlane(followObject.forward, Vector3.up).normalized;
            //if (lookDir != Vector3.zero)
            //{
            //    rootObject.forward = lookDir;
            //}
        }
    }
}
