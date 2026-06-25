using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
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

        var grayMat = CreateMaterial("Graybox_Ground", new Color(0.75f, 0.72f, 0.68f));
        var waterMat = CreateMaterial("Graybox_Water", new Color(0.2f, 0.45f, 0.75f));
        var villageMat = CreateMaterial("Graybox_Village", new Color(0.8f, 0.3f, 0.3f, 0.4f));
        villageMat.SetFloat("_Surface", 1); // transparent

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(7.5f, -0.05f, 5.5f);
        ground.transform.localScale = new Vector3(16f, 0.1f, 12f);
        ground.GetComponent<Renderer>().sharedMaterial = grayMat;

        // Camera
        var camera = new GameObject("Main Camera");
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(7.5f, 14f, -8f);
        camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        var cam = camera.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.9f, 0.88f, 0.82f);
        cam.orthographic = true;
        cam.orthographicSize = 9f;

        // Light
        var lightGO = new GameObject("Directional Light");
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;

        // Water source marker
        var waterMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        waterMarker.name = "WaterSource";
        waterMarker.transform.position = new Vector3(-6f, 0.5f, 2f);
        waterMarker.transform.localScale = Vector3.one * 0.5f;
        waterMarker.GetComponent<Renderer>().sharedMaterial = waterMat;
        Object.DestroyImmediate(waterMarker.GetComponent<Collider>());

        // Village area
        var village = GameObject.CreatePrimitive(PrimitiveType.Cube);
        village.name = "VillageArea";
        village.transform.position = new Vector3(4f, 0.05f, 2f);
        village.transform.localScale = new Vector3(3f, 0.1f, 3f);
        village.GetComponent<Renderer>().sharedMaterial = villageMat;
        var villageCollider = village.GetComponent<BoxCollider>();
        villageCollider.isTrigger = true;

        // Services
        var services = new GameObject("Services");
        services.AddComponent<InputSystem>();
        services.AddComponent<AudioSystem>();
        services.AddComponent<SaveSystem>();
        services.AddComponent<UIManager>();
        services.AddComponent<PuzzleSystem>();
        services.AddComponent<WaterSimulation>();
        services.AddComponent<BlockPlacement>();

        // UI
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();
        var levelUI = canvasGO.AddComponent<LevelUI>();

        var inventoryText = CreateText(canvasGO.transform, "InventoryText", "竹笼: 0  杩槎: 0  石墙: 0",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-200f, -60f), new Vector2(200f, -10f), 28);
        var statusText = CreateText(canvasGO.transform, "StatusText", "编辑阶段",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-150f, -110f), new Vector2(150f, -70f), 24);

        float btnY = 120f;
        float spacing = 110f;
        CreateButton(canvasGO.transform, "竹笼", new Vector2(spacing * 0, btnY), levelUI.SelectBamboo);
        CreateButton(canvasGO.transform, "杩槎", new Vector2(spacing * 1, btnY), levelUI.SelectMaocha);
        CreateButton(canvasGO.transform, "旋转", new Vector2(spacing * 2, btnY), levelUI.RotatePending);
        CreateButton(canvasGO.transform, "放水", new Vector2(spacing * 3, btnY), levelUI.StartSimulation);
        CreateButton(canvasGO.transform, "重置", new Vector2(spacing * 4, btnY), levelUI.ResetLevel);

        // Bootstrap
        var bootstrapGO = new GameObject("LevelBootstrap");
        var bootstrap = bootstrapGO.AddComponent<LevelBootstrap>();
        bootstrap.GetType().GetField("inventoryText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(bootstrap, inventoryText);
        bootstrap.GetType().GetField("statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(bootstrap, statusText);

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

    private static TextMeshProUGUI CreateText(Transform parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
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
        btn.onClick.AddListener(action);

        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100f, 60f);
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPos;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return btn;
    }
}
