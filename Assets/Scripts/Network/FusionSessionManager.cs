using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections;
using System.IO;
#if UNITY_SERVER
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
#endif

public enum PlayerRole
{
    Player,
    Trainer
}

public class FusionSessionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Initialization")]
    public string hostAddress = "192.168.1.12"; 

    [HideInInspector]
    public int mainSceneIndex = 1;

    public ushort port = 27015;

    [Header("Simulated Network Latency")]
    [Range(0, 100)]
    public uint simulatedLatency = 0;
    [Range(0, 20)]
    public uint simulatedJitter = 0;


    [HideInInspector]
    public bool startAsHost = false;

    [Header("Session Settings")]
    public PlayerRole localPlayerRole = PlayerRole.Player;

    [SerializeField]
    private NetworkObject playerPrefab;
    [SerializeField]
    private NetworkObject trainerPrefab;
    [SerializeField]
    private NetworkObject playerState;
    [SerializeField]
    private NetworkObject latencyControllerPrefab;

    private NetworkRunner _sessionRunner;
    private NetworkSceneManagerDefault _sceneManager;
    private NetworkPoseBridge _localPlayerBridge;
    private ServerLatencyController _latencyController;

#if UNITY_SERVER
    private readonly ConcurrentQueue<string> _consoleQueue = new();
    private int _hasPendingCommands_Interlocked = 0; // 0 = false, 1 = true; atomic
#endif

    private readonly List<PlayerRef> _activePlayers = new();
    private readonly Dictionary<PlayerRef, List<NetworkObject>> _playerObjects = new();
    private readonly Dictionary<PlayerRef, PlayerRole> _playerRoles = new();
    private readonly Queue<PlayerRole> _pendingRoles = new();

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
    }

    private async void Start()
    {
        bool _isHost = ResolveHost();
        Debug.Log($"Starting as: {(_isHost ? "SERVER" : "CLIENT")}");

        _sessionRunner = gameObject.AddComponent<NetworkRunner>();
        _sceneManager = _sessionRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        Debug.Log($"[Fusion] SceneManager: {_sceneManager}");

        _sessionRunner.ProvideInput = !_isHost;
        _sessionRunner.AddCallbacks(this);

        if (!_isHost)
        {
            LoadConfig();
        }

        Debug.LogWarning($"[Fusion] runner.ProvideInput: {_sessionRunner.ProvideInput}, runner.Mode={_sessionRunner.GameMode}");

        StartGameArgs args = new()
        {
            GameMode = _isHost ? GameMode.Server : GameMode.Client,
            Address = _isHost ? NetAddress.Any(port) : NetAddress.CreateFromIpPort(hostAddress, port),
            Scene = SceneRef.FromIndex(mainSceneIndex),
            SceneManager = _sceneManager,
            DisableNATPunchthrough = true,
            ConnectionToken = BitConverter.GetBytes((int)localPlayerRole) // convert into binary token
        };
        Debug.Log(args);

        StartGameResult result = await _sessionRunner.StartGame(args);
        if (result.Ok)
        {
        #if UNITY_SERVER
            StartConsoleHandler();
        #endif

            if (_isHost) 
            {
                Debug.Log($"[Fusion] Dedicated server started on {hostAddress}:{port}.");
            }
        }
        else
        {
            Debug.LogError("[Fusion] Connection failed: " + result.ShutdownReason);
        }
    }
#if UNITY_SERVER
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
#endif

    private void StartConsoleHandler()
    {
    #if UNITY_SERVER
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
    #endif
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
        if (!runner.IsServer)
        {
            return;
        }

        PlayerRole role = PlayerRole.Player; // fallback
        if (token != null && token.Length >= 4)
        {
            role = (PlayerRole)BitConverter.ToInt32(token, 0);
        }

        _pendingRoles.Enqueue(role);
        request.Accept();
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
        if (_localPlayerBridge == null || !_localPlayerBridge.IsReady || !_localPlayerBridge.HasInputAuthority) 
        {
            return;
        }

        PoseData _pose = _localPlayerBridge.GetPose();
        _localPlayerBridge.SetLocalPose(_pose);
        input.Set(
            new PoseInput {
                pose = _pose
            }
        );
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
        // Authority check
        if (!runner.IsServer) 
        {
            return;
        }

        if (!_activePlayers.Contains(player))
        {
            _activePlayers.Add(player);
        }

        if (!_playerObjects.ContainsKey(player))
        {
            _playerObjects[player] = new();
        }

        PlayerRole role = PlayerRole.Player;

        if (_pendingRoles.Count > 0)
        {
            role = _pendingRoles.Dequeue();
        }

        _playerRoles[player] = role;

        StartCoroutine(
            SpawnDeferred(player, _activePlayers.IndexOf(player))
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
            Debug.Log($"[SessionManager] Player {player} left.");
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
        if (_sessionRunner.IsServer && _latencyController == null)
        {
            _latencyController = _sessionRunner.Spawn(latencyControllerPrefab).GetComponent<ServerLatencyController>();
        }

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

    public void SetLocalBridge(NetworkPoseBridge bridge) 
    { 
        // Only allow setting NetworkBridge reference if LocalPlayer matches InputAuthority
        // (ie. on the same client)
        if (bridge.Object.InputAuthority == _sessionRunner.LocalPlayer)
        {
            _localPlayerBridge = bridge;
        }
    }

    private bool ResolveHost()
    {
#if DEDICATED_SERVER
        return true;
#elif HMD_CLIENT
        return false;
#else
    #if UNITY_EDITOR
        return startAsHost;
    #else
        return false;
    #endif
#endif
    }

    private IEnumerator SpawnDeferred(PlayerRef player, int playerIndex)
    {
        yield return new WaitUntil(() =>
            _sceneLoaded && 
            PlayerSpawnManager.Instance != null && 
            PlayerSpawnManager.Instance.IsReady
        );

        // Wait a while longer to avoid race conditions
        yield return new WaitForSeconds(0.1f);

        Transform spawn = PlayerSpawnManager.Instance.GetSpawnPointForPlayer(playerIndex % 2);
        if (spawn == null)
        {
            Debug.LogError("Failed to spawn player for " + player);
            yield break;
        }

        PlayerRole role = _playerRoles[player];
        NetworkObject prefab = role == PlayerRole.Trainer ? trainerPrefab : playerPrefab;

        NetworkObject playerAvatar = _sessionRunner.Spawn(
            prefab,
            spawn.position,
            spawn.rotation,
            player,
            (runner, obj) =>
            {
                obj.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            }
        );

        if (playerAvatar == null)
        {
            Debug.LogError("Failed to spawn player for " + player);
            yield break;
        }

        if (role == PlayerRole.Player && playerAvatar.TryGetComponent<NetworkPoseBridge>(out var poseBridge))
        {
            poseBridge.SetPlayerRef(player);
        }

        _playerObjects[player].Add(playerAvatar);

        Debug.LogWarning($"Spawned avatar for {player}; HasInputAuthority={playerAvatar.HasInputAuthority}, HasStateAuthority={playerAvatar.HasStateAuthority}");
    }

    private void LoadConfig()
    {
    #if UNITY_STANDALONE_WIN
        string path = Path.Combine(Application.dataPath, "../ConnectionConfig.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ConnectionConfig config = JsonUtility.FromJson<ConnectionConfig>(json);
            hostAddress = config.serverIP;
            port = (ushort)config.serverPort;
            localPlayerRole = config.joinAsTrainer ? PlayerRole.Trainer : PlayerRole.Player;
        }
        else
        {
            Debug.LogWarning("ConnectionConfig.json not found; using inspector values.");
        }
    #else
        // HMD builds use whatever is set in Inspector
        return;
    #endif
    }

    private void HandleCommand(string command)
    {
    #if UNITY_SERVER
        string[] parts = command.Split(' ');

        if (parts.Length >= 2 && parts[0] == "lat")
        {
            if (int.TryParse(parts[1], out int latency))
            {
                int jitter = 0;
                if (parts.Length >= 3)
                {
                    int.TryParse(parts[2], out jitter);
                }

                latency = Mathf.Clamp(latency, 0, 100);
                jitter = Mathf.Clamp(jitter, 0, 100);

                _latencyController.RPC_SetSimulatedLatency(latency, jitter);
                Debug.Log($"[SERVER] Latency {latency} ms | Jitter {jitter} ms");
            }
        }
    #endif
    }
}

[Serializable]
public class ConnectionConfig
{
    public string serverIP = "127.0.0.1";
    public int serverPort = 27015;
    public bool joinAsTrainer = false;
}
