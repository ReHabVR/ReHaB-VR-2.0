using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class DiceManager : MonoBehaviour, IMinigameManager
{
    public GameObject dice;
    public DiceWhiteboard whiteboard;
    public GameObject diceHolder;

    public UnityEvent correctDiePlaced;
    public UnityEvent anyDiePlaced;

    private List<int> pipCount = new() {1, 2, 3, 4, 5, 6};
    
    private const string TAG = "Die";

    private void Start()
    {
        AssignProperties();
        ConnectNotifiers();
    }

    public void AssignProperties() 
    {
        ShuffleList(pipCount);
        int i = 0;

        // Setup dice
        foreach (Transform t in dice.transform)
        {
            GameObject child = t.gameObject;
            if (child.GetComponent<SixFaceDie>() == null)
            {
                Debug.LogError(child.name + " is not a SixFaceDie.");
                continue;
            }

            SixFaceDie die = child.GetComponent<SixFaceDie>();
            die.Setup(pipCount[i++]);
            die.GetComponent<GrabNotifier>().OnRelease += OnObjectPlaced;
        }

        // Prepare whiteboard
        ShuffleList(pipCount);
        for (int n = 0; n < pipCount.Count; n++)
        {
            whiteboard.AssignMaterial(n, pipCount[n] - 1);
        }
    }

    public void ConnectNotifiers() 
    {
        foreach (Transform t in diceHolder.transform)
        {
            GameObject child = t.gameObject;
            child.GetComponent<CollisionNotifier>().collisionDetected.AddListener(OnCollision);
        }
    }


    public void OnObjectPlaced(IXRSelectInteractor _interactor)
    {
        anyDiePlaced?.Invoke();
    }

    public void OnCollision(GameObject go, int colliderId)
    {
        // Do not check objects other than dice.
        if (!go.CompareTag(TAG)) 
            return;

        // Detectors and Plaques have matching ID
        GameObject plaque = whiteboard.transform.GetChild(colliderId).gameObject;
        SixFaceDie plaqueDie = plaque.GetComponent<SixFaceDie>();
        SixFaceDie die = go.GetComponent<SixFaceDie>();
        
        if (die.GetPipCount() == plaqueDie.GetPipCount()) 
        {
            correctDiePlaced?.Invoke();
        }
    }

    static void ShuffleList<T>(List<T> list)
    {
        // Fisher–Yates shuffle
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
