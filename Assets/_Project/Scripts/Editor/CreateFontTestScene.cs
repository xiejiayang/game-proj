using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateFontTestScene
{
    [MenuItem("Dujiangyan/Create Font Test Scene")]
    public static void Create()
    {
        string scenePath = "Assets/_Project/Scenes/FontTest.unity";

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camera = new GameObject("Main Camera");
        camera.tag = "MainCamera";
        camera.AddComponent<Camera>().backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);

        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Title text
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(canvasGO.transform, false);
        var title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text = "都江堰";
        title.fontSize = 72;
        title.alignment = TextAlignmentOptions.Center;
        title.color = Color.white;
        title.rectTransform.anchorMin = Vector2.zero;
        title.rectTransform.anchorMax = Vector2.one;
        title.rectTransform.offsetMin = Vector2.zero;
        title.rectTransform.offsetMax = Vector2.zero;

        // Subtitle text
        var subtitleGO = new GameObject("Subtitle");
        subtitleGO.transform.SetParent(canvasGO.transform, false);
        var subtitle = subtitleGO.AddComponent<TextMeshProUGUI>();
        subtitle.text = "Dujiangyan / 核心解谜系统";
        subtitle.fontSize = 36;
        subtitle.alignment = TextAlignmentOptions.Center;
        subtitle.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        subtitle.rectTransform.anchorMin = new Vector2(0, 0.4f);
        subtitle.rectTransform.anchorMax = new Vector2(1, 0.55f);
        subtitle.rectTransform.offsetMin = Vector2.zero;
        subtitle.rectTransform.offsetMax = Vector2.zero;

        // Force TMP to use the configured default font asset.
        if (TMP_Settings.instance != null && TMP_Settings.defaultFontAsset != null)
        {
            title.font = TMP_Settings.defaultFontAsset;
            subtitle.font = TMP_Settings.defaultFontAsset;
        }

        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        Debug.Log($"[CreateFontTestScene] Saved test scene to {scenePath}");
    }
}
