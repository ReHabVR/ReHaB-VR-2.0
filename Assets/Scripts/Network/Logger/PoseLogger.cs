using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Fusion;
using UnityEngine;

public class PoseLogger : MonoBehaviour
{
    [SerializeField] 
    private ExternalPoseProvider poseProvider;

    private NetworkTaskManager _taskman;
    private NetworkObject _netObj;

    private readonly List<string> _buffer = new();

    [SerializeField]
    private float _flushInterval = 5f;

    private float _nextFlushTime;
    private float _startTime;

    private string _path;
    private string _header;
    
    private bool _isLocal;
    private bool _taskStarted;

    private void Start()
    {
        _taskman = NetworkTaskManager.Instance;
        if (_taskman == null)
        {
            Debug.LogError("NetworkTaskManager not found!");
            return;
        }

        _netObj = GetComponentInParent<NetworkObject>();
        //TODO: for testing purposes only, remove once confirming logging works everywhere
        //_isLocal = _netObj != null && _netObj.HasInputAuthority;
        _isLocal = true; 

        if (!_isLocal || poseProvider == null)
        {
            return;
        }

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
        if (_isLocal && _taskStarted)
        {
            RecordPose();
            if (Time.time >= _nextFlushTime)
            {
                FlushBuffer();
                _nextFlushTime = Time.time + _flushInterval;
            }
        }
    }

    private void RecordPose()
    {
        poseProvider.headBridge.GetLocalPositionAndRotation(out Vector3 head, out Quaternion headRot);
        poseProvider.lhandBridge.GetLocalPositionAndRotation(out Vector3 lhand, out Quaternion lhandRot);
        poseProvider.rhandBridge.GetLocalPositionAndRotation(out Vector3 rhand, out Quaternion rhandRot);
        
        float gripL = poseProvider.GripL;
        float gripR = poseProvider.GripR;

        float timestamp = Time.time - _startTime;

        string row = string.Join(",",
            FormatFloat(timestamp),
            FormatVector3(head), FormatQuaternion(headRot),
            FormatVector3(lhand), FormatQuaternion(lhandRot),
            FormatVector3(rhand), FormatQuaternion(rhandRot),
            FormatFloat(gripL), FormatFloat(gripR)
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
        string fname = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_Player{playerID}_PoseLog.csv";

        _path = Path.Combine(Application.persistentDataPath, fname);
        _header = "time,head_x,head_y,head_z,head_qx,head_qy,head_qz,head_qw," +
                  "lhand_x,lhand_y,lhand_z,lhand_qx,lhand_qy,lhand_qz,lhand_qw," +
                  "rhand_x,rhand_y,rhand_z,rhand_qx,rhand_qy,rhand_qz,rhand_qw" +
                  "grip_l, grip_r";

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
