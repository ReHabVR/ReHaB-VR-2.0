#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Editor
{
    [InitializeOnLoad]
    public static class SceneMenu
    {
        static SceneMenu() {}

        [MenuItem("ReHaB/Join Session")]
        private static void JoinSession()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Init.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("ReHaB/Go To Scene/Init Scene")]
        private static void OpenInitScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Init.unity", OpenSceneMode.Single);
        }

        [MenuItem("ReHaB/Go To Scene/Room Scene")]
        private static void OpenRoomScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Scena_Rehab.unity", OpenSceneMode.Single);
        }
    }
}
#endif
