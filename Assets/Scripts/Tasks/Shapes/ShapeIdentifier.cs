using UnityEngine;

public class ShapeIdentifier : MonoBehaviour
{
    public enum Shape {
        Cube,
        Triangle,
        Cylinder
    }

    [SerializeField]
    private Shape shapeType = Shape.Cube;

    public int ShapeType 
    {
        get { return (int)shapeType; }
        private set { shapeType = (Shape)value; }
    }
}
