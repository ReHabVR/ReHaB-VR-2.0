using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SceneMenu : MonoBehaviour
{
    
    [MenuItem("Scenes/Launch Init Scene")]
    private static void PlayInitScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Initialization.unity", OpenSceneMode.Single);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Scenes/Go To Scene/Initialization")]
    private static void OpenInitScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Initialization.unity", OpenSceneMode.Single);
    }

    [MenuItem("Scenes/Go To Scene/Lobby")]
    private static void OpenLobbyScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Lobby.unity", OpenSceneMode.Single);
    }

    [MenuItem("Scenes/Go To Scene/Room")]
    private static void OpenRoomScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Scena_Rehab.unity", OpenSceneMode.Single);
    }
}
