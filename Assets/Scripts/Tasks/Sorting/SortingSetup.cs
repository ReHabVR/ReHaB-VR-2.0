using Fusion;
using UnityEngine;
using Random = System.Random;

public class SortingSetup : NetworkBehaviour
{
    public int blueCount = 5;
    public int redCount = 5;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        Random _rand = new();
        foreach (Transform t in transform)
        {
            GameObject child = t.gameObject;
            if (_rand.Next(2) == 1 && redCount > 0) 
            {
                child.GetComponent<BallColor>().ColorID = (int)BallColor.Color.Red;
                redCount--;
                continue;
            }
            else if (blueCount > 0) 
            {
                child.GetComponent<BallColor>().ColorID = (int)BallColor.Color.Blue;
                blueCount--;
                continue;
            }
            // Fallback in case we ran out of blue balls but didn't get enoguh random red balls
            child.GetComponent<BallColor>().ColorID = (int)BallColor.Color.Red;;
        }
    }
}
