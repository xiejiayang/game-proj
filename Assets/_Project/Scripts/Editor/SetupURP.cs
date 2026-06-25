using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class SetupURP
{
    public static void CreateURPAsset()
    {
        const string projectFolder = "Assets/_Project";
        const string settingsFolder = projectFolder + "/Settings";
        const string assetPath = settingsFolder + "/URP_L1.asset";

        if (!AssetDatabase.IsValidFolder(projectFolder))
        {
            AssetDatabase.CreateFolder("Assets", "_Project");
        }
        if (!AssetDatabase.IsValidFolder(settingsFolder))
        {
            AssetDatabase.CreateFolder(projectFolder, "Settings");
        }

        var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(assetPath);
        if (asset == null)
        {
            asset = UniversalRenderPipelineAsset.Create();
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssetIfDirty(asset);
        }

        GraphicsSettings.defaultRenderPipeline = asset;
        QualitySettings.renderPipeline = asset;
        AssetDatabase.SaveAssets();
    }
}
