using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using System;
using System.Collections.Generic;

public class TaskButton : MonoBehaviour
    {
        public GameObject button;
        public UnityEvent onPress;
        public UnityEvent onRelease;
        public Transform spawnPositon;
        public string[] objectsToSpawn;
        public GameObject[] taskButtons;
        private GameObject _presser;
        private bool _isPressed;
        private bool _isTaskActive = false;
        private PhotonView photonView;
        private DateTime lastButtonClickedTimestamp;

        private List<GameObject> spawnedObjectInstance = new List<GameObject>();
    
        public void Start()
        {
            _isPressed = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isPressed)
            {
                _presser = other.gameObject;
                photonView = _presser.GetComponent<PhotonView>();
                if (PhotonNetwork.IsMasterClient == true)
                {
                    button.transform.localPosition = new Vector3(0, 0.003f, 0);
                    onPress.Invoke();
                    _isPressed = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == _presser && PhotonNetwork.IsMasterClient == true)
            {
                DateTime now = DateTime.Now;
                bool isDelayedEnough = lastButtonClickedTimestamp == null 
                        || now.Subtract(lastButtonClickedTimestamp).TotalSeconds >= 1;
                button.transform.localPosition = new Vector3(0, 0.019f, 0);
                onRelease.Invoke();
                _isPressed = false;
                if (isDelayedEnough)
                {
                    _isTaskActive =! _isTaskActive;
                    SpawnObjects(_isTaskActive);
                    lastButtonClickedTimestamp = DateTime.Now;
                }
            }
        }
        private void SpawnObjects(bool alreadyActive)
        {
            DeleteAllTasks();
            if (alreadyActive) 
            {
                DateTime now = DateTime.Now;
                Debug.Log("Time elasped: " + now.Subtract(lastButtonClickedTimestamp).TotalSeconds);
                return;
            }

            SpawnTask();
         }

        public void SpawnTask()
        {
            if (objectsToSpawn.Length == 0) 
            {
                Debug.LogWarning("No objects to spawn were defined.");
                return;
            }

            foreach (GameObject button in taskButtons) {
                Debug.Log(button);
                if (button != this.gameObject)
                    SetButtonActive(button, false);
            }

            foreach (string obj in objectsToSpawn) 
            {
                spawnedObjectInstance.Add(SpawnPhotonObject(obj));
            }
        }
    
        
        private GameObject SpawnPhotonObject(string objectName)
        {

            GameObject prefab = (GameObject)UnityEngine.Resources.Load(objectName);

            GameObject spawnedObject = Instantiate(
                prefab, 
                spawnPositon.position,
                prefab.transform.rotation
            );

            GameObject spawnedPhotonObject = PhotonNetwork.Instantiate( 
                objectName, 
                spawnedObject.transform.position, 
                spawnedObject.transform.rotation
            );

            Destroy(spawnedObject);

            return spawnedPhotonObject;
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
                if (go == null) 
                    continue;

                PhotonNetwork.Destroy(go);     
            }
        }

        public void SetButtonVisibility(bool state)
        {
            button = GameObject.Find("Button");

            if (button == null)
                return;

            button.SetActive(state);
        }

        public void PrepareObjectSpawnButton()
        {
            button = GameObject.Find("Button");
            GameObject buttonClone = GameObject.Find("Button(Clone)");

            if (button == null && buttonClone == null)
            {
                SpawnPhotonObject("Button");
            }
        }

        [PunRPC]
        private void SetButtonActive(GameObject button, bool active) 
        {
            button.SetActive(active);
        }
    }
