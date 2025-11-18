using UnityEngine;

public class BallColor : MonoBehaviour
{
    public enum Color {
        Blue = 0,
        Red = 1
    }
    
    private Color colorId;

    public int ColorID
    {
        get { return (int)colorId; }
        set { colorId = (Color)value; } 
    }
}
