using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class GrabNotifier : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    public event Action<IXRSelectInteractor> OnGrab;
    public event Action<IXRSelectInteractor> OnRelease;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(args => OnGrab?.Invoke(args.interactorObject));
            grabInteractable.selectExited.AddListener(args => OnRelease?.Invoke(args.interactorObject));
        }
    }
}
