using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class ButtonChangeGameState : MonoBehaviour 
{
    public GameObject button;
    public GameState gameState;
    public Material first;
    public Material second;
    private GameObject _presser;
    private bool _isPressed;
    private int _counter = 1;
    private PhotonView photonView;
    private DateTime lastButtonClickedTimestamp;
    
    public void Start()
    { 
        button.GetComponent<MeshRenderer>().material = first;
        _isPressed = false;
        gameState.Value.buttonPressed = false; 
    }
    
    private void OnTriggerEnter(Collider other)
    { 
        if (!_isPressed) {
            _presser = other.gameObject;
            photonView = _presser.GetComponent<PhotonView>();
            if (PhotonNetwork.IsMasterClient == true) { 
                button.transform.localPosition = new Vector3(0, 0.003f, 0);
                _isPressed = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _presser) {
            DateTime now = DateTime.Now;
            bool isDelayedEnough = lastButtonClickedTimestamp == null || now.Subtract(lastButtonClickedTimestamp).TotalSeconds >= 1; 
            button.transform.localPosition = new Vector3(0, 0.019f, 0); 
            _isPressed = false;
            _counter++;
            SwitchColorAndState();
            if (isDelayedEnough)
            {
                lastButtonClickedTimestamp = DateTime.Now;
            }
        }
    }

    public void Increment()
    {
        _counter++;
    }
    
    public void SwitchColorAndState()
    {
        if (_counter % 2 == 0)
        {
            button.GetComponent<MeshRenderer>().material = second;
            gameState.Value.buttonPressed = true;
        }
        else
        {
            button.GetComponent<MeshRenderer>().material = first;
            gameState.Value.buttonPressed = false;
        }

        if (_counter == 2)
        {
            _counter = 0;
        }
    }
}
