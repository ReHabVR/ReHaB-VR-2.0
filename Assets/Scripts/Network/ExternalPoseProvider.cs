using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExternalPoseProvider : MonoBehaviour
{
    [SerializeField]
    private NetworkPoseBridge networkPoseBridge;

    [Header("Bridge Transforms")]
    [SerializeField] 
    private Transform lhandBridge;
    [SerializeField] 
    private Transform rhandBridge;
    [SerializeField]
    private Transform headBridge;

    [Header("Fallback Transforms")]
    [SerializeField] 
    private Transform lhandFallback;
    [SerializeField] 
    private Transform rhandFallback;
    [SerializeField] 
    private Transform headFallback;

    [Header("Hand Raise Settings")]
    [SerializeField] 
    private float raiseHeight = 0.35f;
    [SerializeField] 
    private float raiseSpeed = 4.0f;

    private Vector3 _leftHandStart;
    private Vector3 _rightHandStart;
    private Vector3 _headStart;

    private float _lhandRaise;
    private float _rhandRaise;

    private CurrentTaskManager _taskman;

    private void Awake()
    {
        _leftHandStart = lhandFallback.localPosition;
        _rightHandStart = rhandFallback.localPosition;
        _headStart = headFallback.localPosition;
        
        lhandBridge.localPosition = _leftHandStart;
        rhandBridge.localPosition = _rightHandStart;
        headBridge.localPosition = headFallback.localPosition;        

        _taskman = CurrentTaskManager.Instance;
    }

    public void OnSpawned()
    {
        // Disable PoseProvider for non-local players
        enabled = networkPoseBridge.HasInputAuthority;
    }

    private void LateUpdate()
    {
        if (networkPoseBridge == null || 
            !networkPoseBridge.IsReady || 
            !networkPoseBridge.HasInputAuthority || 
            Keyboard.current == null)
        {
            return;
        }

        // Left hand
        bool raiseLeft = Keyboard.current.qKey.isPressed;

        _lhandRaise = Mathf.MoveTowards(
            _lhandRaise,
            raiseLeft ? 1f : 0f,
            Time.deltaTime * raiseSpeed
        );

        Vector3 lhandOffset = _lhandRaise * raiseHeight * Vector3.up;
        lhandFallback.localPosition = _leftHandStart + lhandOffset;
        lhandBridge.localPosition = lhandFallback.localPosition;

        // Right hand
        bool raiseRight = Keyboard.current.eKey.isPressed;

        _rhandRaise = Mathf.MoveTowards(
            _rhandRaise,
            raiseRight ? 1f : 0f,
            Time.deltaTime * raiseSpeed
        );
        
        Vector3 rhandOffset = _rhandRaise * raiseHeight * Vector3.up;
        rhandFallback.localPosition = _rightHandStart + rhandOffset;
        rhandBridge.localPosition = rhandFallback.localPosition;

        // Fix head position
        headFallback.localPosition = _headStart;
        headBridge.localPosition = headFallback.localPosition;

        #region DEBUG
        if (_taskman == null)
        {
            return;
        }
        
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _taskman.RPC_ToggleTestTask();
        }
        // Debug moves
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            _taskman.RPC_DebugMove(false); // regular move
        }
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            _taskman.RPC_DebugMove(true); // correct move
        }
        #endregion
    }
}
