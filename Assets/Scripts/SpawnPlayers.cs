using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    
    public GameObject playerPrefab;
    public GameObject playerWithoutArmMale;
    public GameObject playerWithoutArmFemale;
    public GameObject secondChairPrefab;
    public GameObject viewerPrefab;
    public bool isViewer = false;
    public int playerModel;

    private void Start()
    {
        if (isViewer)
        {
            SpawnBroadcast();
        } else 
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer() 
    {
        int playerCount = PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
        Vector3 position;
        Vector3 chairPosition;
        if (playerCount == 1)
        {
            position = new Vector3(9.102f, -0.41f, 6.734f);
			//teraz jest zrobione na gracza bez prawej ręki, ale przez to inne animacje nie mają jak działać(przenoszenie piłeczek itp), bo teraz są tylko animacje zrobione przez Martę
			//trzeba zamienić jeśli chcesz prefab z obydwoma rękami
            //PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity * Quaternion.Euler(0, -90f, 0));
			playerModel = 1;
            switch (playerModel)
            {
            //PhotonNetwork.Instantiate(playerWithoutArm.name, position, Quaternion.identity * Quaternion.Euler(0, -90f, 0));
                case 0:
                    PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity * Quaternion.Euler(0, -90f, 0));
                    break;
                case 1:
                    PhotonNetwork.Instantiate(playerWithoutArmMale.name, position, Quaternion.identity * Quaternion.Euler(0, -90f, 0));
                    break;
                case 2:
                    PhotonNetwork.Instantiate(playerWithoutArmFemale.name, position, Quaternion.identity * Quaternion.Euler(0, -90f, 0));
                    break;
                default:
                    PhotonNetwork.Instantiate(playerWithoutArmMale.name, position, Quaternion.identity * Quaternion.Euler(0, -90f, 0));
                    break;
			}
        } else
        {
            position = new Vector3(7.65f, -0.41f, 6.734f);
            chairPosition = new Vector3(7.71f, -0.042f, 6.734f);
            PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity * Quaternion.Euler(0,-265f,0));
            PhotonNetwork.Instantiate(secondChairPrefab.name, chairPosition, Quaternion.identity * Quaternion.Euler(-90f,0,90f));
        }
    }

    public void SpawnBroadcast()
    {
        Instantiate(viewerPrefab);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        SceneManager.LoadScene("Initialization");
    }
}
