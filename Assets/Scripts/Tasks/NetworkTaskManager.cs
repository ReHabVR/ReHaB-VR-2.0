using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkTaskManager : NetworkBehaviour
{
    public enum ECurrentTask
    {
        None = 0,
        Sorting = 1,
        Stacking = 2
    }

    public event Action OnTaskStarted;
    public event Action OnMove;
    public event Action OnCorrectMove;
    public event Action OnTaskStopped;

    public List<NetworkObject> taskPrefabs = new();
    public List<Transform> taskSpawnPositions = new();

    private ECurrentTask _currentTask = ECurrentTask.None;
    private NetworkObject _spawnedObjectReference;
    private IMinigameManager _currentTaskManager;

    public static NetworkTaskManager Instance { get; private set; }

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

    public ECurrentTask GetCurrentTask() => _currentTask;

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ToggleTask(int id)
    {
        if (!Object.HasStateAuthority) 
        {
            return;
        }
        
        if (id < 0 || id > taskPrefabs.Count)
        { 
            return;
        }

        DeleteCurrentTask();
        RPC_TaskStopped();
        
        if (id != 0)
        {
            NetworkObject objectToSpawn = taskPrefabs[id - 1];
            if (objectToSpawn == null)
            {
                Debug.LogWarning($"[SERVER] No task prefab assigned for ID {id}!");
                return;
            }

            Transform spawnPoint = taskSpawnPositions[id - 1];
            if (spawnPoint == null)
            {     
                Debug.LogWarning($"[SERVER] Spawn point for task ID {id} not found!");
                return;
            }

            SpawnTask(objectToSpawn, spawnPoint);
            RPC_TaskStarted((ECurrentTask)id);
        }        
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TaskStarted(ECurrentTask newTask)
    {
        _currentTask = newTask;
        OnTaskStarted?.Invoke();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TaskStopped()
    {
        OnTaskStopped?.Invoke();
        _currentTask = ECurrentTask.None;
    }

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

        _spawnedObjectReference = Runner.Spawn(
            objectToSpawn, 
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (_spawnedObjectReference == null)
        {
            Debug.LogError("Unable to spawn task!");
            return;
        }

        _currentTaskManager = _spawnedObjectReference.GetComponent<IMinigameManager>();
        if (_currentTaskManager != null)
        {
            _currentTaskManager.OnMove += HandleMove;
            _currentTaskManager.OnCorrectMove += HandleCorrectMove;
        }

    }

    private void DeleteCurrentTask()
    {
        if (_currentTaskManager != null)
        {
            _currentTaskManager.OnMove -= HandleMove;
            _currentTaskManager.OnCorrectMove -= HandleCorrectMove;
            _currentTaskManager = null;
        }

        if (_spawnedObjectReference != null && Runner.IsServer)
        {
            Runner.Despawn(_spawnedObjectReference);
            _spawnedObjectReference = null;
        }
    }

    private void HandleMove() => OnMove?.Invoke();
    private void HandleCorrectMove() => OnCorrectMove?.Invoke();
    
    #region DEBUG
    public void DebugMove(bool correctMove)
    {
        HandleMove();
        if (correctMove)
        {
            HandleCorrectMove();
        }
    }
    #endregion
}
