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
    bool TryGetHandPose(PlayerRef player, EHandType hand, out Vector3 position, out Quaternion rotation);
}
