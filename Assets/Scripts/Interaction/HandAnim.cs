using ReHaB.Core;
using UnityEngine;

public class HandAnim : MonoBehaviour
{
    [SerializeField] 
    private MonoBehaviour poseSourceComponent; // workaround to get to the interface

    [SerializeField] 
    private Animator handAnimator;

    private IHandPoseSource poseSource;

    private void Awake()
    {
        poseSource = poseSourceComponent as IHandPoseSource;
    }

    private void Update()
    {
        if (poseSource == null)
        {
            return;
        }

        handAnimator.SetFloat("GripL", poseSource.GetGripL());
        handAnimator.SetFloat("GripR", poseSource.GetGripR());
    }
}
