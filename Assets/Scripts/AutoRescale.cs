using UnityEngine;

public class AutoRescale : MonoBehaviour
{
    public Texture texture;

    private void Start()
    {
        if (texture == null)
        {
            Debug.LogWarning("Texture not set, using default scale.");
            return;
        }

        SetQuadSize(texture.width, texture.height);
    }

    private void SetQuadSize(int width, int height)
    {
        float aspectRatio = (float)width / height;
        transform.localScale = new Vector3(aspectRatio, 1.0f, 1.0f);
        Debug.Log($"RenderTarget aspect ratio: {aspectRatio}");
    }
}
