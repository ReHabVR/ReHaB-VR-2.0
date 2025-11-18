using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CurrentTaskManager : MonoBehaviour
{
    public enum EGameState
    {
        None,
        Shapes,
        Dice,
        Sorting
    }

    public ControllerDevice XRPlayerController;
    public List<GameObject> taskButtons = new();
    public GameObject stopButton;

    private EGameState gameState = EGameState.None;
    private GameObject spawnedObjectReference;

    private int totalGrabsCount = -1;
    private int totalCorrectMoves = 0;
    private float startTime;
    private float endTime;

    private void Start()
    {
        foreach (GameObject go in taskButtons)
        {
            TaskButton taskButton = go.GetComponentInChildren<TaskButton>();
            taskButton.onRelease.AddListener(delegate{
                OnTaskButtonPressed(
                    taskButton.objectToSpawn, 
                    taskButton.spawnPositon.position, 
                    (EGameState)taskButton.taskId
                );
            });
        }
    }

    private void SpawnTask(GameObject objectToSpawn, Vector3 pos)
    {
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

        spawnedObjectReference = SpawnObject(objectToSpawn, pos);
        stopButton.SetActive(true);
    }

    private GameObject SpawnObject(GameObject obj, Vector3 pos)
    {
        GameObject spawnedObject = Instantiate(
            obj, 
            pos,
            obj.transform.rotation
        );
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
            Destroy(spawnedObjectReference);
        }    
    }

    public void OnTaskButtonPressed(GameObject objectToSpawn, Vector3 pos, EGameState newState)
    {
        totalGrabsCount = 0;
        totalCorrectMoves = 0;
        gameState = newState;
        SpawnTask(objectToSpawn, pos);

        switch(gameState)
        {
            case EGameState.Shapes:
            {
                ShapesManager gm = spawnedObjectReference.GetComponent<ShapesManager>();
                gm.anyShapePlaced.AddListener(IncrementGrabCount);
                gm.correctShapePlaced.AddListener(IncrementCorrectMovesCount);
                break;
            }
            case EGameState.Dice:
            {
                DiceManager gm = spawnedObjectReference.GetComponent<DiceManager>();
                gm.anyDiePlaced.AddListener(IncrementGrabCount);
                gm.correctDiePlaced.AddListener(IncrementCorrectMovesCount);
                break;
            }
            case EGameState.Sorting:
            {
                SortingTaskManager gm = spawnedObjectReference.GetComponent<SortingTaskManager>();
                gm.anyBallPlaced.AddListener(IncrementGrabCount);
                gm.correctBallPlaced.AddListener(IncrementCorrectMovesCount);
                break;
            }
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

        // Camera data
        string fovData = $"FOV: {XRPlayerController.renderCamera.GetFov()}";
        string heightData = $"Height offset: {XRPlayerController.cameraOffset.transform.position.y}";
        string distanceData = $"Distance offset: {XRPlayerController.cameraOffset.transform.position.z}";

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
            .AppendLine(fovData)
            .AppendLine(heightData)
            .AppendLine(distanceData)
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
