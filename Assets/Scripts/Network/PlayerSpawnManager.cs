using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField]
    private List<Transform> _spawnPoints;

    public Transform GetSpawnPointForPlayer(PlayerRef player)
    {
        int playerIndex = player.AsIndex; // 0 for host, 1 for first client
        int clampIndex = Mathf.Clamp(playerIndex, 0, _spawnPoints.Count - 1);

        return _spawnPoints[clampIndex].transform;
    }
}
