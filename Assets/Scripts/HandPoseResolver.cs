using Fusion;
using UnityEngine;

public enum EHandType
{
    Left,
    Right
}

public static class HandPoseResolver
{
    public static IHandPoseResolver Instance;
}

public interface IHandPoseResolver
{
    /// <summary>
    /// Returns AUTHORITATIVE (server-side) hand pose.
    /// </summary>
    bool TryGetHandPose(PlayerRef player, EHandType hand, out Vector3 position, out Quaternion rotation);

    /// <summary>
    /// Returns LOCAL (client-side) hand pose.
    /// </summary>
    bool TryGetLocalHandPose(PlayerRef player, EHandType hand, out Vector3 position, out Quaternion rotation);
}
