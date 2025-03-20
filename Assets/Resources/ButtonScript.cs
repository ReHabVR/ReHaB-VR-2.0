using UnityEngine;
using UnityEngine.Events;

public class ButtonScript : MonoBehaviour
    {
        public GameObject button;
        public UnityEvent onPress;
        public UnityEvent onRelease;
        private GameObject _presser;
        private bool _isPressed;

        public void Start()
        {
            _isPressed = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isPressed)
            {
                button.transform.localPosition = new Vector3(0, 0.003f, 0);
                _presser = other.gameObject;
                onPress.Invoke();
                _isPressed = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == _presser)
            {
                button.transform.localPosition = new Vector3(0, 0.019f, 0);
                onRelease.Invoke();
                _isPressed = false;
            }
        }
    }