using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
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

    [Header("")]

    [SerializeField]
    private NetworkRunner runner;

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
        if (runner == null)
        {
            runner = gameObject.GetComponent<NetworkRunner>();    
            if (runner == null)
            {
                runner = gameObject.AddComponent<NetworkRunner>();
            }
        }
        
        runner.ProvideInput = true;
        runner.AddCallbacks(this);

        StartGameArgs args = new();

        if (isHost)
        {
            args.GameMode = GameMode.Host;
            args.Address = NetAddress.Any(port);
            args.SessionName = "ReHaB_Room";
            args.Scene = SceneRef.FromIndex(mainSceneIndex);
        }
        else
        {
            args.GameMode = GameMode.Client;
            args.Address = NetAddress.CreateFromIpPort(hostAddress, port);
            args.SessionName = "ReHaB_Room";
        }
        
        args.SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        
        StartGameResult result = await runner.StartGame(args);
        if (!result.Ok)
        {
            Debug.LogError("Connection failed: " + result.ShutdownReason);
            // TODO: show a retry popup
        }
    }

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
        if (!runner.IsServer) // Authority check.
        {
            return;
        }

        if (player == runner.LocalPlayer)
        {
            // Don't spawn a player for the host/server.
            return;
        }

        Transform spawn = FindObjectOfType<PlayerSpawnManager>().GetSpawnPointForPlayer(player);
        if (spawn)
        {
            runner.Spawn(
                playerPrefab,
                spawn.position,
                spawn.rotation,
                player
            );
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        //throw new NotImplementedException();
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
        //throw new NotImplementedException();
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
}
