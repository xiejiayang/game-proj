using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Dujiangyan.UI;

public class CreateM4Scenes
{
    [MenuItem("Dujiangyan/Create M4 Title + Intro Scenes")]
    public static void CreateAll()
    {
        CreateTitleScene();
        CreateIntroScene();
        UpdateBuildSettings();
    }

    private static void CreateTitleScene()
    {
        string scenePath = "Assets/_Project/Scenes/Title.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camera = new GameObject("Main Camera");
        camera.tag = "MainCamera";
        camera.AddComponent<Camera>().backgroundColor = new Color(0.969f, 0.953f, 0.914f);

        var canvasGO = CreateCanvas(scene);
        CreatePaperNoiseOverlay(canvasGO.transform);

        var bg = CreatePanel(canvasGO.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.1f, 0.1f, 0.1f, 0.2f));
        var bgImage = bg.GetComponent<Image>();
        bgImage.sprite = LoadSprite("Assets/_Project/Art/AI_Generated/Narrative/L1_Intro_Narrative.png");
        bgImage.type = Image.Type.Simple;
        bgImage.color = new Color(1, 1, 1, 0.25f);

        var title = CreateText(canvasGO.transform, "Title", "都江堰", new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f),
            new Vector2(-150, -40), new Vector2(150, 40), 64, TextAlignmentOptions.Center);
        title.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        title.fontStyle = FontStyles.Bold;

        var subtitle = CreateText(canvasGO.transform, "Subtitle", "堵不如疏", new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f),
            new Vector2(-120, -20), new Vector2(120, 20), 28, TextAlignmentOptions.Center);
        subtitle.color = new Color(0.29f, 0.29f, 0.29f, 1f);

        var startBtn = CreateButton(canvasGO.transform, "开始治水", new Vector2(0, -80), () =>
        {
            SceneManager.LoadScene("Intro_L1");
        });
        var btnImage = startBtn.GetComponent<Image>();
        btnImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        var btnLabel = startBtn.GetComponentInChildren<TextMeshProUGUI>();
        btnLabel.color = new Color(0.969f, 0.953f, 0.914f, 1f);
        btnLabel.fontSize = 22;

        var titleUI = camera.AddComponent<TitleUI>();
        titleUI.SetButton(startBtn);

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[CreateM4Scenes] Saved title scene to {scenePath}");
    }

    private static void CreateIntroScene()
    {
        string scenePath = "Assets/_Project/Scenes/Intro_L1.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camera = new GameObject("Main Camera");
        camera.tag = "MainCamera";
        camera.AddComponent<Camera>().backgroundColor = new Color(0.969f, 0.953f, 0.914f);

        var canvasGO = CreateCanvas(scene);
        CreatePaperNoiseOverlay(canvasGO.transform);

        var image = CreateRawImage(canvasGO.transform, "NarrativeImage", "Assets/_Project/Art/AI_Generated/Narrative/L1_Intro_Narrative.png");

        var overlay = CreatePanel(canvasGO.transform, "Overlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.1f, 0.1f, 0.1f, 0.35f));
        overlay.transform.SetAsLastSibling();

        var headline = CreateText(canvasGO.transform, "Headline", "官方筑墙失败，村庄危急", new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f),
            new Vector2(-200, -30), new Vector2(200, 30), 26, TextAlignmentOptions.Center);
        headline.color = new Color(0.97f, 0.95f, 0.91f, 1f);
        headline.fontStyle = FontStyles.Bold;

        var body = CreateText(canvasGO.transform, "Body", "李冰留下的治水智慧，或许藏在「堵不如疏」四个字里。", new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f),
            new Vector2(-220, -60), new Vector2(220, 20), 18, TextAlignmentOptions.Center);
        body.color = new Color(0.95f, 0.93f, 0.89f, 1f);

        var skipBtn = CreateButton(canvasGO.transform, "跳过", new Vector2(140, -40), () =>
        {
            SceneManager.LoadScene("Level_L1");
        });
        var skipImage = skipBtn.GetComponent<Image>();
        skipImage.color = new Color(1, 1, 1, 0.2f);
        var skipLabel = skipBtn.GetComponentInChildren<TextMeshProUGUI>();
        skipLabel.color = new Color(0.97f, 0.95f, 0.91f, 1f);
        skipLabel.fontSize = 16;

        var enterBtn = CreateButton(canvasGO.transform, "进入关卡", new Vector2(0, -140), () =>
        {
            SceneManager.LoadScene("Level_L1");
        });
        var enterImage = enterBtn.GetComponent<Image>();
        enterImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        var enterLabel = enterBtn.GetComponentInChildren<TextMeshProUGUI>();
        enterLabel.color = new Color(0.969f, 0.953f, 0.914f, 1f);
        enterLabel.fontSize = 22;

        var introUI = camera.AddComponent<IntroUI>();
        introUI.SetButtons(skipBtn, enterBtn);

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[CreateM4Scenes] Saved intro scene to {scenePath}");
    }

    private static void UpdateBuildSettings()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/_Project/Scenes/Title.unity", true),
            new EditorBuildSettingsScene("Assets/_Project/Scenes/Intro_L1.unity", true),
            new EditorBuildSettingsScene("Assets/_Project/Scenes/Level_L1.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[CreateM4Scenes] Updated EditorBuildSettings");
    }

    private static GameObject CreateCanvas(Scene scene)
    {
        var go = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private static void CreatePaperNoiseOverlay(Transform canvas)
    {
        string texPath = "Assets/_Project/Art/UI/Textures/PaperNoise.png";
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (tex == null) return;

        var go = new GameObject("PaperNoiseOverlay");
        go.transform.SetParent(canvas, false);
        var raw = go.AddComponent<RawImage>();
        raw.texture = tex;
        raw.color = new Color(1f, 1f, 1f, 0.06f);
        raw.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return go;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return text;
    }

    private static Button CreateButton(Transform parent, string label, Vector2 anchoredPos, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject($"Btn_{label}");
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = image;
        if (action != null)
            btn.onClick.AddListener(action);
        go.AddComponent<Dujiangyan.UI.ButtonFeedback>();

        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160f, 50f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return btn;
    }

    private static GameObject CreateRawImage(Transform parent, string name, string texturePath)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var raw = go.AddComponent<RawImage>();
        raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        raw.color = Color.white;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return go;
    }

    private static Sprite LoadSprite(string path)
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
}
