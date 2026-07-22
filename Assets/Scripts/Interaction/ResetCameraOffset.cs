using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResetCameraOffset : MonoBehaviour
{
    [SerializeField]
    private Transform cameraOffset;

    [SerializeField]
    private Transform cameraTransform;

    [SerializeField]
    private InputActionReference recenterAction;

    private void Start()
    {
        recenterAction.action.Enable();
    }

    private void OnDestroy()
    {
        recenterAction.action.Disable();
    }

    private void Update()
    {
        if (recenterAction.action.WasPressedThisFrame())
        {
            Vector3 cameraPosition = cameraTransform.localPosition;
            cameraPosition.y = 0f;
            cameraOffset.localPosition -= cameraPosition;

            float yaw = cameraTransform.localEulerAngles.y;
            cameraOffset.localRotation *= Quaternion.Euler(0f, -yaw, 0f);
        }
    }
}
