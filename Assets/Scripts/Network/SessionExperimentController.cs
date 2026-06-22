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
    public ECompensationMode CurrentCompensationMode { get; set; } = ECompensationMode.None;

    [Networked] 
    public int ExperimentID { get; set; }

    private int _prevLatency = -1;
    private int _prevJitter = -1;
    //private CompensationMode _prevPredMode = (CompensationMode)(-1);

    public override void Spawned()
    {
        // Ensure initial state
        if (Object.HasStateAuthority)
        {
            ApplyNetworkConditions(Latency, Jitter);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            // Apply only when values change
            if (Latency == _prevLatency && Jitter == _prevJitter)
            {
                return;
            }

            ApplyNetworkConditions(Latency, Jitter);
            _prevLatency = Latency;
            _prevJitter = Jitter;
        }
    }

    private void ApplyNetworkConditions(int latency, int jitter)
    {
        if (Runner == null || Runner.Config == null || Runner.Config.NetworkConditions == null)
        {
            return;
        }

        NetworkSimulationConfiguration nc = Runner.Config.NetworkConditions;
        nc.Enabled = latency > 0;
        nc.DelayMin = latency;
        nc.DelayMax = latency;
        nc.AdditionalJitter = jitter;

        Debug.Log($"Applied latency {latency} ms | jitter {jitter} ms");
    }
}
