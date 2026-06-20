#if UNITY_EDITOR
using Jarvis3DCommandCenter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreateCommandCenterScene
{
    private const string ScenePath = "Assets/Scenes/CommandCenterMockScene.unity";

    [MenuItem("Jarvis3D/Create Mock Command Center Scene")]
    public static void CreateScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var bootstrap = new GameObject("CommandCenterBootstrap");
        bootstrap.AddComponent<CommandCenterBootstrap>();

        // Ensure EventSystem exists for UI interaction.
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (!Application.isBatchMode)
        {
            Selection.activeGameObject = bootstrap;
        }
        Debug.Log($"[Jarvis3D] Scene created: {ScenePath}");
    }

    [MenuItem("Jarvis3D/Open Mock Command Center Scene")]
    public static void OpenScene()
    {
        if (!System.IO.File.Exists(ScenePath))
        {
            Debug.LogWarning("[Jarvis3D] Scene not found. Create it first from Jarvis3D/Create Mock Command Center Scene");
            return;
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log($"[Jarvis3D] Scene opened: {ScenePath}");
    }
}
#endif

