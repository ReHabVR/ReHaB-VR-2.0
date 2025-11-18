using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
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
    public TextMeshPro infoText;
    public float offsetChange = 0.001f;

    public CameraFOV renderCamera;
    
    [SerializeField]
    private float _pressDelay = 1.5f;
    
    private bool _targetDeviceDetectedL;
    private bool _targetDeviceDetectedR;
    private InputDevice _targetDeviceL;
    private InputDevice _targetDeviceR;
    private bool _isMenuVisible = false;

    private bool _canPressMenu = true;
    private float _lastPressed = 0.0f;
    
    
    void Start()
    {
        TryGetDevices();
    }

    public bool DevicesDetected => _targetDeviceDetectedL && _targetDeviceDetectedR;
    
    void Update()
    {
        if (!DevicesDetected) 
            TryGetDevices();
        
        _lastPressed += Time.deltaTime;

        if (!_canPressMenu && _lastPressed > _pressDelay)
            _canPressMenu = true;

        GetOutput();
    }

    private void TryGetDevices()
    {
        List<InputDevice> devicesR = new();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristicR, devicesR);
        if (devicesR.Count > 0) 
        {
            _targetDeviceDetectedR = true;
            _targetDeviceR = devicesR[0];
            Debug.Log($"DEVICE {_targetDeviceR.name} {_targetDeviceR.characteristics} DETECTED");
        }
        
        List<InputDevice> devicesL = new();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristicL, devicesL);
        if (devicesL.Count > 0) 
        {  
            _targetDeviceDetectedL = true;
            _targetDeviceL = devicesL[0];
            Debug.Log($"DEVICE {_targetDeviceL.name} {_targetDeviceL.characteristics} DETECTED");
        }
    }

    private void GetOutput()
    {        
        _targetDeviceL.TryGetFeatureValue(CommonUsages.menuButton, out var menuButtonVal);
		if (menuButtonVal && _canPressMenu)
		{
			_isMenuVisible = !_isMenuVisible;
            _lastPressed = 0.0f;
            _canPressMenu = false;
            if (_isMenuVisible)
            {
                cameraOffset.transform.position = defaultPos.position;
                infoText.gameObject.SetActive(true);
            }
            else
            {               
                infoText.gameObject.SetActive(false);
            }
        }
		
        //Enable camera setup
        StringBuilder sb = new StringBuilder()
            .AppendLine($"SETTINGS\nCurrent FOV: {renderCamera.GetFov()} (grip to change)")
            .AppendLine($"Current height: {cameraOffset.transform.position.y} (left analog up/down to change)")
            .AppendLine($"Current distance offset: {cameraOffset.transform.position.z} (right analog up/down to change)");
        infoText.text = sb.ToString();

        // Prevent accidental camera changes when menu is disabled;
        if (!_isMenuVisible) 
            return;

        // Left Analog Stick
        if (_targetDeviceL.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 lAxisValue)) 
        {
            // Requires the user to move the analog stick a bit further before adjustments
            if (lAxisValue.y > 0.1f || lAxisValue.y < -0.1f) 
            {
                ChangeCameraHeight(offsetChange * lAxisValue.y);
            }
        }

        // Right Analog Stick
        if (_targetDeviceR.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rAxisValue)) 
        {
            // Requires the user to move the analog stick a bit further before adjustments
            if (rAxisValue.y > 0.1f || rAxisValue.y < -0.1f) 
            {
                ChangeDistanceOffset(offsetChange * rAxisValue.y);
            }
        }

        // Left Trigger
        if (_targetDeviceL.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerLPressed)) 
		{
			if (triggerLPressed) 
			{
				ChangeFov(-0.1f);
			}
		}

        // Right Trigger
		if (_targetDeviceR.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerRPressed)) 
		{
			if (triggerRPressed)
			{
				ChangeFov(0.1f);
			}
		}

    #region PRESETS
        // Preset 1 - A
        if (_targetDeviceR.TryGetFeatureValue(CommonUsages.primaryButton, out bool raPressed)) 
		{
			if (raPressed)
				renderCamera.SetFov(85.0f);
		}
        // Preset 2 - B
        if (_targetDeviceR.TryGetFeatureValue(CommonUsages.secondaryButton, out bool rbPressed)) 
		{
			if (rbPressed)
				renderCamera.SetFov(90.0f);
		}
        // Preset 3 - X
        if (_targetDeviceL.TryGetFeatureValue(CommonUsages.primaryButton, out bool laPressed)) 
		{
			if (laPressed)
				renderCamera.SetFov(95.0f);
        }
        // Preset 4 - Y
        if (_targetDeviceL.TryGetFeatureValue(CommonUsages.secondaryButton, out bool lbPressed)) 
		{
			if (lbPressed)
				renderCamera.SetFov(100.0f);
		}
    #endregion
    }

    void ChangeCameraHeight(float amount)
    {
        Vector3 pos = mainCamera.transform.localPosition;
        // Clamp camera
        if ((pos.y >= 2.0f && amount > 0) || (pos.y <= 1.0f && amount < 0)) 
            return; 

        cameraOffset.transform.localPosition += new Vector3(0, amount, 0);
    }

    void ChangeDistanceOffset(float amount) 
    {
        Vector3 pos = mainCamera.transform.localPosition;
        if ((pos.z >= 0.0f && amount > 0) || (pos.z <= -0.5f && amount < 0)) 
            return; 

        cameraOffset.transform.localPosition += new Vector3(0, 0, amount);
    }

    void ChangeFov(float amount) {
        if (renderCamera.GetFov() > 120.0f && amount > 0 ||
            renderCamera.GetFov() < 60.0f && amount < 0)
            return;
        
        renderCamera.secondaryCamera.fieldOfView += amount;
    }
}
