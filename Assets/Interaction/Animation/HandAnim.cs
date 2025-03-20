using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;
using Photon.Pun;
using Debug = UnityEngine.Debug;

public class HandAnim : MonoBehaviour
{
    public InputDeviceCharacteristics controllerR;
    public InputDeviceCharacteristics controllerL;
    public Animator handAnimator;
    public GameObject objectWithPhotonView;
    public int cycleDuration = 500;

    private InputDevice _targetDeviceR;
    private InputDevice _targetDeviceL;
    private bool _targetDeviceDetected;
    private PhotonView photonView;
    private int mode = 0;
    private bool shouldAnimationBeTriggered;
    private Stopwatch _animTimer = Stopwatch.StartNew();
    private bool _grip = true;
    private Animator _exerciseHandAnimatorMaleRight;
    private Animator _exerciseHandAnimatorFemaleRight;
    public GameObject handObjectMaleRight;
    public GameObject handObjectFemaleRight;


    // Start is called before the first frame update
    void Start()
    {
        photonView = objectWithPhotonView.GetComponent<PhotonView>();
        TryToGetDevices();
        _exerciseHandAnimatorMaleRight = handObjectMaleRight.GetComponent<Animator>();
        _exerciseHandAnimatorFemaleRight = handObjectFemaleRight.GetComponent<Animator>();
        if (handAnimator != null)
        {
            ClearAllAnimationBooleans();
        }

    }

    // Update is called once per frame
    void Update()
    {
        EnsureDeviceIsDetected();
        UpdateHandAnimation();
    }

    private void TryToGetDevices()
    {
        if (!GetDevice(controllerR, ref _targetDeviceR)) return;
        if (!GetDevice(controllerL, ref _targetDeviceL)) return;
        _targetDeviceDetected = true;
        Debug.Log($"DEVICE {_targetDeviceR.name} {_targetDeviceR.characteristics} DETECTED");
    }

    private bool GetDevice(InputDeviceCharacteristics device, ref InputDevice input)
    {
        List<InputDevice> devices = new();
        InputDevices.GetDevicesWithCharacteristics(device, devices);
        if (devices.Count <= 0) return false;
        input = devices[0];
        return true;
    }

    private void UpdateHandAnimation()
    {
        if (!photonView.IsMine) return;
        // ClearAllAnimationBooleans();
        switch (mode)
        {
            case 0 when !_targetDeviceDetected:
                return;
            case 0:
                SetAnimationValue("GripR", ref _targetDeviceR);
                SetAnimationValue("GripL", ref _targetDeviceL);
                break;
            case 2:
                ForceQuickAnimationValue("GripR", shouldAnimationBeTriggered);
                break;
            case 3:
                ForceSlowAnimationValue("GripR", shouldAnimationBeTriggered);
                ForceSlowAnimationValue("GripL", shouldAnimationBeTriggered);
                break;
            case 4:
                AnimateHandMovement("GripR", shouldAnimationBeTriggered);
                AnimateHandMovement("GripL", shouldAnimationBeTriggered);
                break;
            case 5:
                PlayExerciseOne(shouldAnimationBeTriggered);
                break;
            case 6:
                PlayExerciseTwoA(shouldAnimationBeTriggered);
                break;
            case 7:
                PlayExerciseSeven(shouldAnimationBeTriggered);
                break;
            case 8:
                PlayExerciseThree(shouldAnimationBeTriggered);
                break;
            case 9:
                PlayExerciseFour(shouldAnimationBeTriggered);
                break;
            case 10:
                PlayExerciseFive(shouldAnimationBeTriggered);
                break;
            case 11:
                PlayExerciseSix(shouldAnimationBeTriggered);
                break;
        }
    }

    private void SetAnimationValue(string id, ref InputDevice targetDevice)
    {
        var isTriggered = targetDevice.TryGetFeatureValue(CommonUsages.trigger, out var triggerVal);
        handAnimator.SetFloat(id, triggerVal > 0.1f ? triggerVal : 0f);
    }

    private void ForceQuickAnimationValue(string id, bool state)
    {
        if (state)
        {
            handAnimator.SetFloat(id, 1f, 1f, 0.1f);
        } else
        {
            handAnimator.SetFloat(id, state ? 1f : 0.01f, 1f, 0.1f);
        }
    }

    private void ForceSlowAnimationValue(string id, bool state)
    {
        if (state)
        {
            handAnimator.SetFloat(id, 1f, 1f, 0.01f);
        }
        else
        {
            handAnimator.SetFloat(id, state ? 1f : 0.01f, 1f, 0.01f);
        }
    }

    private void AnimateHandMovement(string id, bool state)
    {
        if (!state) return;
        if (!_animTimer.IsRunning) _animTimer.Start();

        if (_animTimer.ElapsedMilliseconds > cycleDuration)
        {
            _grip = !_grip;
            _animTimer.Reset();
        }

        handAnimator.SetFloat(id, _grip ? 1f : 0.01f, 1f, 0.02f);
    }

    public void ClearAllAnimationBooleans()
    {
        SetBoolForBothAnimators("Is_Exercise_1", false);
        SetBoolForBothAnimators("Is_Exercise_2A_In_Idle", false);
        SetBoolForBothAnimators("Is_Exercise_2A", false);
        SetBoolForBothAnimators("Is_Exercise_7_In_Idle", false);
        SetBoolForBothAnimators("Is_Exercise_7", false);
        SetBoolForBothAnimators("Is_Exercise_3_In_Idle", false);
        SetBoolForBothAnimators("Is_Exercise_3", false);
        SetBoolForBothAnimators("Is_Exercise_3_In_Idle", false);
        SetBoolForBothAnimators("Is_Exercise_3", false);
        SetBoolForBothAnimators("Is_Exercise_4_In_Idle", false);
        SetBoolForBothAnimators("Is_Exercise_4", false);
        SetBoolForBothAnimators("Is_Exercise_5_In_Idle", false);
        SetBoolForBothAnimators("Is_Exercise_5", false);
        SetBoolForBothAnimators("Is_Exercise_6_In_Idle", false);
        SetBoolForBothAnimators("Is_Exercise_6", false);
    }

    private void SetBoolForBothAnimators(string name, bool value)
    {
        if (_exerciseHandAnimatorMaleRight == null || _exerciseHandAnimatorFemaleRight == null) return;
        _exerciseHandAnimatorMaleRight.SetBool(name, value);
        _exerciseHandAnimatorFemaleRight.SetBool(name, value);

    }

    private void PlayExerciseOne(bool state)
    {
        SetBoolForBothAnimators("Is_Exercise_1", state);
    }

    private void PlayExerciseTwoA(bool state)
    {
        if (state == false)
        {
            SetBoolForBothAnimators("Is_Exercise_2A_In_Idle", true);
            SetBoolForBothAnimators("Is_Exercise_2A", false);
        }
        else
        {
            SetBoolForBothAnimators("Is_Exercise_2A_In_Idle", false);
            SetBoolForBothAnimators("Is_Exercise_2A", true);
        }
    }

    private void PlayExerciseSeven(bool state)
    {
        if (state == false)
        {
            SetBoolForBothAnimators("Is_Exercise_7_In_Idle", true);
            SetBoolForBothAnimators("Is_Exercise_7", false);
        }
        else
        {
            SetBoolForBothAnimators("Is_Exercise_7_In_Idle", false);
            SetBoolForBothAnimators("Is_Exercise_7", true);
        }
    }
    private void PlayExerciseThree(bool state)
    {
        if (state == false)
        {
            SetBoolForBothAnimators("Is_Exercise_3_In_Idle", true);
            SetBoolForBothAnimators("Is_Exercise_3", false);
        }
        else
        {
            SetBoolForBothAnimators("Is_Exercise_3_In_Idle", false);
            SetBoolForBothAnimators("Is_Exercise_3", true);
        }
    }

    private void PlayExerciseFour(bool state)
    {
        if (state == false)
        {
            SetBoolForBothAnimators("Is_Exercise_4_In_Idle", true);
            SetBoolForBothAnimators("Is_Exercise_4", false);
        }
        else
        {
            SetBoolForBothAnimators("Is_Exercise_4_In_Idle", false);
            SetBoolForBothAnimators("Is_Exercise_4", true);
        }
    }

    private void PlayExerciseFive(bool state)
    {
        if (state == false)
        {
            SetBoolForBothAnimators("Is_Exercise_5_In_Idle", true);
            SetBoolForBothAnimators("Is_Exercise_5", false);
        }
        else
        {
            SetBoolForBothAnimators("Is_Exercise_5_In_Idle", false);
            SetBoolForBothAnimators("Is_Exercise_5", true);
        }
    }

    private void PlayExerciseSix(bool state)
    {
        if (state == false)
        {
            SetBoolForBothAnimators("Is_Exercise_6_In_Idle", true);
            SetBoolForBothAnimators("Is_Exercise_6", false);
        }
        else
        {
            SetBoolForBothAnimators("Is_Exercise_6_In_Idle", false);
            SetBoolForBothAnimators("Is_Exercise_6", true);
        }
    }


    private void EnsureDeviceIsDetected()
    {
        if (!_targetDeviceDetected) TryToGetDevices();
    }

    public void SetMode(int targetMode)
    {
        mode = targetMode;
    }

    public void TriggerAnimation(bool state)
    {
        shouldAnimationBeTriggered = state;
    }

    public void Reset()
    {
        handAnimator.SetFloat("GripR", 0f);
        handAnimator.SetFloat("GripL", 0f);
    }
}