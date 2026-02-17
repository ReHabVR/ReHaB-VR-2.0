using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class ShapesManager : MonoBehaviour, IMinigameManager
{
    public List<GameObject> shapes;
    public List<Transform> spawnPoints;
    public List<Material> materials;
    public GameObject boardForShape;

    public UnityEvent correctShapePlaced;
    public UnityEvent anyShapePlaced;

    private List<int> indices = new() {0, 1, 2, 3, 4, 5};
    
    private const string TAG = "Shape";

    public event System.Action OnMove;
    public event System.Action OnCorrectMove;

    private void Start()
    {
        AssignProperties();
        ConnectNotifiers();
    }

    public void AssignProperties()
    {
        // Shuffle the list using Fisher–Yates shuffle
        ShuffleList(indices);

        // Assign a random position
        for (int i = 0; i < indices.Count; i++)
        {
            shapes[i].GetComponent<Rigidbody>().position = spawnPoints[indices[i]].transform.position;
        }
        
        // Assign a random material
        ShuffleList(indices);
        for (int i = 0; i < indices.Count; i++)
        {
            shapes[i].GetComponent<Renderer>().material = materials[indices[i]];
        }
    }

    public void ConnectNotifiers()
    {
        foreach (Transform t in boardForShape.transform)
        {
            GameObject child = t.gameObject;
            child.GetComponent<CollisionNotifier>().collisionDetected.AddListener(OnCollision);
        }
        foreach (GameObject shape in shapes)
        {
            shape.GetComponent<GrabNotifier>().OnRelease += OnObjectPlaced;
        }
    }

    public void OnObjectPlaced(IXRSelectInteractor _interactor)
    {
        OnMove?.Invoke();
    }

    public void OnCollision(GameObject go, int colliderId)
    {
        // Do not check objects other than shapes.
        if (!go.CompareTag(TAG)) 
            return;

        if (go.GetComponent<ShapeIdentifier>().ShapeType == colliderId)
        {
            OnCorrectMove?.Invoke();
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
