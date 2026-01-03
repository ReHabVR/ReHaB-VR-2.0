using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugPoseProvider : MonoBehaviour
{
    [Header("Bridge Transforms")]
    [SerializeField] 
    private Transform lhandBridge;
    [SerializeField] 
    private Transform rhandBridge;

    [Header("Fallback Transforms")]
    [SerializeField] 
    private Transform lhandFallback;
    [SerializeField] 
    private Transform rhandFallback;

    [Header("Hand Raise Settings")]
    [SerializeField] 
    private float raiseHeight = 0.35f;
    [SerializeField] 
    private float raiseSpeed = 4.0f;

    private Vector3 _leftHandStart;
    private Vector3 _rightHandStart;

    private float _lhandRaise;
    private float _rhandRaise;

    private void Awake()
    {
        _leftHandStart = lhandFallback.localPosition;
        _rightHandStart = rhandFallback.localPosition;
#if UNITY_EDITOR
        bool isLocalPlayer = true;
#else
        bool isLocalPlayer = GetComponent<NetworkPoseBridge>().HasInputAuthority;
#endif

        enabled = isLocalPlayer;
    }

    private void LateUpdate()
    {
        // Left hand
        bool raiseLeft = Keyboard.current != null && Keyboard.current.qKey.isPressed;

        _lhandRaise = Mathf.MoveTowards(
            _lhandRaise,
            raiseLeft ? 1f : 0f,
            Time.deltaTime * raiseSpeed
        );

        Vector3 lhandOffset = _lhandRaise * raiseHeight * Vector3.up;
        lhandFallback.localPosition = _leftHandStart + lhandOffset;
        lhandBridge.localPosition = lhandFallback.localPosition;

        // Right hand
        bool raiseRight = Keyboard.current != null && Keyboard.current.eKey.isPressed;

        _rhandRaise = Mathf.MoveTowards(
            _rhandRaise,
            raiseRight ? 1f : 0f,
            Time.deltaTime * raiseSpeed
        );
        
        Vector3 rhandOffset = _rhandRaise * raiseHeight * Vector3.up;
        rhandFallback.localPosition = _rightHandStart + rhandOffset;
        rhandBridge.localPosition = rhandFallback.localPosition;
    }
}
