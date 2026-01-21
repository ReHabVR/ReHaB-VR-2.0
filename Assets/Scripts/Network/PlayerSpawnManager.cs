using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField]
    private List<Transform> _spawnPoints;

    public static PlayerSpawnManager Instance { get; private set; }

    public bool IsReady { get; private set; }
    
    private void Awake() 
    {
        if (Instance == null) 
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        IsReady = true;
    }

    public Transform GetSpawnPointForPlayer(int playerIndex)
    {
        if (_spawnPoints.Count == 0)
        {
            Debug.LogError("[PlayerSpawnManager] No spawn points assigned!");
            return null;
        }

        int clampIndex = Mathf.Clamp(playerIndex, 0, _spawnPoints.Count - 1); 
        Transform spawnPoint = _spawnPoints[clampIndex];
        if (spawnPoint == null) 
        {
            Debug.LogError($"[PlayerSpawnManager] Spawn point at index {clampIndex} is null!");
            return null;
        }
        
        return spawnPoint.transform;
    }
}
