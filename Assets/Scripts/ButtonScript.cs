using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ButtonScript : MonoBehaviour
{
    public float pressCooldown = 0.8f;
    public GameObject button;
    public UnityEvent onPress;
    public UnityEvent onRelease;

    private GameObject _presser;
    private bool _isPressed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_isPressed)
            return;
        
        button.transform.localPosition = new Vector3(0, 0.003f, 0);
        _presser = other.gameObject;
        _isPressed = true;
        onPress?.Invoke();
        StartCoroutine(PressCooldown());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _presser)
        {
            button.transform.localPosition = new Vector3(0, 0.019f, 0);
            onRelease?.Invoke();
            _presser = null;
        }
    }

    private IEnumerator PressCooldown()
    {
        yield return new WaitForSeconds(pressCooldown);
        _isPressed = false;
    }
}