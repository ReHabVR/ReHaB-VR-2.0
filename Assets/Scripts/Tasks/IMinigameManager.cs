using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public interface IMinigameManager
{
    public void AssignProperties();
    public void ConnectNotifiers();
    public void OnObjectPlaced(IXRSelectInteractor _interactor);
    public void OnCollision(GameObject go, int colliderId);
}
