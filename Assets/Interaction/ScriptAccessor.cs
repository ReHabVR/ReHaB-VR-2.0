using System.Collections;
using System.Collections.Generic;
using Interaction;
using UnityEngine;

public class ScriptAccessor : MonoBehaviour
{
    public ControllerMode controllerMode;
    public CSharpForGIT cSharpForGit;
    public ControllerDevice controllerDevice;
    private HandAnim animationController;
    public GameState gameState;


    private void Start()
    {
        if (animationController == null)
        {
            GameObject armature = GameObject.Find("Armature");
            animationController = armature.GetComponent<HandAnim>();
        }
    }

    public void SetAnimatorControllerMode(int mode)
    {
        animationController.ClearAllAnimationBooleans();
        animationController.SetMode(mode);
    }

    public void TriggerAnimation(bool state)
    {
        animationController.TriggerAnimation(state);
    }

    public void ResetAnim()
    {
        animationController.TriggerAnimation(false);
        animationController.Reset();
    }
}
