#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace EditorTools
{
    [InitializeOnLoad]
    public static class SceneMenu
    {
        private const string KEY = "StartAsHost";
                
        static SceneMenu() {}

        [MenuItem("Scenes/Launch/Server")]
        private static void LaunchServer()
        {
            EditorPrefs.SetBool(KEY, true);
            EditorSceneManager.OpenScene("Assets/Scenes/Init.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Scenes/Launch/Client")]
        private static void LaunchClient()
        {
            EditorPrefs.SetBool(KEY, false);
            EditorSceneManager.OpenScene("Assets/Scenes/Init.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Scenes/Go To Scene/Init")]
        private static void OpenInitScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Init.unity", OpenSceneMode.Single);
        }

        [MenuItem("Scenes/Go To Scene/Room")]
        private static void OpenRoomScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Scena_Rehab.unity", OpenSceneMode.Single);
        }
    }
}
#endif
