using UnityEngine;

public static class PoseMathHelpers
{
    public static bool PoseEqualsApprox(in PoseData a, in PoseData b, float posEps = 0.0001f, float rotEps = 0.001f) =>
            Vector3.SqrMagnitude(a.lhandPos - b.lhandPos) < posEps * posEps && 
            Quaternion.Dot(a.lhandRot, b.lhandRot) > 1f - rotEps && 
            Vector3.SqrMagnitude(a.rhandPos - b.rhandPos) < posEps * posEps && 
            Quaternion.Dot(a.rhandRot, b.rhandRot) > 1f - rotEps &&
            Vector3.SqrMagnitude(a.headPos - b.headPos) < posEps * posEps && 
            Quaternion.Dot(a.headRot, b.headRot) > 1f - rotEps;

    public static Vector3 QuaternionToRotationVector(Quaternion quat)
    {
        quat.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180.0f)
        {
            angle -= 360.0f;
        }

        return angle * Mathf.Deg2Rad * axis;
    }

    public static Quaternion RotationVectorToQuaternion(Vector3 vec)
    {
        float angle = vec.magnitude;
        if (angle < 0.0001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, vec.normalized);
    }

    public static Vector3 CalculateVelocity(Vector3 currentPos, Vector3 previousPos, 
            float currentTimestamp, float previousTimestamp)
    {
        float dt = currentTimestamp - previousTimestamp;
        return dt > 0f ? (currentPos - previousPos) / dt : Vector3.zero;
    }
}
