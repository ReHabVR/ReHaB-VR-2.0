using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FusionSessionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Initialization")]

    [Tooltip("LAN host IP")]
    public string hostAddress = "192.168.1.12"; 

    [Tooltip("Needs to match build index.")]
    public int mainSceneIndex = 1;

    public ushort port = 27015;

    [Tooltip("PC = host, HMD = client")]
    public bool isHost;

    [Header("Session Settings")]

    [SerializeField]
    private NetworkObject playerPrefab;

    private NetworkRunner _runner;
    private NetworkSceneManagerDefault _sceneManager;

    private readonly Dictionary<PlayerRef, List<NetworkObject>> _playerObjects = new();
    private readonly List<PlayerRef> _activePlayers = new();
    private bool _sceneLoaded = false;

    public static FusionSessionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
#if UNITY_EDITOR
        isHost = EditorPrefs.GetBool("StartAsHost", true);
#endif
    }

    private async void Start()
    {
        Debug.LogWarning($"Starting as: {(isHost ? "SERVER" : "CLIENT")}");

        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = !isHost;
        _runner.AddCallbacks(this);
        
        _sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        Debug.LogWarning($"SceneManager: {_sceneManager}");
        Debug.LogWarning($"runner.ProvideInput: {_runner.ProvideInput}");

        StartGameArgs args = new()
        {
            GameMode = isHost ? GameMode.Server : GameMode.Client,
            Address = isHost ? NetAddress.Any(port) : NetAddress.CreateFromIpPort(hostAddress, port),
            Scene = SceneRef.FromIndex(mainSceneIndex),
            SceneManager = _sceneManager,
            DisableNATPunchthrough = true
        };
        Debug.LogWarning(args);

        StartGameResult result = await _runner.StartGame(args);
        if (result.Ok)
        {
            Debug.LogWarning($"Dedicated server started on {hostAddress}:{port}.");
        }
        else
        {
            Debug.LogError("Connection failed: " + result.ShutdownReason);
            // TODO: show a retry popup
        }
    }
    
#region INetworkRunnerCallbacks
    public void OnConnectedToServer(NetworkRunner runner)
    {
        //throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        //throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        //throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        //throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        //throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        //throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        //throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        //throw new NotImplementedException();
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        //throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // Authority check.
        if (!runner.IsServer) 
        {
            return;
        }

        // Don't spawn a player for the host/server.
        if (runner.IsServer && player == runner.LocalPlayer)
        {    
            return;
        }

        // Register player
        if (!_activePlayers.Contains(player))
        {
            _activePlayers.Add(player);    
        }
        
        StartCoroutine(
            SpawnDeferred(player)
        );
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Authority check.
        if (!runner.IsServer) 
        {
            return;
        }

        if (_playerObjects.TryGetValue(player, out var list))
        {
            foreach (NetworkObject no in list)
            {
                if (no != null && no.Runner != null)
                {
                    runner.Despawn(no);
                }
            }

            _playerObjects.Remove(player);
        }
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        //throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        //throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        _sceneLoaded = true;
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Application.logMessageReceived += (log, stack, type) =>
        {
            if (type == UnityEngine.LogType.Exception)
                Debug.LogError("Exception detected: " + log + "\n" + stack);
        };
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        //throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        //throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        //throw new NotImplementedException();
    }
    #endregion

    private IEnumerator SpawnDeferred(PlayerRef player)
    {
        yield return new WaitUntil(() => _sceneLoaded);
        yield return new WaitForEndOfFrame();

        int playerIndex = _activePlayers.IndexOf(player);

        Transform spawn = PlayerSpawnManager.Instance.GetSpawnPointForPlayer(playerIndex);
        if (spawn)
        {
            NetworkObject spawnedPlayer = _runner.Spawn(
                playerPrefab,
                spawn.position,
                spawn.rotation,
                player
            );
            
            if (spawnedPlayer)
            {
                if (!_playerObjects.TryGetValue(player, out var list))
                {
                    list = new List<NetworkObject>();
                    _playerObjects[player] = list;
                }

                list.Add(spawnedPlayer);
            }
            else
            {
                Debug.LogError("Failed to spawn player for " + player);
            }    
        }
    }
}
