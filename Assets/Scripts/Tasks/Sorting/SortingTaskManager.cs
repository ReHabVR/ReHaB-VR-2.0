using System;
using Fusion;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SortingTaskManager : NetworkBehaviour, IMinigameManager
{
    public event Action OnMove;
    public event Action OnCorrectMove;

    [SerializeField]
    private GameObject ballManager;
    
    [SerializeField]
    private BoxCollider detectorBlue;
    [SerializeField]
    private BoxCollider detectorRed;
    
    private const string TAG = "Ball";


    private void Start()
    {
        AssignProperties();
        ConnectNotifiers();
    }
    
    public void AssignProperties() 
    {
        foreach (Transform t in ballManager.transform)
        {
            GameObject go = t.gameObject;
            go.GetComponent<GrabNotifier>().OnRelease += OnObjectPlaced;
        }
    }

    public void ConnectNotifiers() 
    {
        detectorBlue.GetComponent<CollisionNotifier>().collisionDetected.AddListener(OnCollision);
        detectorRed.GetComponent<CollisionNotifier>().collisionDetected.AddListener(OnCollision);
    }

    public void OnObjectPlaced(IXRSelectInteractor _interactor)
    {
        if (Object.HasStateAuthority)
        {
            OnMove?.Invoke();
        }
    }

    public void OnCollision(GameObject go, int colliderId)
    {
        if (!go.CompareTag(TAG))
            return;
        
        if (go.GetComponent<BallColor>().ColorID == colliderId)
        {
            OnCorrectMove?.Invoke();
        }
    }
}
