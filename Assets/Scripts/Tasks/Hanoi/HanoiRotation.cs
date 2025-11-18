using UnityEngine;

using Random = System.Random;

public class HanoiRotation : MonoBehaviour
{
    void Start()
    {
        Random rand = new();
        float rot = 180.0f * rand.Next(0, 2); //this gives either 0 or 1
        transform.rotation = Quaternion.Euler(0, rot, 0);
        Debug.LogWarning($"Rotation: {rot}");
    }
}
