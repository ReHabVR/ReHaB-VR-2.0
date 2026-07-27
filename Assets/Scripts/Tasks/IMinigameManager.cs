using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public interface IMinigameManager
{    
    /// Called whenever the player performs any action that counts as a move
    public event Action OnMove;

    /// Called whenever the player performs a correct move
    public event Action OnCorrectMove;

    public void AssignProperties();
    public void ConnectNotifiers();
    public void OnObjectPlaced(IXRSelectInteractor _interactor);
    public void OnCollision(GameObject go, int colliderId);
}
