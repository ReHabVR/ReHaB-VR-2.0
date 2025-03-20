using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Interaction
{
    public class ControllerMode : MonoBehaviour
    {
        public int controllerMode = 0;
        public float liftHandR = 0f;
        public float liftHandL = 0f;
        public GameObject rController;
        public GameObject lController;
        public ScriptAccessor scriptAccessor;
        public GameObject rHandPos;
        public GameObject lHandPos;
        private ButtonTrainingSwapper objectSpawner;
        private bool updateMode = false;
        // private Animator handAnimator;
        // public GameObject handObject;

        // private bool IsAnimationRunning;

        void Start()
        {
            objectSpawner = new ButtonTrainingSwapper();
            // handAnimator = handObject.GetComponent<Animator>();
            // handAnimator.SetTrigger("Stop");
            SetUpMode();
        }

        private void Update()
        {
            if (updateMode)
            {
                updateMode = false;
                Debug.Log($"Mode changed to: {controllerMode}");
                // handAnimator.SetTrigger("Stop");
                SetUpMode();
            }
        }

        // -1 => next mode
        public void ChangeMode(int mode = -1)
        {
            if (mode == -1)
            {
                controllerMode++;
                if (controllerMode > 11) controllerMode = 0;
                
            }
            else
            {
                controllerMode = mode;
            }
            updateMode = true;
        }

        public void SetUpMode()
        {
            objectSpawner.DeleteAllObjects();
            scriptAccessor.cSharpForGit.mode = controllerMode;
            scriptAccessor.SetAnimatorControllerMode(controllerMode);
            objectSpawner.SetButtonVisibility(controllerMode == 0);
            rController.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>().enabled = controllerMode == 0;
            lController.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>().enabled = controllerMode == 0;

            Clear();
            switch (controllerMode)
            {
                case 0:
                    SetUpButtonOnTheTable();
                    break;
                case 1:
                    SetUpLiftHandsMode();
                    break;
                case 2:
                    SetUpBallsMoving();
                    break;
                case 3:
                    SetUpGripping();
                    break;
                case 4:
                    SetUpGripping();
                    break;
                // case 5:
                //     SetFirstExerciseAnimation();
                //     break;
                // case 6:
                //     SetSecondExerciseAnimation();
                //     break;
                // case 7:
                //     SetThirdExerciseAnimation();
                //     break;
                default:
                    break;
            }

            scriptAccessor.cSharpForGit.UseReceivedData().ConfigureAwait(false);;
        }

        private void Clear()
        {
            rController.transform.position = rHandPos.transform.position;
            rController.transform.rotation = rHandPos.transform.rotation;
            lController.transform.position = lHandPos.transform.position;
            lController.transform.rotation = lHandPos.transform.rotation;
            scriptAccessor.ResetAnim();
            // ResetAllHandTriggers();
        }

        private void SetUpButtonOnTheTable()
        {
            objectSpawner.PrepareObjectSpawnButton();
        }

        private void SetUpLiftHandsMode()
        {
            var up = transform.up;
            rController.transform.position = rHandPos.transform.position + up * liftHandR;
            rController.transform.rotation = rHandPos.transform.rotation;
            
            lController.transform.position = lHandPos.transform.position + up * liftHandL;
            lController.transform.rotation = lHandPos.transform.rotation;
        }

        public void SetUpGripping()
        {
            rController.transform.rotation = Quaternion.Euler(new Vector3(0, -25, -95)); 
            lController.transform.rotation = Quaternion.Euler(new Vector3(0, 25, 75)); 
        }

        private void SetUpBallsMoving()
        {
            SetUpHandsForBallz();
            objectSpawner.SpawnFirstTraining();
        }

        public void SetUpHandsForBallz()
        {
            rController.transform.rotation = Quaternion.Euler(new Vector3(0, -25, 75));
            lController.transform.rotation = Quaternion.Euler(new Vector3(0, 25, -95));
        }

        // private void ResetAllHandTriggers()
        // {
            // handAnimator.SetTrigger("Stop");
        // }

        // public void SetFirstExerciseAnimation()
        // {
            // handAnimator.SetTrigger("Exercise_1");
            // handAnimator.SetBool("Is_Exercise_1", true);
        // }
        // public void SetSecondExerciseAnimation()
        // {
        //     animator.SetTrigger("Exercise_2A");
        // }
        // public void SetThirdExerciseAnimation()
        // {
        //     animator.SetTrigger("Exercise_7");
        // }

        // public void SetIsAnimationRunning(bool shouldAnimationRun)
        // {
        //     IsAnimationRunning = shouldAnimationRun;
        //     Debug.Log("Should Animation run: " + IsAnimationRunning);
        // }
    }
}
