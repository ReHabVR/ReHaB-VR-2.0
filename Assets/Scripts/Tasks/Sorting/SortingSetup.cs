using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Random = System.Random;

public class SortingSetup : NetworkBehaviour
{
    public event Action OnBallsSpawned;
    
    [SerializeField]
    private NetworkObject ballPrefab;

    [SerializeField]
    private int blueCount = 5;
    [SerializeField]
    private int redCount = 5;

    private readonly List<NetworkObject> spawnedBalls = new();

    public List<NetworkObject> SpawnedBalls => spawnedBalls;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        Random _rand = new();
        foreach (Transform spawn in transform)
        {
            NetworkObject ball = Runner.Spawn(
                ballPrefab,
                spawn.position,
                spawn.rotation
            );

            spawnedBalls.Add(ball);
            BallColor color = ball.GetComponent<BallColor>();

            if (_rand.Next(2) == 1 && redCount > 0)
            {
                color.ColorID = (int)BallColor.Color.Red;
                redCount--;
            }
            else if (blueCount > 0)
            {
                color.ColorID = (int)BallColor.Color.Blue;
                blueCount--;
            }
            else
            {
                // Fallback in case we ran out of blue balls but didn't get enoguh random red balls
                color.ColorID = (int)BallColor.Color.Red;
            }
        }

        OnBallsSpawned?.Invoke();
    }
}
