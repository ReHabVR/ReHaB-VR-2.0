using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class StandaloneTaskButton : MonoBehaviour
    {
        public GameObject button;
        public UnityEvent onPress;
        public UnityEvent onRelease;
        public Transform spawnPositon;
        public List<GameObject> objectsToSpawn = new();
        public GameObject[] taskButtons;

        private GameObject _presser;
        private bool _isPressed = false;
        private bool _isTaskActive = false;
        private DateTime lastButtonClickedTimestamp = DateTime.Now;
        private List<GameObject> spawnedObjectInstance = new();
    
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
                onPress.Invoke();
                _isPressed = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == _presser)
            {
                DateTime now = DateTime.Now;
                button.transform.localPosition = new Vector3(0.0f, 0.019f, 0.0f);
                onRelease.Invoke();
                _isPressed = false;

                if (now.Subtract(lastButtonClickedTimestamp).TotalSeconds < 1.0f)
                    return;
                
                _isTaskActive = !_isTaskActive;
                SpawnObjects(_isTaskActive);
                lastButtonClickedTimestamp = DateTime.Now;
            }
        }
        private void SpawnObjects(bool isAlreadyActive)
        {
            DeleteAllTasks();
            if (isAlreadyActive) 
            {
                // Show time elapsed between start and stop
                DateTime now = DateTime.Now;
                Debug.Log("Time elasped: " + now.Subtract(lastButtonClickedTimestamp).TotalSeconds);
                return;
            }

            SpawnTask();
         }

        public void SpawnTask()
        {
            if (objectsToSpawn.Count == 0)
            {
                Debug.LogWarning("No objects to spawn were defined.");
                return;
            }

            foreach (GameObject button in taskButtons) 
            {
                // Hide other buttons
                if (button != this.gameObject)
                    SetButtonActive(button, false);
            }

            foreach (GameObject obj in objectsToSpawn) 
            {
                spawnedObjectInstance.Add(SpawnObject(obj));
            }
        }
    
        
        private GameObject SpawnObject(GameObject obj)
        {
            GameObject spawnedObject = Instantiate(
                obj, 
                spawnPositon.position,
                obj.transform.rotation
            );

            return spawnedObject;
        }

        public void DeleteAllTasks()
        {
            foreach (GameObject button in taskButtons) 
            {
                if (button != this.gameObject)
                    SetButtonActive(button, true);
            }

            foreach (GameObject go in spawnedObjectInstance)
            {
                if (go != null) 
                    Destroy(go);     
            }
        }

        public void SetButtonVisibility(bool state)
        {
            button = GameObject.Find("Button");

            if (button == null)
                return;

            button.SetActive(state);
        }

        private void SetButtonActive(GameObject button, bool active) 
        {
            button.SetActive(active);
        }
    }
