using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using TMPro;
using UnityEngine.Events;
using System.Text;

public class ControllerDevice : MonoBehaviour
{
    public InputDeviceCharacteristics controllerCharacteristicL;
    public InputDeviceCharacteristics controllerCharacteristicR;
    public GameObject cameraOffset;
    public GameObject mainCamera;
    public Transform defaultPos;
    
    [SerializeField]
    private float _pressDelay = 1.5f;
    
    private bool _targetDeviceDetectedL;
    private bool _targetDeviceDetectedR;
    private InputDevice _targetDeviceL;
    private InputDevice _targetDeviceR;
    //private bool _isMenuVisible = false;

    private bool _canPressMenu = true;
    private float _lastPressed = 0.0f;
    
    public bool DevicesDetected => _targetDeviceDetectedL && _targetDeviceDetectedR;
    
    void Start()
    {
        TryGetDevices();
    }

    void Update()
    {
        if (!DevicesDetected) 
        {
            TryGetDevices();
        }
        
        _lastPressed += Time.deltaTime;

        if (!_canPressMenu && _lastPressed > _pressDelay)
        {
            _canPressMenu = true;
        }

        GetOutput();
    }

    private void TryGetDevices()
    {
        List<InputDevice> devicesR = new();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristicR, devicesR);
        if (devicesR.Count > 0) 
        {
            if (!_targetDeviceDetectedR)
            {
                Debug.Log($"DEVICE {_targetDeviceR.name} {_targetDeviceR.characteristics} DETECTED");
            }
            _targetDeviceDetectedR = true;
            _targetDeviceR = devicesR[0];
        }
        else
        {
            _targetDeviceDetectedR = false;
        }
        
        List<InputDevice> devicesL = new();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristicL, devicesL);
        if (devicesL.Count > 0) 
        {  
            if (!_targetDeviceDetectedL)
            {
                Debug.Log($"DEVICE {_targetDeviceL.name} {_targetDeviceL.characteristics} DETECTED");
            }
            _targetDeviceDetectedL = true;
            _targetDeviceL = devicesL[0];
        }
        else
        {
            _targetDeviceDetectedL = false;
        }
    }

    private void GetOutput()
    {        
        _targetDeviceL.TryGetFeatureValue(CommonUsages.menuButton, out var menuButtonVal);
		if (menuButtonVal && _canPressMenu)
		{
            // Add menu button logic here
			return;
        }
    }
}
