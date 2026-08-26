using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ServerConsoleHandler : MonoBehaviour
{
    private NetworkTaskManager _taskman;
    
    private SessionExperimentController _expController;

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

        if (NetworkTaskManager.Instance == null)
        {
            Debug.LogError("[SERVER] Failed to start console handler: NetworkTaskManager not found!");
            return;
        }
        _taskman = NetworkTaskManager.Instance;

        if (FusionSessionManager.Instance == null)
        {
            Debug.LogError("[SERVER] Failed to start console handler: FusionSessionManager not found!");
            return;
        }

        if (FusionSessionManager.Instance.experimentController == null)
        {
            Debug.LogError("[SERVER] Failed to start console handler: SessionExperimentController not found!");
        }
        _expController = FusionSessionManager.Instance.experimentController;

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
}
