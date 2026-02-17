using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class CurrentTaskManager : NetworkBehaviour
{
    public enum EGameState
    {
        None,
        Shapes,
        Dice,
        Sorting
    }
    public List<GameObject> taskButtons = new();
    public GameObject stopButton;

    private EGameState gameState = EGameState.None;
    private NetworkObject spawnedObjectReference;

    private int totalGrabsCount = -1;
    private int totalCorrectMoves = 0;
    private float startTime;
    private float endTime;

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
            taskButton.onRelease.AddListener(delegate{
                OnTaskButtonPressed(
                    taskButton.objectToSpawn, 
                    taskButton.spawnPoint, 
                    (EGameState)taskButton.taskId
                );
            });
        }
    }

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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_DebugMove(bool correctMove)
    {
        if (!Object.HasStateAuthority) 
        {
            return;
        }

        IncrementGrabCount();
        if (correctMove)
        {
            IncrementCorrectMovesCount();
        }
    }
    #endregion

    private void SpawnTask(NetworkObject objectToSpawn, Transform spawnPoint)
    {
        // Only server should spawn network objects
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

        spawnedObjectReference = SpawnObject(objectToSpawn, spawnPoint);
        stopButton.SetActive(true);
    }

    private NetworkObject SpawnObject(NetworkObject obj, Transform spawnPoint)
    {
        NetworkObject spawnedObject = Runner.Spawn(
            obj, 
            spawnPoint.position,
            spawnPoint.rotation
            //null,
            //(runner, instance) =>
            //{
                //instance.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            //}
        );
        Debug.LogError($"Spawned at {spawnedObject.transform.position}");
        return spawnedObject;
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
            if (spawnedObjectReference.TryGetComponent<IMinigameManager>(out IMinigameManager manager))
            {
                manager.OnMove -= IncrementGrabCount;
                manager.OnCorrectMove -= IncrementCorrectMovesCount;
            }

            Runner.Despawn(spawnedObjectReference);
            spawnedObjectReference = null;
        }
    }

    public void OnTaskButtonPressed(NetworkObject objectToSpawn, Transform spawnPoint, EGameState newState)
    {
        totalGrabsCount = 0;
        totalCorrectMoves = 0;
        gameState = newState;
        
        SpawnTask(objectToSpawn, spawnPoint);
        if (spawnedObjectReference.TryGetComponent<IMinigameManager>(out var manager))
        {
            manager.OnMove += IncrementGrabCount;
            manager.OnCorrectMove += IncrementCorrectMovesCount;
        }

        startTime = Time.time;
    }

    public void OnStopButtonPressed()
    {
        endTime = Time.time;
        DeleteAllTasks();
        SaveResults();
        gameState = EGameState.None;
    }

    private void IncrementGrabCount() 
    {
        totalGrabsCount++;
    }

    private void IncrementCorrectMovesCount() 
    {
        totalCorrectMoves++; 
    }

    private void SaveResults() 
    {
        string fname =  DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + Enum.GetName(typeof(EGameState), (int)gameState) + ".txt";
        string path = Path.Combine(Application.persistentDataPath, fname);

        // Task data
        string timeElapsed = $"Time elapsed: {endTime - startTime:F2} sec.";
        string totalMoves = $"Total moves: {totalGrabsCount}";
        string correctMoves = $"Correct moves: {totalCorrectMoves}";
        string accuracy = "";
        if (totalGrabsCount > 0)
        {
            accuracy = $"Accuracy: {100.0f * totalCorrectMoves / (totalGrabsCount * 1.0f):F2}%";
        }

        StringBuilder sb = new StringBuilder()
            .AppendLine(timeElapsed)
            .AppendLine(totalMoves)
            .AppendLine(correctMoves)
            .AppendLine(accuracy);

        using (StreamWriter file = new(File.Open(path, FileMode.Append))) 
        {
            file.Write(sb.ToString());
        }

        Debug.Log(sb.ToString());
    }
}
