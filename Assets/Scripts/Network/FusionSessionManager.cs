using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections;

public enum PlayerRole
{
    Player,
    Trainer
}

public class FusionSessionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Initialization")]

    [Tooltip("LAN host IP")]
    public string hostAddress = "192.168.1.12"; 

    [Tooltip("Needs to match build index.")]
    public int mainSceneIndex = 1;

    public ushort port = 27015;

    [Header("Editor Only")]
    public bool startAsHost = false;
    public PlayerRole editorPlayerRole = PlayerRole.Player;

    [Header("Session Settings")]

    [SerializeField]
    private NetworkObject playerPrefab;
    [SerializeField]
    private NetworkObject trainerPrefab;
    [SerializeField]
    private NetworkObject playerState;

    private NetworkRunner _sessionRunner;
    private NetworkSceneManagerDefault _sceneManager;
    private NetworkPoseBridge _localPlayerBridge;

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
            return;
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

        Debug.LogWarning($"[Fusion] runner.ProvideInput: {_sessionRunner.ProvideInput}, runner.Mode={_sessionRunner.GameMode}");

        PlayerRole localRole = ResolvePlayerRole();

        StartGameArgs args = new()
        {
            GameMode = _isHost ? GameMode.Server : GameMode.Client,
            Address = _isHost ? NetAddress.Any(port) : NetAddress.CreateFromIpPort(hostAddress, port),
            Scene = SceneRef.FromIndex(mainSceneIndex),
            SceneManager = _sceneManager,
            DisableNATPunchthrough = true,
            ConnectionToken = BitConverter.GetBytes((int)localRole) // convert into binary token
        };
        Debug.Log(args);

        StartGameResult result = await _sessionRunner.StartGame(args);
        if (result.Ok)
        {
            if (_isHost) 
            {
                Debug.Log($"[Fusion] Dedicated server started on {hostAddress}:{port}.");
            }
        }
        else
        {
            Debug.LogError("[Fusion] Connection failed: " + result.ShutdownReason);
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

    private PlayerRole ResolvePlayerRole()
    {
    #if UNITY_EDITOR
        return editorPlayerRole;
    #else
        return PlayerRole.Player; // VR build is always Player
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
}
