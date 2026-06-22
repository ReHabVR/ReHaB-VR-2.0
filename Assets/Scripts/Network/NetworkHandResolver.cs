using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Fusion;
using UnityEngine;

public class NetworkHandResolver : MonoBehaviour, IHandPoseResolver
{
    private readonly Dictionary<PlayerRef, NetworkPoseBridge> _bridges = new();

    public static IHandPoseResolver Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            HandPoseResolver.Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public void RegisterBridge(PlayerRef player, NetworkPoseBridge bridge)
    {
        _bridges[player] = bridge;
    }

    public void UnregisterBridge(PlayerRef player)
    {
        _bridges.Remove(player);
    }

    public bool TryGetHandPose(PlayerRef player, EHandType hand, out Vector3 position, out Quaternion rotation)
    {
        position = default;
        rotation = default;

        if (!_bridges.TryGetValue(player, out NetworkPoseBridge bridge))
        {
            return false;
        }

        if (!bridge.TryGetHandSocket(hand, out Transform socket))
        {
            return false;
        }

        position = socket.position;
        rotation = socket.rotation;
        return true;
    }

    public bool TryGetLocalHandPose(PlayerRef player, EHandType hand, out Vector3 position, out Quaternion rotation)
    {
        position = default;
        rotation = default;

        if (!_bridges.TryGetValue(player, out NetworkPoseBridge bridge))
        {
            return false;
        }

        PoseData pose = bridge.GetLocalPose();

        switch (hand)
        {
            case EHandType.Left:
                position = pose.lhandPos;
                rotation = pose.lhandRot;
                return true;

            case EHandType.Right:
                position = pose.rhandPos;
                rotation = pose.rhandRot;
                return true;
        }

        return false;
    }
}
