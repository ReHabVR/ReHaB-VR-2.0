using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Fusion;
using UnityEngine;

public class CurrentTaskManager : NetworkBehaviour
{
    public enum EGameState
    {
        None = 0,
        Shapes = 1,
        Dice = 2,
        Sorting = 3
    }

    public event Action OnTaskStarted;
    public event Action OnMove;
    public event Action OnCorrectMove;
    public event Action OnTaskStopped;

    public List<GameObject> taskButtons = new();
    public GameObject stopButton;

    private EGameState gameState = EGameState.None;
    private NetworkObject spawnedObjectReference;

    public static CurrentTaskManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (GameObject go in taskButtons)
        {
            TaskButton taskButton = go.GetComponentInChildren<TaskButton>();
            taskButton.onRelease.AddListener(delegate {
                if (Runner.IsServer) {
                    OnTaskButtonPressed(
                        taskButton.objectToSpawn, 
                        taskButton.spawnPoint, 
                        (EGameState)taskButton.taskId
                    );
                }
            });
        }
    }

    public EGameState GetGameState() => gameState;
    
#region DEBUG
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ToggleTestTask()
    {
        if (!Object.HasStateAuthority) 
        {
            return;
        }

        if (gameState == EGameState.None)
        {
            TaskButton sortingTask = taskButtons[0].GetComponentInChildren<TaskButton>();
            OnTaskButtonPressed(
                sortingTask.objectToSpawn,
                sortingTask.spawnPoint,
                EGameState.Sorting
            );
        }
        else if (gameState == EGameState.Sorting)
        {
            OnStopButtonPressed();
        }
    }

    public void DebugMove(bool correctMove)
    {
        IncrementGrabCount();
        if (correctMove)
        {
            IncrementCorrectMovesCount();
        }
    }
#endregion

    private void SpawnTask(NetworkObject objectToSpawn, Transform spawnPoint)
    {
        // Only server spawns network objects
        if (!Runner.IsServer) 
        {
            return;
        }

        if (objectToSpawn == null)  
        {
            Debug.LogWarning("No object to spawn defined.");
            return;
        }

        // Disable all task buttons during game
        foreach (GameObject button in taskButtons) 
        {
            button.SetActive(false);
        }

        spawnedObjectReference = Runner.Spawn(
            objectToSpawn, 
            spawnPoint.position,
            spawnPoint.rotation
        );

        stopButton.SetActive(true);
    }

    private void DeleteAllTasks()
    {
        stopButton.SetActive(false);
        foreach (GameObject button in taskButtons) 
        {
            button.SetActive(true);
        }

        if (spawnedObjectReference != null) 
        {
            if (Runner.IsServer)
            {
                Runner.Despawn(spawnedObjectReference);
            }

            spawnedObjectReference = null;
        }
    }

    public void OnTaskButtonPressed(NetworkObject objectToSpawn, Transform spawnPoint, EGameState newState)
    {
        if (Runner.IsServer)
        {
            SpawnTask(objectToSpawn, spawnPoint);
            RPC_TaskStarted(newState);
        }
    }
    
    public void OnStopButtonPressed()
    {
        if (Runner.IsServer)
        {
            DeleteAllTasks();
            RPC_TaskStopped();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TaskStarted(EGameState newState)
    {
        gameState = newState;
        OnTaskStarted?.Invoke();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TaskStopped()
    {
        OnTaskStopped?.Invoke();
        gameState = EGameState.None;
    }


    private void IncrementGrabCount() => OnMove?.Invoke();
    private void IncrementCorrectMovesCount() => OnCorrectMove?.Invoke();
}
