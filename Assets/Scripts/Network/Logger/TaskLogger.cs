using System;
using System.Globalization;
using System.IO;
using Fusion;
using UnityEngine;

public class TaskLogger : MonoBehaviour
{
    private NetworkTaskManager _taskman;

    private float _startTime;
    private float _endTime;

    private int _grabsCount = -1;
    private int _correctMoves = 0;

    private bool _isLocal;


    void Start()
    {
        _taskman = NetworkTaskManager.Instance;
        
        if (_taskman == null)
        {
            Debug.LogError("CurrentTaskManager not found!");
            return;
        }

        NetworkObject netObj = GetComponentInParent<NetworkObject>();
        _isLocal = netObj != null && netObj.HasInputAuthority;

        if (!_isLocal)
        {
            return;
        }

        _taskman.OnMove += OnMove;
        _taskman.OnCorrectMove += OnCorrectMove;
        _taskman.OnTaskStarted += OnTaskStarted;
        _taskman.OnTaskStopped += OnTaskStopped;
    }

    private void OnDestroy()
    {
        if (_taskman == null) 
        {
            return;
        }

        _taskman.OnMove -= OnMove;
        _taskman.OnCorrectMove -= OnCorrectMove;
        _taskman.OnTaskStarted -= OnTaskStarted;
        _taskman.OnTaskStopped -= OnTaskStopped;
    }

    private void OnMove() => _grabsCount++;

    private void OnCorrectMove() => _correctMoves++;

    private void OnTaskStarted()
    {
        _grabsCount = 0;
        _correctMoves = 0;
        _startTime = Time.time;   
    }

    private void OnTaskStopped()
    {
        _endTime = Time.time;
        
        if (_isLocal) 
        {
            SaveResults();
        }
    }

    private void SaveResults() 
    {
        int playerID = GetComponentInParent<NetworkObject>().Runner.LocalPlayer.RawEncoded;
        string fname = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_Player{playerID}_{Enum.GetName(typeof(NetworkTaskManager.ECurrentTask), (int)_taskman.GetCurrentTask())}.txt";
        string path = Path.Combine(Application.persistentDataPath, fname);

        string header = "time_elapsed,total_moves,correct_moves,accuracy";
        string row = string.Join(",",
            (_endTime - _startTime).ToString("F3", CultureInfo.InvariantCulture), //time_elapsed
            _grabsCount.ToString(), //total_moves
            _correctMoves.ToString(), //correct_moves
            _grabsCount > 0 ? (100.0f * _correctMoves / (_grabsCount * 1.0f)).ToString("F2", CultureInfo.InvariantCulture) : "0.0" //accuracy
        );

        using (StreamWriter file = new(File.Open(path, FileMode.Append))) 
        {
            file.WriteLine(header);
            file.Write(row);
        }

        Debug.Log($"Task log for Player {playerID}: {row}");
    }
}
