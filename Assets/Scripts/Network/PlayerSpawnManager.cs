using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField]
    private List<Transform> _spawnPoints;

    public static PlayerSpawnManager Instance { get; private set; }
    
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

    public Transform GetSpawnPointForPlayer(int playerIndex)
    {
        int clampIndex = Mathf.Clamp(playerIndex, 0, _spawnPoints.Count - 1);
        return _spawnPoints[clampIndex].transform;
    }
}
