using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPresence : MonoBehaviour
{
    public bool showController = false;
    public InputDeviceCharacteristics controllerCharacteristic;
    public List<GameObject> controllerPrefabs;
    public GameObject handModelPrefab;

    private InputDevice _targetDevice;
    private bool _targetDeviceDetected;
    private GameObject _spawnedController;
    private GameObject _spawnedHandModel;
    private Animator _handAnimator;

    // Start is called before the first frame update
    void Start()
    {
        TryToGetDevices();
    }

    // Update is called once per frame
    void Update()
    {
        EnsureDeviceIsDetected();
        //GetOutput();
        SetActiveHandModel();
    }

    private void TryToGetDevices()
    {
        List<InputDevice> devices = new();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristic, devices);
        if (devices.Count <= 0) return;
        _targetDeviceDetected = true;
        _targetDevice = devices[0];
        Debug.Log($"DEVICE {_targetDevice.name} {_targetDevice.characteristics} DETECTED");
        var prefab = controllerPrefabs.Find(ctrl => ctrl.name == _targetDevice.name);
        _spawnedController = Instantiate(prefab ? prefab : controllerPrefabs.First(), transform);
        _spawnedHandModel = Instantiate(handModelPrefab, transform);
        _handAnimator = _spawnedHandModel.GetComponent<Animator>();
    }

    private void UpdateHandAnimation()
    {
        SetAnimationValue("Trigger", CommonUsages.trigger);
        SetAnimationValue("Grip", CommonUsages.grip);
    }

    private void SetAnimationValue(string id, InputFeatureUsage<float> usages)
    {
        var isTriggered = _targetDevice.TryGetFeatureValue(usages, out var triggerVal);
        _handAnimator.SetFloat(id, isTriggered ? triggerVal : 0);
    }

    private void SetActiveHandModel()
    {
        if (!_targetDeviceDetected) return;
        _spawnedController.SetActive(showController);
        _spawnedHandModel.SetActive(!showController);
        if (!showController) UpdateHandAnimation();
    }

    private void GetOutput()
    {
        //For getting user input in the future
        _targetDevice.TryGetFeatureValue(CommonUsages.trigger, out var triggerVal);
        if (triggerVal > 0.01) Debug.Log("Trigger");
    }

    private void EnsureDeviceIsDetected()
    {
        if (!_targetDeviceDetected) TryToGetDevices();
    }
}