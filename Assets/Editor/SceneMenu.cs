#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace EditorTools
{
    [InitializeOnLoad]
    public static class SceneMenu
    {                
        static SceneMenu() {}

        [MenuItem("Scenes/Join Session")]
        private static void JoinSession()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Init.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Scenes/Go To/Init Scene")]
        private static void OpenInitScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Init.unity", OpenSceneMode.Single);
        }

        [MenuItem("Scenes/Go To/Room Scene")]
        private static void OpenRoomScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Scena_Rehab.unity", OpenSceneMode.Single);
        }
    }
}
#endif
