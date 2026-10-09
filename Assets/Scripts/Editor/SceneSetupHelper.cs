#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SpaceJet.Core;

namespace SpaceJet.Editor
{
    public static class SceneSetupHelper
    {
        [MenuItem("Space Jet/Build All Game Scenes and Settings", true)]
        public static bool ValidateBuildAllScenes()
        {
            return !EditorApplication.isPlaying;
        }

        [MenuItem("Space Jet/Build All Game Scenes and Settings")]
        public static void BuildAllScenes()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[SpaceJet] Cannot generate or save scene files while Play Mode is active. Please stop Play Mode first.");
                return;
            }

            Debug.Log("[SpaceJet] Starting Scene & Build Settings Generation...");

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            // 1. Build MainMenu Scene
            Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject bootMenu = new GameObject("=== SPACE_JET_BOOTSTRAPPER ===");
            bootMenu.AddComponent<GameBootstrapper>();
            EditorSceneManager.SaveScene(menuScene, "Assets/Scenes/MainMenu.unity");
            Debug.Log("[SpaceJet] Saved Assets/Scenes/MainMenu.unity");

            // 2. Build GameScene Scene
            Scene gameScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject bootGame = new GameObject("=== SPACE_JET_BOOTSTRAPPER ===");
            bootGame.AddComponent<GameBootstrapper>();
            EditorSceneManager.SaveScene(gameScene, "Assets/Scenes/GameScene.unity");
            Debug.Log("[SpaceJet] Saved Assets/Scenes/GameScene.unity");

            // 3. Configure Editor Build Settings
            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
            };
            EditorBuildSettings.scenes = buildScenes;
            Debug.Log("[SpaceJet] Successfully configured EditorBuildSettings.scenes (MainMenu = 0, GameScene = 1).");

            // Re-open MainMenu scene as the active editor scene
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SpaceJet] Complete! Game is ready to play in Unity Editor or Build.");
        }
    }
}
#endif
