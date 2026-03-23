using Fusion;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BallColor : NetworkBehaviour
{
    public enum Color 
    { 
        Blue = 0, 
        Red = 1 
    }

    [Networked]
    public int ColorID { get; set; }

    public Material blueMaterial;
    public Material redMaterial;

    private Renderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public override void Spawned()
    {
        if (ColorID == (int)Color.Blue)
        {
            _renderer.material = blueMaterial;
        }
        else
        {
            _renderer.material = redMaterial;
        }
    }
}
