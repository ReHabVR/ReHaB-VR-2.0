using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbabilityDisplayerController : MonoBehaviour
{

    public GameObject probabilityDisplayer;
    private Renderer _probabilityDisplayerRenderer;
    private float _probability = 0f;
    private Color _currentColor = new Color(1f, 0f, 0f);
    private float _currentProbability = 0f;
    private float _colorChangeDurationInSeconds = 1f;

    void Start()
    {
        _probabilityDisplayerRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (_probability != _currentProbability)
        {
            UpdateColor();
        }
    }

    public void ChangeColor(float probability)
    {
        _probability = Mathf.Clamp(probability, 0f, 1f);
    }

    private Color GetColor(float probability)
    {
        Color red = new Color(1f, 0f, 0f);
        Color green = new Color(0f, 1f, 0f);
        return Color.Lerp(red, green, probability);
    }

    private void UpdateColor()
    {
        Color newColor = GetColor(_probability);
        _currentColor = GetColor(_currentProbability);
        _currentProbability = _probability;
        
        StartCoroutine(AnimateColorChange(_currentColor, newColor, _colorChangeDurationInSeconds));
    }

    // Coroutine to smoothly transition between two colors over a specified duration
    private IEnumerator AnimateColorChange(Color startColor, Color endColor, float duration)
    {
        float time = 0;

        while (time < duration)
        {
            // Update the elapsed time
            time += Time.deltaTime;

            // Calculate the interpolation factor
            float t = time / duration;
            float easedT = EaseInOutQuad(t);

            // Linearly interpolate between the start and end colors based on the interpolation factor
            _probabilityDisplayerRenderer.material.color = Color.Lerp(startColor, endColor, easedT);

            // Yield until the next frame
            yield return null;
        }

        // Ensure the final color is set
        _probabilityDisplayerRenderer.material.color = endColor;
    }

    private float EaseInOutQuad(float t)
    {
        if (t < 0.5)
            return 2 * t * t;
        else
            return -1 + (4 - 2 * t) * t;
    }
}
