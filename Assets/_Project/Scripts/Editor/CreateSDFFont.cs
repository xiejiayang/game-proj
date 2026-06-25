using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.TextCore.LowLevel;

public class CreateSDFFont
{
    private const string FontPath = "Assets/_Project/Art/UI/Fonts/NotoSerifSC-Regular.ttf";
    private const string SdfOutputPath = "Assets/_Project/Art/UI/Fonts/NotoSerifSC-Regular_SDF.asset";
    private const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
    private const string FallbackFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    [MenuItem("Dujiangyan/Create Noto Serif SC SDF")]
    public static void CreateNotoSerifSC()
    {
        // Ensure existing asset is removed so we can recreate it cleanly.
        AssetDatabase.DeleteAsset(SdfOutputPath);
        AssetDatabase.Refresh();

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (sourceFont == null)
        {
            Debug.LogError($"[CreateSDFFont] Could not load source font at {FontPath}");
            return;
        }

        var fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            samplingPointSize: 90,
            atlasPadding: 9,
            renderMode: GlyphRenderMode.SDFAA,
            atlasWidth: 1024,
            atlasHeight: 1024,
            atlasPopulationMode: AtlasPopulationMode.Dynamic,
            enableMultiAtlasSupport: true);

        if (fontAsset == null)
        {
            Debug.LogError("[CreateSDFFont] TMP_FontAsset.CreateFontAsset returned null.");
            return;
        }

        // Persist the font asset first, then add the material as a sub-asset.
        AssetDatabase.CreateAsset(fontAsset, SdfOutputPath);

        // Create an empty atlas texture so the material has a valid _MainTex reference.
        // TMP will populate it dynamically when characters are first requested.
        Texture2D atlasTexture = new Texture2D(1024, 1024, TextureFormat.Alpha8, false);
        atlasTexture.name = $"{fontAsset.name} Atlas";
        atlasTexture.filterMode = FilterMode.Bilinear;
        atlasTexture.wrapMode = TextureWrapMode.Clamp;
        AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);

        if (fontAsset.atlasTextures == null || fontAsset.atlasTextures.Length == 0)
            fontAsset.atlasTextures = new Texture2D[] { atlasTexture };
        else
            fontAsset.atlasTextures[0] = atlasTexture;

        Shader sdfShader = Shader.Find("TextMeshPro/Distance Field");
        if (sdfShader != null)
        {
            Material material = new Material(sdfShader);
            material.SetTexture(ShaderUtilities.ID_MainTex, atlasTexture);
            material.SetFloat(ShaderUtilities.ID_TextureWidth, fontAsset.atlasWidth);
            material.SetFloat(ShaderUtilities.ID_TextureHeight, fontAsset.atlasHeight);
            material.SetFloat(ShaderUtilities.ID_GradientScale, fontAsset.atlasPadding);
            material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
            material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
            material.name = $"{fontAsset.name} Material";

            AssetDatabase.AddObjectToAsset(material, fontAsset);
            fontAsset.material = material;
            EditorUtility.SetDirty(fontAsset);
        }
        else
        {
            Debug.LogWarning("[CreateSDFFont] TextMeshPro/Distance Field shader not found. Material will be created at runtime.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CreateSDFFont] Created SDF font asset at {SdfOutputPath}");
    }

    [MenuItem("Dujiangyan/Set Noto Serif SC as Default TMP Font")]
    public static void SetNotoSerifSCAsDefault()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SdfOutputPath);
        if (fontAsset == null)
        {
            Debug.LogError($"[CreateSDFFont] SDF font asset not found at {SdfOutputPath}. Run Create Noto Serif SC SDF first.");
            return;
        }

        var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath);
        if (settings == null)
        {
            Debug.LogError($"[CreateSDFFont] TMP Settings not found at {TmpSettingsPath}. Import TMP Essential Resources first.");
            return;
        }

        var fallbackAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackFontPath);

        SerializedObject so = new SerializedObject(settings);
        so.Update();
        so.FindProperty("m_defaultFontAsset").objectReferenceValue = fontAsset;
        so.FindProperty("m_defaultFontAssetPath").stringValue = "Fonts & Materials/";
        so.FindProperty("m_defaultFontSize").floatValue = 36f;

        var fallbackProp = so.FindProperty("m_fallbackFontAssets");
        fallbackProp.ClearArray();
        if (fallbackAsset != null)
        {
            fallbackProp.arraySize = 1;
            fallbackProp.GetArrayElementAtIndex(0).objectReferenceValue = fallbackAsset;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log("[CreateSDFFont] Noto Serif SC SDF is now the default TMP font.");
    }

    [MenuItem("Dujiangyan/Validate Font Setup")]
    public static void ValidateSetup()
    {
        bool ok = true;
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SdfOutputPath);
        if (fontAsset == null)
        {
            Debug.LogError($"[ValidateFontSetup] SDF font asset missing at {SdfOutputPath}.");
            ok = false;
        }
        else
        {
            if (fontAsset.material == null)
            {
                Debug.LogError("[ValidateFontSetup] SDF font asset has no material.");
                ok = false;
            }
            if (fontAsset.atlasTexture == null)
            {
                Debug.LogError("[ValidateFontSetup] SDF font asset has no atlas texture.");
                ok = false;
            }
        }

        if (TMP_Settings.instance == null)
        {
            Debug.LogError("[ValidateFontSetup] TMP_Settings instance is null.");
            ok = false;
        }
        else if (TMP_Settings.defaultFontAsset != fontAsset)
        {
            Debug.LogError("[ValidateFontSetup] TMP default font asset is not Noto Serif SC SDF.");
            ok = false;
        }

        if (ok)
        {
            Debug.Log("[ValidateFontSetup] Font setup is valid.");
            EditorApplication.Exit(0);
        }
        else
        {
            EditorApplication.Exit(1);
        }
    }
}
