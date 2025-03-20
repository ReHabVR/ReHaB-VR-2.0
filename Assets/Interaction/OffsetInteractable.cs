using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Interaction
{
    public class OffsetInteractable : XRGrabInteractable
    {
        private bool isAttachCreated = false;
        
        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            if (!isAttachCreated) CreateAttach();
            base.OnSelectEntering(args);
            MatchAttachmentPoints(args.interactorObject);
        }

        protected void MatchAttachmentPoints(IXRInteractor interactor)
        {
            if(IsFirstSelecting(interactor))
            {
                bool IsDirect = interactor is XRDirectInteractor;
                attachTransform.position = IsDirect ? interactor.GetAttachTransform(this).position : transform.position;
                attachTransform.rotation = IsDirect ? interactor.GetAttachTransform(this).rotation : transform.rotation;
            }
        }
        private void CreateAttach()
        {
            var attachObject = new GameObject("Attach");
            attachObject.transform.SetParent(transform);
            attachObject.transform.localPosition = Vector3.zero;
            attachObject.transform.localRotation = Quaternion.identity;
        
            attachTransform = attachObject.transform;
        }
        private bool IsFirstSelecting(IXRInteractor interactor)
        {
            return interactor == firstInteractorSelecting;
        }
    }
}