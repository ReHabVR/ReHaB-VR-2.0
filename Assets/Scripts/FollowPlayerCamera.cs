using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerCamera : MonoBehaviour
{

    public GameObject cameraOffset;
    private GameObject _playerToFollow;
    private GameObject _cameraOffsetToFollow;
    
    void Start()
    {
        cameraOffset.transform.GetChild(0).gameObject.transform.position = new Vector3(0, 0, 0);
        _cameraOffsetToFollow = GameObject
                                    .Find("XRRig(Clone)")
                                    .transform.GetChild(0).gameObject // avatar
                                    .transform.GetChild(0).gameObject // armature
                                    .transform.GetChild(1).gameObject // character rig
                                    .transform.GetChild(2).gameObject // ik head
                                    .transform.GetChild(0).gameObject; // ik head target
    }

    // Update is called once per frame
    void Update()
    {
        if (_cameraOffsetToFollow != null) {
            cameraOffset.transform.position = _cameraOffsetToFollow.transform.position; 
            cameraOffset.transform.rotation = _cameraOffsetToFollow.transform.rotation; 
        }
    }
}
