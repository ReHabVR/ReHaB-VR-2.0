using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;

public class Head : MonoBehaviour
{

    [SerializeField] private Transform rootObject, followObject;
    [SerializeField] private Vector3 positionOffset, rotationOffset, headBodyOffset;

    private PhotonView photonView;

    public void Start()
    {
        this.followObject.gameObject.AddComponent<PhotonView>();
        photonView = PhotonView.Get(this);
        
        var isMine = photonView.IsMine; 
        var isMainCamera = this.followObject.gameObject.tag == "MainCamera"; 

        if (!isMine && isMainCamera) 
        {
            this.followObject.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (rootObject != null)
        {
            rootObject.position = transform.position + headBodyOffset;
            rootObject.forward = Vector3.ProjectOnPlane(followObject.up, Vector3.up).normalized;
        }

        if (photonView.IsMine == true) 
        {
            transform.position = followObject.TransformPoint(positionOffset);
            transform.rotation = followObject.rotation * Quaternion.Euler(rotationOffset);
        }
    }
}