using System.Linq;
using UnityEditor;
using UnityEngine;
using Dujiangyan.Data;

public class ValidateDataAssets
{
    [MenuItem("Dujiangyan/Validate L1 Data Assets")]
    public static void Validate()
    {
        bool ok = true;

        var level = AssetDatabase.LoadAssetAtPath<LevelConfigSO>("Assets/_Project/ScriptableObjects/Levels/LevelConfig_L1.asset");
        if (level == null)
        {
            Debug.LogError("[ValidateDataAssets] LevelConfig_L1.asset missing.");
            ok = false;
        }
        else
        {
            if (string.IsNullOrEmpty(level.id) || string.IsNullOrEmpty(level.title))
            {
                Debug.LogError("[ValidateDataAssets] LevelConfig_L1 id/title empty.");
                ok = false;
            }
            if (level.hintTree == null || level.hintTree.Length < 2)
            {
                Debug.LogError("[ValidateDataAssets] LevelConfig_L1 hintTree should have at least 2 nodes.");
                ok = false;
            }
            if (level.hintTree.Any(h => h.triggerAfterFails <= 0))
            {
                Debug.LogError("[ValidateDataAssets] LevelConfig_L1 hint node triggerAfterFails must be > 0.");
                ok = false;
            }
            if (level.inventory.bamboo <= 0 && level.inventory.maocha <= 0 && level.inventory.wall <= 0)
            {
                Debug.LogError("[ValidateDataAssets] LevelConfig_L1 inventory is empty.");
                ok = false;
            }
            if (!level.frugalThreshold.IsLessOrEqual(level.resourceLimit))
            {
                Debug.LogError("[ValidateDataAssets] LevelConfig_L1 frugalThreshold exceeds resourceLimit.");
                ok = false;
            }
        }

        var bamboo = LoadBlock("BlockConfig_Bamboo");
        var maocha = LoadBlock("BlockConfig_Maocha");
        var wall = LoadBlock("BlockConfig_Wall");

        if (bamboo == null || maocha == null || wall == null)
        {
            Debug.LogError("[ValidateDataAssets] One or more BlockConfig assets missing.");
            ok = false;
        }
        else
        {
            if (!(bamboo.maxHealth < maocha.maxHealth && maocha.maxHealth < wall.maxHealth))
            {
                Debug.LogError($"[ValidateDataAssets] Durability order violated: bamboo={bamboo.maxHealth}, maocha={maocha.maxHealth}, wall={wall.maxHealth}.");
                ok = false;
            }
            if (bamboo.type != BlockType.Bamboo || maocha.type != BlockType.Maocha || wall.type != BlockType.Wall)
            {
                Debug.LogError("[ValidateDataAssets] BlockConfig types mismatch.");
                ok = false;
            }
        }

        if (ok)
        {
            Debug.Log("[ValidateDataAssets] L1 data assets are valid.");
            EditorApplication.Exit(0);
        }
        else
        {
            EditorApplication.Exit(1);
        }
    }

    private static BlockConfigSO LoadBlock(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<BlockConfigSO>($"Assets/_Project/ScriptableObjects/Blocks/{fileName}.asset");
    }
}
