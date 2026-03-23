using System;
using Fusion;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SortingTaskManager : NetworkBehaviour, IMinigameManager
{
    public event Action OnMove;
    public event Action OnCorrectMove;
    
    private const string TAG = "Ball";

    [SerializeField] 
    private SortingSetup sortingSetup;
    
    [SerializeField]
    private BoxCollider detectorBlue;
    [SerializeField]
    private BoxCollider detectorRed;

    private void Start()
    {
        sortingSetup.OnBallsSpawned += ConnectNotifiers;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        foreach (NetworkObject no in sortingSetup.SpawnedBalls)
        {
            if (no.TryGetComponent(out GrabNotifier grab))
            {
                grab.OnRelease -= OnObjectPlaced;
            }
        }
    }

    public void AssignProperties()
    {
        return;
    }

    public void ConnectNotifiers() 
    {
        foreach (NetworkObject no in sortingSetup.SpawnedBalls)
        {
            if (no.TryGetComponent(out GrabNotifier grab))
            {
                grab.OnRelease += OnObjectPlaced;
            }
        }

        //detectorBlue.GetComponent<CollisionNotifier>().collisionDetected.AddListener(OnCollision);
        //detectorRed.GetComponent<CollisionNotifier>().collisionDetected.AddListener(OnCollision);
    }

    public void OnObjectPlaced(IXRSelectInteractor interactor)
    {
        OnMove?.Invoke();

        // HACK: Physics are server sided, so we cannot use OnCollision on clients
        if (interactor.firstInteractableSelected.transform.TryGetComponent(out BallColor ball))
        {
            Vector3 ballPos = ball.transform.position;
            int zoneID = -1;
            if (detectorBlue.bounds.Contains(ballPos))
            {
                zoneID = 0;
            }
            if (detectorRed.bounds.Contains(ballPos))
            {
                zoneID = 1;
            }
            if (zoneID != -1 && ball.ColorID == zoneID)
            {
                OnCorrectMove?.Invoke();
            }
        }
    }

    // This fires only on server.
    public void OnCollision(GameObject _go, int _colliderId)
    {
        return; // Temporary solution
        /*
        if (!go.CompareTag(TAG))
        {
            return;
        }
        
        if (go.GetComponent<BallColor>().ColorID == colliderId)
        {
            OnCorrectMove?.Invoke();
        }
        */
    }
}
