using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    
    // public InputField createInput;
    // public InputField joinInput;
    public string roomNameConstant = "testRoom";
    private int checker = 0;

    void Awake()
    {
        JoinRoom();
    }

    public void CreateRoom()
    {
        // PhotonNetwork.CreateRoom(createInput.text);
        PhotonNetwork.CreateRoom(roomNameConstant);
    }

    public void JoinRoom()
    {
        // PhotonNetwork.JoinRoom(joinInput.text);
        PhotonNetwork.JoinRoom(roomNameConstant);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Connection to PhotonNetwork succeeded");
        PhotonNetwork.LoadLevel("Scena_Rehab");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (message == "Game does not exist")
        {
            Debug.Log("Connection to PhotonNetwork failed, trying one more time");
            CreateRoom();
            JoinRoom();
        } else
        {
            Debug.Log("Connection to PhotonNetwork failed, trying one more time");
            JoinRoom();
        }
    }

}