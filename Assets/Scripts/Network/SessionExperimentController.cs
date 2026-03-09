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

    public override void Render()
    {
        if (Runner.IsServer)
        {
            return;
        }

        if (Latency != _prevLatency || Jitter != _prevJitter)
        {
            ApplyLatency();
            _prevLatency = Latency;
            _prevJitter = Jitter;
        }

        if (CurrentPredictionMode != _prevPredMode)
        {
            Debug.Log($"Updated prediction mode to {CurrentPredictionMode}");
            _prevPredMode = CurrentPredictionMode;
        }
    }

    private void ApplyLatency()
    {
        if (Runner.Config == null || Runner.Config.NetworkConditions == null)
        {
            return;
        }

        Runner.Config.NetworkConditions.Enabled = Latency > 0;
        Runner.Config.NetworkConditions.DelayMin = Latency;
        Runner.Config.NetworkConditions.DelayMax = Latency;
        Runner.Config.NetworkConditions.AdditionalJitter = Jitter;

        Debug.Log($"Applied latency {Latency} ms | jitter {Jitter} ms");
    }
}
