using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.XR;
using Interaction;
using TMPro;

public class ControllerDevice : MonoBehaviour
{
    public InputDeviceCharacteristics controllerCharacteristicL;
    public InputDeviceCharacteristics controllerCharacteristicR;
    public ScriptAccessor scriptAccessor;
    public GameObject cameraOffset;
    public GameObject defaultPos;
    public GameObject camPos;
    public TextMeshPro ipText;
    public float offsetChange = 0.005f;
    
    private DateTime lastChange;
    private bool _targetDeviceDetectedL;
    private bool _targetDeviceDetectedR;
    private InputDevice _targetDeviceL;
    private InputDevice _targetDeviceR;
	public GameObject maleArmRight;
	public GameObject maleArmLeft;
	public GameObject femaleArmRight;
	public GameObject femaleArmLeft;
	public GameObject bananaArmRight;
	public GameObject bananaArmLeft;
	public float rotationSpeed = 20.0f;
	private bool _isMenuVisible;
	private int _typeOfArm = 0;
    
    
    // Start is called before the first frame update
    void Start()
    {
	    ChaneTypeOfArm(true);
        TryToGetDevices();
        lastChange = DateTime.Now;
        SetIpText();
        ipText.enabled = false;
		_isMenuVisible = false;
    }

    public bool DevicesDetected => _targetDeviceDetectedL && _targetDeviceDetectedR;
    
    private static string[] _modeDesc = { "Controllers", "Lift Hands", "Balls", "Grip and Hold", "Grip and release", "Exercise 1", "Exercise 2A", "Exercise 7", "Exercise 3", "Exercise 4", "Exercise 5", "Exercise 6" };
    private static string[] _armsMode = { "Controllers Arms", "Male Arms", "Female Arms"};
    public void SetIpText()
    {
        var mode = scriptAccessor.cSharpForGit.mode;
        // ipText.color = Color.black;
        ipText.text = $"SETTINGS\nArms Mode(Grip Buttons):{_typeOfArm} ({_armsMode[_typeOfArm]})\nCamera rotation in X axis - Triggers\nCamera position Y axis - A/B Buttons\nCamera position X axis - X/Y Buttons\nMode:{mode} ({_modeDesc[mode]})\n{GetLocalIPAddress()}:{scriptAccessor.cSharpForGit.connectionPort}" ;
        // Debug.Log(ipText.text);
    }

    // Update is called once per frame
    void Update()
    {
        EnsureDeviceIsDetected();
        GetOutput();
        // Debug.Log("Is Menu Visible: " + _isMenuVisible);
        //
        // if (!_isMenuVisible) return;
        if (Input.GetKey(KeyCode.P)){_isMenuVisible = !_isMenuVisible;}
        if (!_isMenuVisible)
        {
	        ipText.enabled = false;
	        return;
        } 
        SetIpText();
        ipText.enabled = true;
        if (Input.GetKey(KeyCode.O)){RotateCameraInXAxis(-rotationSpeed);}
        if (Input.GetKey(KeyCode.M)){ChaneTypeOfArm(true);;}
        if (Input.GetKey(KeyCode.U)){ChaneTypeOfArm(false);}
    }

    private void TryToGetDevices()
    {
        List<InputDevice> devicesR = new();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristicR, devicesR);
        if (devicesR.Count <= 0) return;
        _targetDeviceDetectedR = true;
        _targetDeviceR = devicesR[0];
        Debug.Log($"DEVICE {_targetDeviceR.name} {_targetDeviceR.characteristics} DETECTED");
        
        List<InputDevice> devicesL = new();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristicL, devicesL);
        if (devicesL.Count <= 0) return;
        _targetDeviceDetectedL = true;
        _targetDeviceL = devicesL[0];
        Debug.Log($"DEVICE {_targetDeviceL.name} {_targetDeviceL.characteristics} DETECTED");
    }

    private void GetOutput()
    {
        _targetDeviceL.TryGetFeatureValue(CommonUsages.menuButton, out var menuButtonVal);
		if (menuButtonVal)
		{
			_isMenuVisible = !_isMenuVisible;
		}

		if (!_isMenuVisible)
		{
			ipText.enabled = false;
			return;
		} 
		SetIpText();
		ipText.enabled = true;
		// Sterowanie kamerą za pomocą przycisków na VIVE FOCUS 3
		if (_targetDeviceR.TryGetFeatureValue(CommonUsages.primaryButton, out var raPressed)) 
		{
			if (raPressed) 
			{
				ChangeCameraHeight(-offsetChange);
			}
		}	
		if (_targetDeviceR.TryGetFeatureValue(CommonUsages.secondaryButton, out var rbPressed)) 
		{
			if (rbPressed)
			{
				ChangeCameraHeight(offsetChange);
			}
		}
		if (_targetDeviceL.TryGetFeatureValue(CommonUsages.primaryButton, out var laPressed)) 
		{
			if (laPressed) 
			{
				ChangeDistanceOffset(offsetChange);
			}
		}
		if (_targetDeviceL.TryGetFeatureValue(CommonUsages.secondaryButton, out var lbPressed)) 
		{
			if (lbPressed)
			{
				ChangeDistanceOffset(-offsetChange);
			}
		}
		if (_targetDeviceL.TryGetFeatureValue(CommonUsages.triggerButton, out var triggerLPressed)) 
		{
			if (triggerLPressed) 
			{
				RotateCameraInXAxis(-rotationSpeed);
			}
		}
		if (_targetDeviceR.TryGetFeatureValue(CommonUsages.triggerButton, out var triggerRPressed)) 
		{
			if (triggerRPressed)
			{
				RotateCameraInXAxis(rotationSpeed);
			}
		}
		if (_targetDeviceR.TryGetFeatureValue(CommonUsages.gripButton, out var gripRPressed)) 
		{
			if (gripRPressed)
			{
				ChaneTypeOfArm(true);
			}
		}

		ipText.enabled = false;
		if (_targetDeviceL.TryGetFeatureValue(CommonUsages.gripButton, out var gripLPressed)) 
		{
			if (gripLPressed)
			{
				ChaneTypeOfArm(false);
			}
		}

		/*
        if (menuButtonVal)
        {
            if ((DateTime.Now - lastChange).TotalMilliseconds < 100)
            {
                //For holding
                lastChange = DateTime.Now;
                return;
            }
            lastChange = DateTime.Now;
            scriptAccessor.controllerMode.ChangeMode();
            SetIpText();
            ipText.enabled = true;
			//maleArm.SetActive(true);
			//femaleArm.SetActive(true);
			//bananaArm.SetActive(true);
        }
        else
        {
            ipText.enabled = false;
        }
        
        _targetDeviceL.TryGetFeatureValue(CommonUsages.menuButton, out var triggerVal2);
        if (triggerVal2)
        {
            var diff = defaultPos.transform.position - camPos.transform.position;
            var target = cameraOffset.transform.position + diff;
            cameraOffset.transform.position = Vector3.MoveTowards(cameraOffset.transform.position, target, 0.01f);
            SetIpText();
            ipText.enabled = true;
        }
        else
        {
            ipText.enabled = false;
        }	
		*/	
    }

    private void ChaneTypeOfArm(bool shouldIncrease)
    {
	    IncrementTypeOfArmValue(shouldIncrease);
	    switch (_typeOfArm)
	    {
		    case 0:
			    SetActiveBananaArms(true);
			    SetActiveMaleArms(false);
			    SetActiveFemaleArms(false);
			    break;
		    case 1:
			    SetActiveBananaArms(false);
			    SetActiveMaleArms(true);
			    SetActiveFemaleArms(false);
			    break;
		    case 2:
			    SetActiveBananaArms(false);
			    SetActiveMaleArms(false);
			    SetActiveFemaleArms(true);
			    break;
		    default:
			    return;
	    }
    }
    
    private void SetActiveBananaArms(bool value)
    {
	    if (value)
	    {
		    bananaArmRight.transform.localScale = Vector3.one;
		    bananaArmLeft.transform.localScale = Vector3.one;
	    }
	    else
	    {
		    bananaArmRight.transform.localScale = Vector3.zero;
		    bananaArmLeft.transform.localScale = Vector3.zero;	    
	    }
    }
    
    private void SetActiveMaleArms(bool value)
    {
	    maleArmRight.SetActive(value);
	    maleArmLeft.SetActive(value);
    }

    private void SetActiveFemaleArms(bool value)
    {
	    femaleArmRight.SetActive(value); 
	    femaleArmLeft.SetActive(value); 
    }

    private void IncrementTypeOfArmValue(bool shouldIncrease)
    {
	    int increment = shouldIncrease ? 1 : -1;
	    _typeOfArm = (_typeOfArm + increment + 3) % 3;
    }

	void RotateCameraInXAxis(float rotation)
	{
		var currentCameraPosition = cameraOffset.transform.position;
		cameraOffset.transform.Rotate(Vector3.right * rotation * Time.deltaTime);
		// cameraOffset.transform.position = currentCameraPosition;
		var diff = defaultPos.transform.position - camPos.transform.position;
		cameraOffset.transform.position += diff;
	}

	void ChangeCameraHeight(float amount)
    {
        Vector3 pos = camPos.transform.position;
        //Clamp camera.
        if ((pos.y >= 2.0f && amount > 0) || (pos.y <= 1.0f && amount < 0)) 
            return; 

        cameraOffset.transform.position += new Vector3(0, amount, 0);
        // defaultPos.transform.position = cameraOffset.transform.position;
    }

    void ChangeDistanceOffset(float amount) 
    {
        //Vector3 pos = camPos.transform.position;
        //Clamp camera.
        //if ((pos.z >= 2.0f && amount > 0) || (pos.z <= 1.0f && amount < 0)) 
            //return; 
        cameraOffset.transform.position += new Vector3(amount, 0, 0);
        // defaultPos.transform.position = cameraOffset.transform.position;
    }

    public void SetCameraHeight(float height)
    {
        Vector3 pos = cameraOffset.transform.position;
        pos.y = height;
        cameraOffset.transform.position = pos;
    }

    public void SetCameraDistance(float distance) 
    {
        Vector3 pos = cameraOffset.transform.position;
        pos.x = distance;
        cameraOffset.transform.position = pos;
    }

    private void EnsureDeviceIsDetected()
    {
        if (!_targetDeviceDetectedR || !_targetDeviceDetectedL) TryToGetDevices();
    }
    
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                Debug.Log(ip.ToString());
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}