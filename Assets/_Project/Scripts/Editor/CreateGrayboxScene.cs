using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using Dujiangyan.Core;
using Dujiangyan.Systems;
using Dujiangyan.UI;
using Dujiangyan.Data;

public class CreateGrayboxScene
{
    [MenuItem("Dujiangyan/Create L1 Graybox Scene")]
    public static void Create()
    {
        string scenePath = "Assets/_Project/Scenes/Level_L1.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 水墨淡彩配色（DESIGN §2）
        var paper = new Color(0.969f, 0.953f, 0.914f); // #f7f3e9
        var paperDark = new Color(0.910f, 0.878f, 0.816f); // #e8e0d0
        var inkDark = new Color(0.102f, 0.102f, 0.102f); // #1a1a1a
        var inkMid = new Color(0.290f, 0.290f, 0.290f); // #4a4a4a
        var waterColor = new Color(0.227f, 0.353f, 0.416f, 0.75f); // #3a5a6a
        var dangerColor = new Color(0.545f, 0.227f, 0.227f, 0.35f); // #8b3a3a

        Shader inkShader = Shader.Find("Dujiangyan/InkWash");
        var grayMat = CreateMaterial("InkPaper_Ground", inkShader, paperDark, inkDark);
        var waterMat = CreateMaterial("InkWater_Source", waterColor);
        var villageMat = CreateMaterial("InkWater_Village", dangerColor);
        var waterSurfaceMat = CreateMaterial("InkWater_Surface", waterColor);

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(7.5f, -0.05f, 5.5f);
        ground.transform.localScale = new Vector3(16f, 0.1f, 12f);
        ground.GetComponent<Renderer>().sharedMaterial = grayMat;

        // Low-poly ink-wash decorations
        CreateDecorations(ground.transform.parent);

        // Camera
        var camera = new GameObject("Main Camera");
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(7.5f, 14f, -8f);
        camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        var cam = camera.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = paper;
        cam.orthographic = true;
        cam.orthographicSize = 9f;
        var cameraData = cam.GetUniversalAdditionalCameraData();
        cameraData.renderPostProcessing = true;

        // Global Volume (post-processing)
        CreateGlobalVolume(paperDark, inkMid);

        // URP fog for distance haze
        EnableURPFog(inkMid);

        // Light
        var lightGO = new GameObject("Directional Light");
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;

        // Water surface (river)
        var waterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterPlane.name = "WaterSurface";
        waterPlane.transform.position = new Vector3(7.5f, 0.005f, 5.5f);
        waterPlane.transform.localScale = new Vector3(1.6f, 1f, 1.2f);
        waterPlane.GetComponent<Renderer>().sharedMaterial = waterSurfaceMat;
        Object.DestroyImmediate(waterPlane.GetComponent<Collider>());

        // Water source marker
        var waterMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        waterMarker.name = "WaterSource";
        waterMarker.transform.position = new Vector3(-6f, 0.55f, 2f);
        waterMarker.transform.localScale = Vector3.one * 0.4f;
        waterMarker.GetComponent<Renderer>().sharedMaterial = waterMat;
        Object.DestroyImmediate(waterMarker.GetComponent<Collider>());

        // Village area
        var village = GameObject.CreatePrimitive(PrimitiveType.Cube);
        village.name = "VillageArea";
        village.transform.position = new Vector3(4f, 0.05f, 2f);
        village.transform.localScale = new Vector3(3f, 0.1f, 3f);
        village.GetComponent<Renderer>().sharedMaterial = villageMat;
        village.GetComponent<BoxCollider>().isTrigger = true;

        // Services
        var services = new GameObject("Services");
        services.AddComponent<InputSystem>();
        services.AddComponent<AudioSystem>();
        services.AddComponent<SaveSystem>();
        services.AddComponent<UIManager>();
        services.AddComponent<PuzzleSystem>();
        services.AddComponent<WaterSimulation>();
        services.AddComponent<BlockPlacement>();
        services.AddComponent<PerformanceManager>();

        // UI Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Paper noise overlay (DESIGN §2.3: 6% background texture)
        CreatePaperNoiseOverlay(canvasGO.transform);

        var levelUI = canvasGO.AddComponent<LevelUI>();

        // Resource HUD
        var hudGO = CreatePanel(canvasGO.transform, "ResourceHUD", new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(16, -100), new Vector2(220, -16), new Color(1, 1, 1, 0.4f));
        var hud = hudGO.AddComponent<ResourceHUD>();
        var matLabel = CreateText(hudGO.transform, "MaterialLabel", "料 0", new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(8, -12), new Vector2(70, 12), 16, TextAlignmentOptions.Left);
        var laborLabel = CreateText(hudGO.transform, "LaborLabel", "工 0", new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(78, -12), new Vector2(140, 12), 16, TextAlignmentOptions.Left);
        var timeLabel = CreateText(hudGO.transform, "TimeLabel", "时 0", new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(148, -12), new Vector2(210, 12), 16, TextAlignmentOptions.Left);
        SetField(hud, "materialLabel", matLabel);
        SetField(hud, "laborLabel", laborLabel);
        SetField(hud, "timeLabel", timeLabel);

        // HintPill
        var pillGO = CreatePanel(canvasGO.transform, "HintPill", new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(-150, -70), new Vector2(150, -30), new Color(0.97f, 0.95f, 0.91f, 0.95f));
        pillGO.AddComponent<CanvasGroup>();
        var pillLabel = CreateText(pillGO.transform, "Label", "", Vector2.zero, Vector2.one,
            new Vector2(12, 8), new Vector2(-12, -8), 16, TextAlignmentOptions.Center);
        var pill = pillGO.AddComponent<HintPill>();
        SetField(pill, "label", pillLabel);
        SetField(pill, "canvasGroup", pillGO.GetComponent<CanvasGroup>());

        // Toolbar
        var toolbarGO = CreatePanel(canvasGO.transform, "Toolbar", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(-140, 80), new Vector2(140, 16), new Color(1, 1, 1, 0.3f));
        var toolbar = toolbarGO.AddComponent<Toolbar>();
        var bambooBtn = CreateToolbarItem(toolbarGO.transform, "竹笼", new Vector2(-70, 0));
        var maochaBtn = CreateToolbarItem(toolbarGO.transform, "杩槎", new Vector2(0, 0));
        var rotateBtn = CreateToolbarItem(toolbarGO.transform, "旋转", new Vector2(70, 0));
        rotateBtn.button.onClick.AddListener(levelUI.RotatePending);

        var toolbarItems = new Toolbar.ToolbarItem[2];
        toolbarItems[0] = new Toolbar.ToolbarItem { blockId = "bamboo", button = bambooBtn.button, countLabel = bambooBtn.count, background = bambooBtn.bg };
        toolbarItems[1] = new Toolbar.ToolbarItem { blockId = "maocha", button = maochaBtn.button, countLabel = maochaBtn.count, background = maochaBtn.bg };
        SetField(toolbar, "items", toolbarItems);
        bambooBtn.button.onClick.AddListener(levelUI.SelectBamboo);
        maochaBtn.button.onClick.AddListener(levelUI.SelectMaocha);

        // Pause button
        var pauseBtn = CreateButton(canvasGO.transform, "暂停", new Vector2(40, 40), levelUI.TogglePause);

        // Action buttons
        var simulateBtn = CreateButton(canvasGO.transform, "放水", new Vector2(200, 40), levelUI.StartSimulation);
        var undoBtn = CreateButton(canvasGO.transform, "撤销", new Vector2(320, 40), levelUI.Undo);
        var resetBtn = CreateButton(canvasGO.transform, "重置", new Vector2(440, 40), levelUI.ResetLevel);

        // Status text
        var statusText = CreateText(canvasGO.transform, "StatusText", "编辑阶段",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-100, -120), new Vector2(100, -90), 20, TextAlignmentOptions.Center);

        // Inventory text (legacy small)
        var inventoryText = CreateText(canvasGO.transform, "InventoryText", "",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(240, -60), new Vector2(420, -30), 14, TextAlignmentOptions.Left);

        // HintDialog
        var hintDialogGO = CreatePanel(canvasGO.transform, "HintDialog", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(-200, 120), new Vector2(200, 16), new Color(0.97f, 0.95f, 0.91f, 0.98f));
        var hintDialogCG = hintDialogGO.AddComponent<CanvasGroup>();
        var hintSpeaker = CreateText(hintDialogGO.transform, "Speaker", "老河工", new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(12, -40), new Vector2(120, -12), 16, TextAlignmentOptions.Left);
        var hintContent = CreateText(hintDialogGO.transform, "Content", "", new Vector2(0, 0.5f), new Vector2(1, 0.5f),
            new Vector2(12, -24), new Vector2(-12, 24), 16, TextAlignmentOptions.Left);
        var hintClose = CreateButton(hintDialogGO.transform, "知道了", new Vector2(0, -40), null);
        var hintDialog = hintDialogGO.AddComponent<HintDialog>();
        SetField(hintDialog, "speakerLabel", hintSpeaker);
        SetField(hintDialog, "contentLabel", hintContent);
        SetField(hintDialog, "closeButton", hintClose);
        SetField(hintDialog, "canvasGroup", hintDialogCG);
        hintClose.onClick.AddListener(hintDialog.Hide);

        // ResultModal
        var resultGO = CreateFullScreenOverlay(canvasGO.transform, "ResultModal");
        var resultCG = resultGO.AddComponent<CanvasGroup>();
        resultCG.alpha = 0;
        resultCG.blocksRaycasts = false;

        var screen1 = CreatePanel(resultGO.transform, "Screen1", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.97f, 0.95f, 0.91f, 1f));
        screen1.AddComponent<CanvasGroup>();
        var screen2 = CreatePanel(resultGO.transform, "Screen2", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.97f, 0.95f, 0.91f, 1f));
        screen2.AddComponent<CanvasGroup>();
        screen2.SetActive(false);

        var seal = CreateText(screen1.transform, "Seal", "安", new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f),
            new Vector2(-60, -60), new Vector2(60, 60), 64, TextAlignmentOptions.Center);
        var resultTitle = CreateText(screen1.transform, "Title", "暂时安全", new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f),
            new Vector2(-120, -30), new Vector2(120, 10), 28, TextAlignmentOptions.Center);
        var resultDesc = CreateText(screen1.transform, "Desc", "", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-160, -20), new Vector2(160, 40), 18, TextAlignmentOptions.Center);

        var successGroup = new GameObject("SuccessGroup");
        successGroup.transform.SetParent(screen1.transform, false);
        var failGroup = new GameObject("FailGroup");
        failGroup.transform.SetParent(screen1.transform, false);

        var galleryBtn = CreateButton(screen1.transform, "查看碑廊", new Vector2(0, -60), null);
        var retryBtnS1 = CreateButton(screen1.transform, "再试一次", new Vector2(0, -120), null);

        var galleryTitle = CreateText(screen2.transform, "GalleryTitle", "堵不如疏", new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f),
            new Vector2(-120, -30), new Vector2(120, 10), 28, TextAlignmentOptions.Center);
        var galleryContent = CreateText(screen2.transform, "GalleryContent", "", new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f),
            new Vector2(-180, -80), new Vector2(180, 80), 18, TextAlignmentOptions.Center);
        var retryBtnS2 = CreateButton(screen2.transform, "再试一次", new Vector2(-80, -140), null);
        var nextLevelBtn = CreateButton(screen2.transform, "下一关", new Vector2(80, -140), null);

        var resultModal = resultGO.AddComponent<ResultModal>();
        SetField(resultModal, "screen1", screen1);
        SetField(resultModal, "screen2", screen2);
        SetField(resultModal, "successGroup", successGroup);
        SetField(resultModal, "successTitle", resultTitle);
        SetField(resultModal, "successDesc", resultDesc);
        SetField(resultModal, "galleryButton", galleryBtn);
        SetField(resultModal, "failGroup", failGroup);
        SetField(resultModal, "failTitle", resultTitle);
        SetField(resultModal, "failDesc", resultDesc);
        SetField(resultModal, "sealLabel", seal);
        SetField(resultModal, "retryButtonS1", retryBtnS1);
        SetField(resultModal, "retryButtonS2", retryBtnS2);
        SetField(resultModal, "nextLevelButton", nextLevelBtn);
        SetField(resultModal, "galleryContent", galleryContent);

        // PauseMenu
        var pauseGO = CreateFullScreenOverlay(canvasGO.transform, "PauseMenu");
        var pauseImage = pauseGO.GetComponent<Image>();
        pauseImage.color = new Color(0.97f, 0.95f, 0.91f, 0.95f);
        var pauseCG = pauseGO.AddComponent<CanvasGroup>();

        var pauseTitle = CreateText(pauseGO.transform, "Title", "已暂停", new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f),
            new Vector2(-100, -30), new Vector2(100, 10), 28, TextAlignmentOptions.Center);
        var resumeBtn = CreateButton(pauseGO.transform, "继续", new Vector2(0, 40), null);
        var retryBtnPause = CreateButton(pauseGO.transform, "重试", new Vector2(0, -30), null);
        var settingsPanel = CreatePanel(pauseGO.transform, "Settings", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-120, -60), new Vector2(120, -100), new Color(1, 1, 1, 0.4f));
        var musicLabel = CreateText(settingsPanel.transform, "MusicLabel", "音乐", new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(10, -30), new Vector2(60, -8), 14, TextAlignmentOptions.Left);
        var musicSliderGO = CreateSlider(settingsPanel.transform, new Vector2(70, -20), "MusicSlider");
        var sfxLabel = CreateText(settingsPanel.transform, "SfxLabel", "音效", new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(10, -15), new Vector2(60, 15), 14, TextAlignmentOptions.Left);
        var sfxSliderGO = CreateSlider(settingsPanel.transform, new Vector2(70, 0), "SfxSlider");
        var qualityBtn = CreateButton(pauseGO.transform, "画质：中", new Vector2(-80, -100), null);
        var languageBtn = CreateButton(pauseGO.transform, "语言：中文", new Vector2(80, -100), null);
        var titleBtn = CreateButton(pauseGO.transform, "返回标题", new Vector2(0, -160), null);

        var pauseMenu = pauseGO.AddComponent<PauseMenu>();
        SetField(pauseMenu, "canvasGroup", pauseCG);
        SetField(pauseMenu, "resumeButton", resumeBtn);
        SetField(pauseMenu, "retryButton", retryBtnPause);
        SetField(pauseMenu, "musicSlider", musicSliderGO.GetComponent<Slider>());
        SetField(pauseMenu, "sfxSlider", sfxSliderGO.GetComponent<Slider>());
        SetField(pauseMenu, "qualityButton", qualityBtn);
        SetField(pauseMenu, "languageButton", languageBtn);
        SetField(pauseMenu, "titleButton", titleBtn);

        // Wire LevelUI
        SetField(levelUI, "hintPill", pill);
        SetField(levelUI, "hintDialog", hintDialog);
        SetField(levelUI, "resultModal", resultModal);
        SetField(levelUI, "pauseMenu", pauseMenu);

        // Bootstrap
        var bootstrapGO = new GameObject("LevelBootstrap");
        var bootstrap = bootstrapGO.AddComponent<LevelBootstrap>();
        SetField(bootstrap, "inventoryText", inventoryText);
        SetField(bootstrap, "statusText", statusText);
        SetField(bootstrap, "levelUI", levelUI);

        // HintSystem
        var hintSystemGO = new GameObject("HintSystem");
        var hintSystem = hintSystemGO.AddComponent<HintSystem>();
        SetField(hintSystem, "levelUI", levelUI);

        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CreateGrayboxScene] Saved graybox scene to {scenePath}");
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string dir = "Assets/_Project/Art/Materials/Graybox";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            string parent = System.IO.Path.GetDirectoryName(dir).Replace('\\', '/');
            string folderName = System.IO.Path.GetFileName(dir);
            AssetDatabase.CreateFolder(parent, folderName);
        }
        string path = $"{dir}/{name}.mat";
        AssetDatabase.DeleteAsset(path);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.name = name;
        mat.color = color;
        if (color.a < 1f)
        {
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
        }
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static Material CreateMaterial(string name, Shader shader, Color baseColor, Color inkColor)
    {
        string dir = "Assets/_Project/Art/Materials/Graybox";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            string parent = System.IO.Path.GetDirectoryName(dir).Replace('\\', '/');
            string folderName = System.IO.Path.GetFileName(dir);
            AssetDatabase.CreateFolder(parent, folderName);
        }
        string path = $"{dir}/{name}.mat";
        AssetDatabase.DeleteAsset(path);
        var mat = new Material(shader);
        mat.name = name;
        mat.SetColor("_BaseColor", baseColor);
        mat.SetColor("_InkColor", inkColor);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
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

    private static GameObject CreateFullScreenOverlay(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.5f);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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
        rect.sizeDelta = new Vector2(100f, 50f);
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

    private static (Button button, TextMeshProUGUI count, Image bg) CreateToolbarItem(Transform parent, string label, Vector2 anchoredPos)
    {
        var go = new GameObject($"ToolbarItem_{label}");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(60f, 60f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;

        var bg = go.AddComponent<Image>();
        bg.color = new Color(1, 1, 1, 0.4f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        go.AddComponent<Dujiangyan.UI.ButtonFeedback>();

        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(go.transform, false);
        var icon = iconGO.AddComponent<TextMeshProUGUI>();
        icon.text = label.Substring(0, 1);
        icon.fontSize = 22;
        icon.alignment = TextAlignmentOptions.Center;
        icon.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        if (TMP_Settings.defaultFontAsset != null)
            icon.font = TMP_Settings.defaultFontAsset;
        var iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.3f);
        iconRect.anchorMax = new Vector2(1, 1);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        var countGO = new GameObject("Count");
        countGO.transform.SetParent(go.transform, false);
        var count = countGO.AddComponent<TextMeshProUGUI>();
        count.text = "0";
        count.fontSize = 11;
        count.alignment = TextAlignmentOptions.Center;
        count.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        if (TMP_Settings.defaultFontAsset != null)
            count.font = TMP_Settings.defaultFontAsset;
        var countRect = countGO.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0, 0);
        countRect.anchorMax = new Vector2(1, 0.3f);
        countRect.offsetMin = Vector2.zero;
        countRect.offsetMax = Vector2.zero;

        return (btn, count, bg);
    }

    private static GameObject CreateSlider(Transform parent, Vector2 anchoredPos, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.7f;
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120f, 20f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(go.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.8f, 0.8f, 0.8f);
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        slider.targetGraphic = bgImg;

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillArea.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.18f, 0.35f, 0.29f);
        var fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        slider.fillRect = fillRect;
        slider.direction = Slider.Direction.LeftToRight;

        return go;
    }

    private static void CreateDecorations(Transform root)
    {
        Shader inkShader = Shader.Find("Dujiangyan/InkWash");
        if (inkShader == null) return;

        var rockMat = CreateMaterial("InkDecoration_Rock", inkShader, new Color(0.65f, 0.62f, 0.58f), new Color(0.1f, 0.1f, 0.1f));
        var treeMat = CreateMaterial("InkDecoration_Tree", inkShader, new Color(0.35f, 0.45f, 0.38f), new Color(0.08f, 0.12f, 0.1f));
        var woodMat = CreateMaterial("InkDecoration_Wood", inkShader, new Color(0.65f, 0.55f, 0.45f), new Color(0.12f, 0.08f, 0.06f));

        string modelDir = "Assets/_Project/Art/Models/Kenney_NatureKit";

        // River banks: left and right of the central channel (z ≈ 0..4)
        for (int x = -6; x <= 8; x += 1)
        {
            PlacePrefab($"{modelDir}/cliff_block_stone.fbx", new Vector3(x, 0.45f, -0.5f), Vector3.zero, Vector3.one * 0.9f, rockMat, root);
            PlacePrefab($"{modelDir}/cliff_block_stone.fbx", new Vector3(x, 0.45f, 4.5f), new Vector3(0, 180f, 0), Vector3.one * 0.9f, rockMat, root);
        }

        // Distant mountains along the far edges
        for (int z = -4; z <= 10; z += 2)
        {
            PlacePrefab($"{modelDir}/cliff_large_stone.fbx", new Vector3(-8f, 1.0f, z), new Vector3(0, 90f, 0), Vector3.one * 1.4f, rockMat, root);
            PlacePrefab($"{modelDir}/cliff_large_stone.fbx", new Vector3(17f, 1.0f, z), new Vector3(0, -90f, 0), Vector3.one * 1.4f, rockMat, root);
        }
        for (int x = -6; x <= 14; x += 2)
        {
            PlacePrefab($"{modelDir}/cliff_large_stone.fbx", new Vector3(x, 1.0f, -5f), Vector3.zero, Vector3.one * 1.4f, rockMat, root);
            PlacePrefab($"{modelDir}/cliff_large_stone.fbx", new Vector3(x, 1.0f, 13f), new Vector3(0, 180f, 0), Vector3.one * 1.4f, rockMat, root);
        }

        // Trees scattered on banks and background
        string[] treePaths = {
            $"{modelDir}/tree_pineTallA.fbx",
            $"{modelDir}/tree_pineRoundA.fbx",
            $"{modelDir}/tree_oak.fbx",
            $"{modelDir}/tree_pineSmallA.fbx"
        };
        System.Random rng = new System.Random(42);
        for (int i = 0; i < 24; i++)
        {
            float x = -7f + (float)(rng.NextDouble() * 22f);
            float z = -4f + (float)(rng.NextDouble() * 14f);
            // Avoid the central river channel
            if (z > 0f && z < 4f) continue;
            string path = treePaths[rng.Next(treePaths.Length)];
            float scale = 0.5f + (float)(rng.NextDouble() * 0.5f);
            float rot = (float)(rng.NextDouble() * 360f);
            PlacePrefab(path, new Vector3(x, 0.2f, z), new Vector3(0, rot, 0), Vector3.one * scale, treeMat, root);
        }

        // Village tents near the village area (4, 2)
        PlacePrefab($"{modelDir}/tent_detailedClosed.fbx", new Vector3(3.0f, 0.05f, 1.0f), new Vector3(0, -30f, 0), Vector3.one * 0.8f, woodMat, root);
        PlacePrefab($"{modelDir}/tent_smallClosed.fbx", new Vector3(4.5f, 0.05f, 2.5f), new Vector3(0, 60f, 0), Vector3.one * 0.8f, woodMat, root);
        PlacePrefab($"{modelDir}/tent_detailedClosed.fbx", new Vector3(5.0f, 0.05f, 0.5f), new Vector3(0, 120f, 0), Vector3.one * 0.75f, woodMat, root);
    }

    private static void PlacePrefab(string assetPath, Vector3 position, Vector3 rotation, Vector3 scale, Material material, Transform parent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[CreateGrayboxScene] Missing prefab: {assetPath}");
            return;
        }

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(rotation);
        go.transform.localScale = scale;

        foreach (var rend in go.GetComponentsInChildren<Renderer>())
            rend.sharedMaterial = material;
    }

    private static void EnableURPFog(Color fogColor)
    {
        var urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            var prop = urpAsset.GetType().GetProperty("supportsFog");
            if (prop != null)
                prop.SetValue(urpAsset, true);
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogStartDistance = 12f;
        RenderSettings.fogEndDistance = 32f;
    }

    private static void CreatePaperNoiseOverlay(Transform canvas)
    {
        string texDir = "Assets/_Project/Art/UI/Textures";
        string texPath = $"{texDir}/PaperNoise.png";
        if (!AssetDatabase.IsValidFolder(texDir))
        {
            string parent = System.IO.Path.GetDirectoryName(texDir).Replace('\\', '/');
            string folderName = System.IO.Path.GetFileName(texDir);
            AssetDatabase.CreateFolder(parent, folderName);
        }
        AssetDatabase.DeleteAsset(texPath);

        int size = 512;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.5f +
                          Mathf.PerlinNoise(x * 0.07f, y * 0.07f) * 0.5f;
                tex.SetPixel(x, y, new Color(n, n, n, 1f));
            }
        }
        tex.Apply();
        System.IO.File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath);

        var go = new GameObject("PaperNoiseOverlay");
        go.transform.SetParent(canvas, false);
        var raw = go.AddComponent<RawImage>();
        raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        raw.color = new Color(1f, 1f, 1f, 0.06f);
        raw.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void CreateGlobalVolume(Color vignetteColor, Color fogColor)
    {
        string profilePath = "Assets/_Project/Settings/VolumeProfile_L1.asset";
        AssetDatabase.DeleteAsset(profilePath);

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "VolumeProfile_L1";

        var colorAdjustments = profile.Add<ColorAdjustments>();
        colorAdjustments.saturation.Override(-25f);
        colorAdjustments.contrast.Override(10f);

        var vignette = profile.Add<Vignette>();
        vignette.intensity.Override(0.25f);
        vignette.color.Override(vignetteColor);
        vignette.smoothness.Override(0.4f);

        var tonemapping = profile.Add<Tonemapping>();
        tonemapping.mode.Override(TonemappingMode.Neutral);

        AssetDatabase.CreateAsset(profile, profilePath);

        var volumeGO = new GameObject("Global Volume");
        var volume = volumeGO.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.profile = profile;
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        field?.SetValue(target, value);
    }
}
