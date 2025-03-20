using System;
using UnityEngine;
using UnityEngine.Events;

public class TrainingSwapper : MonoBehaviour
    {
        [SerializeField] private float threshold = 0.1f;
        [SerializeField] private float deadZone = 0.025f;

        private bool _isPressed;
        private Vector3 _startPos;
        private ConfigurableJoint _joint;
        private int _indexOfObjects = -1;
    
    
        public UnityEvent onPressed, onReleased;
        void Start()
        {
            _startPos = transform.localPosition;
            _joint = GetComponent<ConfigurableJoint>();
            SpawnObjects(_indexOfObjects);
        }
    
        void Update()
        {
            if (!_isPressed && GetValue() + threshold >= 1)
                Pressed(); 
            if (_isPressed && GetValue() - threshold <= 0)
                Released();
        }

        private float GetValue()
        {
            var value = Vector3.Distance(_startPos, transform.localPosition) / _joint.linearLimit.limit;

            if (Math.Abs(value) < deadZone)
                value = 0;
        
            return Mathf.Clamp(value, -1f, 1f);
        }
    
        private void Pressed()
        {
            _isPressed = true;
            onPressed.Invoke();
            Debug.Log("Pressed");
        }

        private void Released()
        {
            _isPressed = false;
            _indexOfObjects++;
            var temp = _indexOfObjects % 3;
            _indexOfObjects = temp;
            Debug.Log(_indexOfObjects);
            onReleased.Invoke();
            SpawnObjects(_indexOfObjects);


        }

        private void SpawnObjects(int index)
        {
            switch (index)
            {
                case 0:
                    SpawnFirstTraining();
                    break;
                case 1:
                    SpawnSecondTraining();
                    break;
                case 2:
                    SpawnThirdTraining();
                    break;
            }
        }

 
    
        private void SpawnFirstTraining()
        {
            Destroy(GameObject.Find("Buttons(Clone)"));
            Instantiate((GameObject)UnityEngine.Resources.Load("EmptyBox"));
            Instantiate((GameObject)UnityEngine.Resources.Load("BoxWithBalls"));
            Debug.Log("Spawn first object");
        }
    
        private void SpawnSecondTraining()
        {
            Destroy(GameObject.Find("EmptyBox(Clone)"));
            Destroy(GameObject.Find("BoxWithBalls(Clone)"));
            Instantiate((GameObject)UnityEngine.Resources.Load("Lock"));
            Instantiate((GameObject)UnityEngine.Resources.Load("Key"));
            Debug.Log("Spawn second object");
        }
    
        private void SpawnThirdTraining()
        {
            Destroy(GameObject.Find("Lock(Clone)"));
            Destroy(GameObject.Find("Key(Clone)"));
            Instantiate((GameObject)UnityEngine.Resources.Load("Buttons"));
        }
    }
