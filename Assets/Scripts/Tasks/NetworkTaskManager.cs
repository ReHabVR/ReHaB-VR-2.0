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

    private ECurrentTask currentTask = ECurrentTask.None;
    private NetworkObject spawnedObjectReference;

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

    public ECurrentTask GetCurrentTask() => currentTask;

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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TaskStarted(ECurrentTask newTask)
    {
        currentTask = newTask;
        OnTaskStarted?.Invoke();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TaskStopped()
    {
        OnTaskStopped?.Invoke();
        currentTask = ECurrentTask.None;
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

        spawnedObjectReference = Runner.Spawn(
            objectToSpawn, 
            spawnPoint.position,
            spawnPoint.rotation
        );
    }

    private void DeleteCurrentTask()
    {
        if (spawnedObjectReference != null) 
        {
            if (Runner.IsServer)
            {
                Runner.Despawn(spawnedObjectReference);
            }

            spawnedObjectReference = null;
        }
    }

    private void IncrementGrabCount() => OnMove?.Invoke();
    private void IncrementCorrectMovesCount() => OnCorrectMove?.Invoke();

    
    #region DEBUG
    public void DebugMove(bool correctMove)
        {
            IncrementGrabCount();
            if (correctMove)
            {
                IncrementCorrectMovesCount();
            }
        }
    #endregion
}
