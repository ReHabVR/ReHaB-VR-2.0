using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SessionExperimentController : NetworkBehaviour
{
    [Networked] 
    public int Latency { get; set; }
    [Networked] 
    public int Jitter { get; set; }

    [Networked] 
    public PredictionMode CurrentPredictionMode { get; set; } = PredictionMode.Fusion;

    [Networked] 
    public int ExperimentID { get; set; }

    private int _prevLatency = -1;
    private int _prevJitter = -1;
    private PredictionMode _prevPredMode = (PredictionMode)(-1);

    private bool IsReady { get; set; }

    public override void Spawned()
    {
        IsReady = true;
    }

    public override void Render()
    {
        if (!IsReady || Runner == null || Runner.IsServer)
        {
            return;
        }

        int latency = Latency;
        int jitter = Jitter;
        PredictionMode predMode = CurrentPredictionMode;

        if (latency != _prevLatency || jitter != _prevJitter)
        {
            ApplyLatency(latency, jitter);
            _prevLatency = latency;
            _prevJitter = jitter;
        }

        if (CurrentPredictionMode != _prevPredMode)
        {
            Debug.Log($"Updated prediction mode to {CurrentPredictionMode}");
            _prevPredMode = CurrentPredictionMode;
        }
    }

    private void ApplyLatency(int latency, int jitter)
    {
        if (Runner.Config == null || Runner.Config.NetworkConditions == null)
        {
            return;
        }

        Runner.Config.NetworkConditions.Enabled = latency > 0;
        Runner.Config.NetworkConditions.DelayMin = latency;
        Runner.Config.NetworkConditions.DelayMax = latency;
        Runner.Config.NetworkConditions.AdditionalJitter = jitter;

        Debug.Log($"Applied latency {Latency} ms | jitter {Jitter} ms");
    }
}
