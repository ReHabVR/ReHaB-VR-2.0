using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExternalPoseProvider : MonoBehaviour
{
    [SerializeField]
    private NetworkPoseBridge networkPoseBridge;

    [Header("Bridge Transforms")]
    public Transform lhandBridge;
    public Transform rhandBridge;
    public Transform headBridge;

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
        _taskman = CurrentTaskManager.Instance;

        /*
        _leftHandStart = lhandFallback.localPosition;
        _rightHandStart = rhandFallback.localPosition;
        _headStart = headFallback.localPosition;
        
        lhandBridge.localPosition = _leftHandStart;
        rhandBridge.localPosition = _rightHandStart;
        headBridge.localPosition = _headStart;
        */
    }

    public void OnSpawned()
    {
        // Disable PoseProvider for non-local players
        enabled = networkPoseBridge.HasInputAuthority;
    }

    private void LateUpdate()
    {
    #if CLIENT_VR
        return;
    #endif
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

    #if CLIENT_PC
        lhandBridge.localPosition = lhandFallback.localPosition;
    #endif

        // Right hand
        bool raiseRight = Keyboard.current.eKey.isPressed;

        _rhandRaise = Mathf.MoveTowards(
            _rhandRaise,
            raiseRight ? 1f : 0f,
            Time.deltaTime * raiseSpeed
        );
        
        Vector3 rhandOffset = _rhandRaise * raiseHeight * Vector3.up;
        rhandFallback.localPosition = _rightHandStart + rhandOffset;
    #if CLIENT_PC
        rhandBridge.localPosition = rhandFallback.localPosition;
    #endif

        // Fix head position
        headFallback.localPosition = _headStart;
    #if CLIENT_PC
        headBridge.localPosition = headFallback.localPosition;
    #endif

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
            _taskman.DebugMove(false); // regular move
        }
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            _taskman.DebugMove(true); // correct move
        }
        #endregion
    }
}
