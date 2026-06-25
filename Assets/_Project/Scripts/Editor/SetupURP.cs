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
        const string rendererPath = settingsFolder + "/URP_L1_Renderer.asset";

        if (!AssetDatabase.IsValidFolder(projectFolder))
        {
            AssetDatabase.CreateFolder("Assets", "_Project");
        }
        if (!AssetDatabase.IsValidFolder(settingsFolder))
        {
            AssetDatabase.CreateFolder(projectFolder, "Settings");
        }

        // Remove old pipeline asset so we can recreate with a valid renderer reference.
        var oldAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(assetPath);
        if (oldAsset != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
        }

        // Create the renderer data asset on disk.
        var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
        AssetDatabase.CreateAsset(rendererData, rendererPath);

        // Create the pipeline asset, referencing the saved renderer data.
        var asset = UniversalRenderPipelineAsset.Create(rendererData);
        AssetDatabase.CreateAsset(asset, assetPath);

        AssetDatabase.SaveAssetIfDirty(rendererData);
        AssetDatabase.SaveAssetIfDirty(asset);

        GraphicsSettings.defaultRenderPipeline = asset;
        QualitySettings.renderPipeline = asset;
        AssetDatabase.SaveAssets();
    }
}
