 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
    
    [SerializeField] 
    private Vector3 bodyOffset;

    void LateUpdate()
    {
        Vector3 pos = followObject.position + followObject.rotation * positionOffset;
        Quaternion rot = followObject.rotation * Quaternion.Euler(rotationOffset);
        transform.SetPositionAndRotation(pos, rot);

        if (rootObject != null)
        {
            rootObject.position = transform.position + bodyOffset;
            rootObject.forward = Vector3.ProjectOnPlane(followObject.up, Vector3.up).normalized;
        }
    }
}
