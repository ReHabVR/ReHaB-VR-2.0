using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class ServerLatencyController : NetworkBehaviour
{
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetSimulatedLatency(int latency, int jitter)
    {
        if (Runner.IsServer)
        {
            return;
        }

        if (Runner.Config == null)
        {
            Debug.LogError($"Could not access Runner.Config!");
            return;
        }

        if (Runner.Config.NetworkConditions == null)
        {
            Debug.LogError($"Could not access Runner.Config.NetworkConditions!");
            return;
        }

        Runner.Config.NetworkConditions.Enabled = latency > 0;
        Runner.Config.NetworkConditions.DelayMin = latency;
        Runner.Config.NetworkConditions.DelayMax = latency;
        Runner.Config.NetworkConditions.AdditionalJitter = jitter;

        Debug.Log($"[CLIENT] Applied latency {latency} ms | jitter {jitter} ms");
    }
}
