using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using System;

public class ButtonTrainingSwapper : MonoBehaviour
    {
        public GameObject button;
        public UnityEvent onPress;
        public UnityEvent onRelease;
        private GameObject _presser;
        private bool _isPressed;
        private int _indexOfObjects = -1;
        private PhotonView photonView;
        private DateTime lastButtonClickedTimestamp;
    
        public void Start()
        {
            _isPressed = false;
            SpawnObjects(_indexOfObjects);
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
                bool isDelayedEnough = lastButtonClickedTimestamp == null || now.Subtract(lastButtonClickedTimestamp).TotalSeconds >= 1;
                button.transform.localPosition = new Vector3(0, 0.019f, 0);
                onRelease.Invoke();
                _isPressed = false;
                if (isDelayedEnough)
                {
                    _indexOfObjects++;
                    var temp = _indexOfObjects % 5;
                    _indexOfObjects = temp;
                    SpawnObjects(_indexOfObjects);
                    lastButtonClickedTimestamp = DateTime.Now;
                }
            }
        }
        private void SpawnObjects(int index)
        {
            DeleteAllObjects();

            switch (index)
            {
                case 0:
                    //Czysty stol
                    break;
                case 1:
                    SpawnFirstTraining();
                    break;
                case 2:
                    SpawnSecondTraining();
                    break;
                case 3:
                    SpawnThirdTraining();
                    break;
                case 4:
                    SpawnFourthTraining();
                    break;
            }
        }

        public void SpawnFirstTraining()
        {
            SpawnPhotonObject("EmptyBox");
            SpawnPhotonObject("BoxWithBalls");
        }
    
        private void SpawnSecondTraining()
        {
            SpawnPhotonObject("Shapes");
            SpawnPhotonObject("ShapesBox");
        }
    
        private void SpawnThirdTraining()
        {
            SpawnPhotonObject("Buttons");
        }
        private void SpawnFourthTraining()
        {
            SpawnPhotonObject("RingsHolder");
            SpawnPhotonObject("Rings");
        }
        private GameObject SpawnPhotonObject(string objectName)
        {
            GameObject spawnedObject = Instantiate((GameObject)UnityEngine.Resources.Load(objectName));
            GameObject spawnedPhotonObject = PhotonNetwork.Instantiate(objectName, spawnedObject.transform.position, spawnedObject.transform.rotation);
            Destroy(spawnedObject);

            return spawnedPhotonObject;
        }

        public void DeleteAllObjects()
        {
            string[] objectNamesToDestroy = new string[] {
                "RingsHolder(Clone)",
                "Rings(Clone)",
                "EmptyBox(Clone)", 
                "BoxWithBalls(Clone)",
                "Shapes(Clone)",
                "ShapesBox(Clone)",
                "Buttons(Clone)"
            };

            foreach (string objectName in objectNamesToDestroy)
            {
                GameObject gameObject = GameObject.Find(objectName);
                if (gameObject != null) 
                {
                    PhotonNetwork.Destroy(gameObject);     
                } 
            }
        }

        public void SetButtonVisibility(bool state)
        {
            button = GameObject.Find("Button");

            if (button != null)
            {
                button.SetActive(state); 
            }
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
    }
