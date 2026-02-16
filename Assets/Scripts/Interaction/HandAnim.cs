using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandAnim : MonoBehaviour
{
    public InputDeviceCharacteristics controllerR;
    public InputDeviceCharacteristics controllerL;
    public Animator handAnimator;
    
    private InputDevice _targetDeviceR;
    private InputDevice _targetDeviceL;
    private bool _targetDeviceDetected;
    

    void Start()
    {
        TryToGetDevices();
    }

    void Update()
    {
        if (!_targetDeviceDetected) 
            TryToGetDevices();
        
        UpdateHandAnimation();
    }

    private void TryToGetDevices()
    {
        _targetDeviceDetected = GetDevice(controllerR, ref _targetDeviceR) && GetDevice(controllerL, ref _targetDeviceL);
        if (_targetDeviceDetected) 
        {
            Debug.Log($"DEVICE {_targetDeviceR.name} {_targetDeviceR.characteristics} DETECTED");
            Debug.Log($"DEVICE {_targetDeviceL.name} {_targetDeviceL.characteristics} DETECTED");
        }
    }

    private bool GetDevice(InputDeviceCharacteristics device, ref InputDevice input)
    {
        List<InputDevice> devices = new();
        InputDevices.GetDevicesWithCharacteristics(device, devices);
        if (devices.Count < 1) 
            return false;
        
        input = devices[0];
        return true;
    }

    private void UpdateHandAnimation()
    {
        if (!_targetDeviceDetected)
            return;
        
        SetAnimationValue("GripR", ref _targetDeviceR);
        SetAnimationValue("GripL", ref _targetDeviceL);
    }

    private void SetAnimationValue(string id, ref InputDevice targetDevice)
    {
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out var triggerVal);
        handAnimator.SetFloat(id, triggerVal > 0.1f ? triggerVal : 0f);
    }

    public void Reset()
    {
        handAnimator.SetFloat("GripR", 0f);
        handAnimator.SetFloat("GripL", 0f);
    }
}
