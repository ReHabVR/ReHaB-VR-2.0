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

public enum EPlayerRole
{
    Player,
    Trainer,
    Spectator
}

public enum ECompensationMode
{
    // Invalid compensation mode
    Invalid = -1,
    // Raw network pose without any prediction applied
    None = 0,
    // Base smoothing; technically not a prediction method
    Interpolation = 1,
    // Simple prediction based on extrapolation
    Extrapolation = 2,
    // Model-based motion prediction
    DeadReckoning = 3,
    // Filter-based prediction
    KalmanFilter = 4
}

public class FusionSessionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public const int MAIN_SCENE_INDEX = 1;

    [Header("Initialization")]
    public string hostAddress = "192.168.1.12"; 

    public ushort port = 27015;

    [HideInInspector]
    public bool startAsHost = false;


    //DEPRECATED - now set directly in config SO
    //[Header("Simulated Network Latency")]
    //[Range(0, 1000), Tooltip("End-to-end latency. For RTT, multiply by 2.")]
    //public uint endToEndDelay = 100;

    [Header("Player Settings")]
    public EPlayerRole localPlayerRole = EPlayerRole.Player;
    [Tooltip("Editor only. Joining as spectator does not spawn a player avatar.")]
    public bool joinAsSpectator = true;

    [Header("Session Settings")]
    public ECompensationMode compensationMode = ECompensationMode.None;

    [Header("Object References")]
    [SerializeField]
    private NetworkObject playerPrefab;
    [SerializeField]
    private NetworkObject trainerPrefab;
    [SerializeField]
    private NetworkObject sessionExperimentControllerPrefab;
    [SerializeField]
    private ServerConsoleHandler _consoleHandler;

    private NetworkRunner _sessionRunner;
    private NetworkSceneManagerDefault _sceneManager;
    private NetworkPoseBridge _localPlayerBridge;

    [HideInInspector]
    public SessionExperimentController experimentController;
    

    private readonly List<PlayerRef> _activePlayers = new();
    private readonly Dictionary<PlayerRef, List<NetworkObject>> _playerObjects = new();
    private readonly Dictionary<PlayerRef, EPlayerRole> _playerRoles = new();
    private readonly Queue<EPlayerRole> _pendingRoles = new();

    private bool _sceneLoaded = false;
    
    public static FusionSessionManager Instance { get; private set; }

    public ECompensationMode CurrentCompensationMode
    {
        get 
        {
            if (experimentController == null || experimentController.Object == null || !experimentController.Object.IsValid)
            {
                return ECompensationMode.None;
            }

            return experimentController.CurrentCompensationMode;
        }
    }

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
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 90;
    #endif
    }

    private async void Start()
    {
        bool _isHost = ResolveHost();
        Debug.Log($"Starting as: {(_isHost ? "SERVER" : "CLIENT")}");
        
        GameObject runnerObject = new("FusionRunner");
        _sessionRunner = runnerObject.AddComponent<NetworkRunner>();
        _sceneManager = runnerObject.AddComponent<NetworkSceneManagerDefault>();
        DontDestroyOnLoad(runnerObject);
        Debug.Log($"[Fusion] SceneManager: {_sceneManager}");

        _sessionRunner.ProvideInput = !_isHost;
        _sessionRunner.AddCallbacks(this);

        if (!_isHost)
        {
            LoadConfig();
        }
        Debug.Log($"[Fusion] runner.ProvideInput: {_sessionRunner.ProvideInput}, runner.Mode={_sessionRunner.GameMode}");
        
        //ApplyNetworkConditions();
        StartGameArgs args = new()
        {
            GameMode = _isHost ? GameMode.Server : GameMode.Client,
            Address = _isHost ? NetAddress.Any(port) : NetAddress.CreateFromIpPort(hostAddress, port),
            Scene = SceneRef.FromIndex(MAIN_SCENE_INDEX),
            SceneManager = _sceneManager,
            DisableNATPunchthrough = true,
            ConnectionToken = BitConverter.GetBytes((int)localPlayerRole) // convert into binary token
        };
        Debug.Log(args);

        StartGameResult result = await _sessionRunner.StartGame(args);
        if (result.Ok)
        {
        #if UNITY_SERVER
            _consoleHandler.StartConsoleHandler();
        #endif
            if (_isHost) 
            {
                Debug.Log($"[Fusion] Dedicated server started on {hostAddress}:{port}.");
            }
            else 
            {
                Debug.Log($"[Fusion] Connected to server ({hostAddress}:{port}).");
            }
        }
        else
        {
            Debug.LogError("[Fusion] Connection failed: " + result.ShutdownReason);
        }
    }

    public void SetLocalBridge(NetworkPoseBridge bridge) 
    { 
        // Only allow setting NetworkBridge reference if LocalPlayer matches InputAuthority
        // (ie. on the same client)
        if (bridge.Object.InputAuthority == _sessionRunner.LocalPlayer)
        {
            _localPlayerBridge = bridge;
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
        if (!runner.IsServer)
        {
            return;
        }

        EPlayerRole role = EPlayerRole.Player; // fallback
        if (token != null && token.Length >= 4)
        {
            role = (EPlayerRole)BitConverter.ToInt32(token, 0);
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
        if (_localPlayerBridge == null || !_localPlayerBridge.IsReady) 
        {
            return;
        }

        PoseData newPose = _localPlayerBridge.GetLocalPose();
        input.Set(
            new PoseInput {
                pose = newPose
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

        EPlayerRole role = EPlayerRole.Player;
        if (_pendingRoles.Count > 0)
        {
            role = _pendingRoles.Dequeue();
        }

        _playerRoles[player] = role;

        #if UNITY_EDITOR
        if (joinAsSpectator)
        {
            Debug.Log($"[SERVER] Player {player} joined as spectator.");
            return;
        }
        #endif

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

            _activePlayers.Remove(player);
            _playerRoles.Remove(player);

            Debug.Log($"[SERVER] Player {player} left.");
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
        if (_sessionRunner.IsServer && experimentController == null)
        {
            NetworkObject obj = _sessionRunner.Spawn(sessionExperimentControllerPrefab);
            experimentController = obj.GetComponent<SessionExperimentController>();
        }

        _sceneLoaded = true;
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        //throw new NotImplementedException();
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

    private bool ResolveHost()
    {
#if DEDICATED_SERVER
        return true; // Server is always the host
#elif HMD_CLIENT
        return false; // HMD clients can never be the host
#elif UNITY_EDITOR
        return startAsHost; // Determine whether editor acts as server or a client
#else
        return false;
#endif
    }

    private IEnumerator SpawnDeferred(PlayerRef player, int playerIndex)
    {
        yield return new WaitUntil(() =>
            _sceneLoaded && 
            PlayerSpawnManager.Instance != null && 
            PlayerSpawnManager.Instance.IsReady
        );

        // Wait a little longer to avoid race conditions
        yield return new WaitForSeconds(0.1f);

        Transform spawn = PlayerSpawnManager.Instance.GetSpawnPointForPlayer(playerIndex % 2);
        if (spawn == null)
        {
            Debug.LogError("Failed to spawn player for " + player);
            yield break;
        }

        EPlayerRole role = _playerRoles[player];
        NetworkObject prefab = role == EPlayerRole.Trainer ? trainerPrefab : playerPrefab;

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
            localPlayerRole = config.joinAsTrainer ? EPlayerRole.Trainer : EPlayerRole.Player;
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
}

[Serializable]
public class ConnectionConfig
{
    public string serverIP = "127.0.0.1";
    public int serverPort = 27015;
    public int endToEndDelay = 100; 
    public bool joinAsTrainer = false;
}
