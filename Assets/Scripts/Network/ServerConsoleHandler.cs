using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ServerConsoleHandler : MonoBehaviour
{
#if UNITY_SERVER
    private readonly ConcurrentQueue<string> _consoleQueue = new();
    private int _hasPendingCommands_Interlocked;

    private void Update()
    {
        // Only enter if signaled
        if (Interlocked.Exchange(ref _hasPendingCommands_Interlocked, 0) == 1)
        {
            while (_consoleQueue.TryDequeue(out string command))
            {
                HandleCommand(command);
            }
        }
    }
    
    public void StartConsoleHandler()
    {
        Debug.Log("[SERVER] Starting console handler.");
        Task.Run(() =>
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    _consoleQueue.Enqueue(input.Trim());
                    Interlocked.Exchange(ref _hasPendingCommands_Interlocked, 1);
                }
            }
        });
    }

    private void HandleCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        string[] parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string cmd = parts[0].ToLower();
        
        switch (cmd)
        {
            case "pred":
            {
                SessionExperimentController _expController = FusionSessionManager.Instance.experimentController;
                if (parts.Length > 1)
                {
                    if (int.TryParse(parts[1], out int predMode))
                    {
                        _expController.CurrentCompensationMode = (ECompensationMode)Mathf.Clamp(predMode, 0, 4);
                    }
                }

                Debug.Log($"[SERVER] Prediction mode: {_expController.CurrentCompensationMode}");
                break;
            }

            case "task":
            {
                NetworkTaskManager _taskman = NetworkTaskManager.Instance;
                if (parts.Length > 1)
                {
                    if (int.TryParse(parts[1], out int taskType))
                    {
                        _taskman.RPC_ToggleTask(taskType);
                    }
                }

                Debug.Log($"[SERVER] Current task: {_taskman.GetCurrentTask()}");
                break;
            }

            default:
            {
                Debug.Log($"[SERVER] Unknown command: {cmd}");
                break;
            }
        }
    }

    /*
    private void ApplyNetworkConditions()
    {
        NetworkProjectConfig config = _sessionRunner.Config;
        if (config == null)
        {
            Debug.LogError("Unable to access NetworkProjectConfig!");
            return;
        }

        NetworkSimulationConfiguration nc = _sessionRunner.Config.NetworkConditions;
        if (nc == null)
        {
            Debug.LogError("Unable to access NetworkSimulationConfiguration!");
            return;
        }

        nc.Enabled = endToEndDelay > 0;
        nc.DelayMin = endToEndDelay;
        nc.DelayMax = endToEndDelay;
        nc.AdditionalJitter = 0; // no jitter; keep latency relatively stable
    }
    */
#endif
}
