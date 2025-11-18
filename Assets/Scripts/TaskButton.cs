using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class TaskButton : MonoBehaviour
{
    public GameObject button;
    public UnityEvent onPress;
    public UnityEvent onRelease;
    public Transform spawnPositon;
    public GameObject objectToSpawn;
    public int taskId;

    private GameObject _presser;
    private bool _isPressed = false;
    private DateTime lastButtonClickedTimestamp = DateTime.Now;

    public void Start()
    {
        _isPressed = false;
        lastButtonClickedTimestamp = DateTime.Now;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isPressed)
        {
            _presser = other.gameObject;
            button.transform.localPosition = new Vector3(0.0f, 0.003f, 0.0f);
            onPress?.Invoke();
            _isPressed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _presser)
        {
            DateTime now = DateTime.Now;
            button.transform.localPosition = new Vector3(0.0f, 0.019f, 0.0f);
            onRelease?.Invoke();
            _isPressed = false;

            if (now.Subtract(lastButtonClickedTimestamp).TotalSeconds < 1.0f)
                return;

            lastButtonClickedTimestamp = DateTime.Now;
        }
    }
}
