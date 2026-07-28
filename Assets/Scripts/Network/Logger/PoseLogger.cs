using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Fusion;
using UnityEngine;

public class PoseLogger : MonoBehaviour
{
    private NetworkTaskManager _taskman;
    private NetworkObject _netObj;
    private NetworkRunner _runner;

    private readonly List<string> _buffer = new();

    [SerializeField]
    private float _flushInterval = 5f;

    private float _nextFlushTime;
    private float _startTime;

    private string _path;
    private string _header;
    
    private bool _taskStarted;
    
    public bool IsLocal { get; set; }

    public void Initialize(NetworkTaskManager taskManager, bool isLocal)
    {
        _netObj = GetComponentInParent<NetworkObject>();
        _runner = _netObj.Runner;

        IsLocal = isLocal;
        _taskman = taskManager;

        _taskman.OnTaskStarted += OnTaskStarted;
        _taskman.OnTaskStopped += OnTaskStopped;
    }

    private void OnDestroy()
    {
        FlushBuffer();
        if (_taskman) 
        {
            _taskman.OnTaskStarted -= OnTaskStarted;
            _taskman.OnTaskStopped -= OnTaskStopped;
        }
    }

    private void LateUpdate()
    {
        if (_taskStarted && Time.time >= _nextFlushTime)
        {
            FlushBuffer();
            _nextFlushTime = Time.time + _flushInterval;
        }
    }

    public void RecordPose(PoseData pose)
    {
        if (!_taskStarted) // Do not log pose outside of tasks
        {
            return;
        }

        int tick = _runner.Tick.Raw; // raw frame number
        float renderTime = _runner.LocalRenderTime;
        float taskTime = Time.time - _startTime;

        string row = string.Join(",",
            tick, FormatFloat(renderTime), FormatFloat(taskTime),
            FormatVector3(pose.headPos), FormatQuaternion(pose.headRot),
            FormatVector3(pose.lhandPos), FormatQuaternion(pose.lhandRot),
            FormatVector3(pose.rhandPos), FormatQuaternion(pose.rhandRot),
            FormatFloat(pose.gripL), FormatFloat(pose.gripR)
        );

        _buffer.Add(row);
    }

    public void FlushBuffer()
    {
        if (_buffer.Count == 0)
        {
            return;
        }

        File.AppendAllLines(_path, _buffer);
        _buffer.Clear();
    }

    public void OnTaskStarted()
    {
        _buffer.Clear();
        _startTime = Time.time;
        _nextFlushTime = _startTime + _flushInterval;
        _taskStarted = true;

        string playerID = _netObj.InputAuthority.RawEncoded.ToString();
        string peerType = IsLocal ? "Local" : "Remote";
        string fname = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_Player{playerID}_{peerType}_PoseLog.csv";

        _path = Path.Combine(Application.persistentDataPath, fname);
        _header = "tick,render_time,task_time," + 
                "head_x,head_y,head_z,head_qx,head_qy,head_qz,head_qw," +
                "lhand_x,lhand_y,lhand_z,lhand_qx,lhand_qy,lhand_qz,lhand_qw," +
                "rhand_x,rhand_y,rhand_z,rhand_qx,rhand_qy,rhand_qz,rhand_qw," +
                "grip_l,grip_r";

        // Write header immediately
        File.WriteAllText(_path, _header + Environment.NewLine);
    }

    public void OnTaskStopped()
    {
        _taskStarted = false;
        FlushBuffer();
    }

    private static string FormatFloat(float value) => value.ToString("F4", CultureInfo.InvariantCulture);

    private static string FormatVector3(Vector3 v) => $"{FormatFloat(v.x)},{FormatFloat(v.y)},{FormatFloat(v.z)}";

    private static string FormatQuaternion(Quaternion q) => $"{FormatFloat(q.x)},{FormatFloat(q.y)},{FormatFloat(q.z)},{FormatFloat(q.w)}";
}
