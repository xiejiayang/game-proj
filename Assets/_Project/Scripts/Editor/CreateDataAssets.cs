using UnityEditor;
using UnityEngine;
using Dujiangyan.Data;

public class CreateDataAssets
{
    [MenuItem("Dujiangyan/Setup L1 Data Assets")]
    public static void SetupL1DataAssets()
    {
        var blocks = CreateBlockConfigs();
        LevelConfigSO level = CreateLevelConfigL1();
        CreateBlockDatabase(blocks);
        CreateLevelDatabase(level);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateDataAssets] L1 data assets created.");
    }

    private static BlockConfigSO[] CreateBlockConfigs()
    {
        string dir = "Assets/_Project/ScriptableObjects/Blocks";
        EnsureDirectory(dir);

        var bambooPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Blocks/Block_Bamboo.prefab");
        var maochaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Blocks/Block_Maocha.prefab");
        var wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Blocks/Block_Wall.prefab");

        var bamboo = CreateBlockConfig(
            path: $"{dir}/BlockConfig_Bamboo.asset",
            id: "bamboo",
            displayName: "竹笼",
            type: BlockType.Bamboo,
            maxHealth: 30f,
            cost: new ResourceCost { material = 2, labor = 1, time = 0 },
            interaction: WaterInteraction.Split,
            prefab: bambooPrefab);

        var maocha = CreateBlockConfig(
            path: $"{dir}/BlockConfig_Maocha.asset",
            id: "maocha",
            displayName: "杩槎",
            type: BlockType.Maocha,
            maxHealth: 60f,
            cost: new ResourceCost { material = 4, labor = 2, time = 0 },
            interaction: WaterInteraction.Split,
            prefab: maochaPrefab);

        var wall = CreateBlockConfig(
            path: $"{dir}/BlockConfig_Wall.asset",
            id: "wall",
            displayName: "石墙",
            type: BlockType.Wall,
            maxHealth: 200f,
            cost: new ResourceCost { material = 10, labor = 5, time = 0 },
            interaction: WaterInteraction.Bounce,
            prefab: wallPrefab);

        return new[] { bamboo, maocha, wall };
    }

    private static BlockConfigSO CreateBlockConfig(string path, string id, string displayName, BlockType type, float maxHealth, ResourceCost cost, WaterInteraction interaction, GameObject prefab)
    {
        AssetDatabase.DeleteAsset(path);
        var asset = ScriptableObject.CreateInstance<BlockConfigSO>();
        asset.id = id;
        asset.displayName = displayName;
        asset.type = type;
        asset.maxHealth = maxHealth;
        asset.cost = cost;
        asset.interaction = interaction;
        asset.prefab = prefab;
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void CreateBlockDatabase(BlockConfigSO[] blocks)
    {
        string resourcesDir = "Assets/_Project/Resources";
        EnsureDirectory(resourcesDir);
        string path = $"{resourcesDir}/BlockDatabase.asset";
        AssetDatabase.DeleteAsset(path);

        var asset = ScriptableObject.CreateInstance<BlockDatabaseSO>();
        asset.blocks = new System.Collections.Generic.List<BlockConfigSO>(blocks);
        AssetDatabase.CreateAsset(asset, path);
    }

    private static LevelConfigSO CreateLevelConfigL1()
    {
        string dir = "Assets/_Project/ScriptableObjects/Levels";
        EnsureDirectory(dir);
        string path = $"{dir}/LevelConfig_L1.asset";
        AssetDatabase.DeleteAsset(path);

        var asset = ScriptableObject.CreateInstance<LevelConfigSO>();
        asset.id = "L1";
        asset.act = 1;
        asset.title = "堵";
        asset.targetDuration = 30f;

        asset.resourceLimit = new ResourceCost { material = 50, labor = 30, time = 60 };
        asset.frugalThreshold = new ResourceCost { material = 20, labor = 10, time = 30 };

        asset.terrain = new TerrainConfig
        {
            width = 16,
            depth = 12,
            cellSize = 1f,
            origin = Vector3.zero,
            terrainPrefab = null
        };

        asset.waterSource = new WaterSourceConfig
        {
            position = new Vector3(-6f, 0.5f, 2f),
            emitDirection = Vector3.right,
            emissionRate = 20f,
            particleSpeed = 4f,
            particleLifetime = 6f
        };

        asset.village = new VillageConfig
        {
            center = new Vector3(4f, 0f, 2f),
            size = new Vector2(3f, 3f),
            floodThreshold = 20
        };

        asset.inventory = new BlockInventory
        {
            bamboo = 8,
            maocha = 4,
            wall = 0
        };

        asset.preplacedBlocks = new[]
        {
            new PreplacedBlock
            {
                blockId = "wall",
                position = new Vector3(0f, 0f, 2f),
                rotStep = 1,
                isIndestructible = true
            }
        };

        asset.hintTree = new[]
        {
            new HintNode
            {
                id = "hint_l1_1",
                speaker = "老河工",
                text = "水势太猛，单靠硬堵怕是不行……先看看村里还能撑多久。",
                triggerAfterFails = 3
            },
            new HintNode
            {
                id = "hint_l1_2",
                speaker = "老河工",
                text = "竹笼、杩槎不是挡水，是分水。让水从两边走，村子才保得住。",
                triggerAfterFails = 5
            }
        };

        asset.narrative = new[]
        {
            new NarrativeBeat
            {
                id = "nar_l1_intro",
                text = "官方筑墙失败，村庄危急。李冰留下的治水智慧，或许藏在“堵不如疏”四个字里。",
                illustration = null,
                skippable = true
            },
            new NarrativeBeat
            {
                id = "nar_l1_success",
                text = "水流被引向两侧，村子暂时安全。但这只是第一关。",
                illustration = null,
                skippable = true
            }
        };

        asset.galleryUnlocks = new[] { "gallery_l1_wall", "gallery_l1_flood" };
        asset.hiddenGalleryUnlocks = new[] { "gallery_l1_frugal" };

        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void CreateLevelDatabase(LevelConfigSO level)
    {
        string resourcesDir = "Assets/_Project/Resources";
        EnsureDirectory(resourcesDir);
        string path = $"{resourcesDir}/LevelDatabase.asset";
        AssetDatabase.DeleteAsset(path);

        var asset = ScriptableObject.CreateInstance<LevelDatabaseSO>();
        asset.levels = new System.Collections.Generic.List<LevelConfigSO> { level };
        AssetDatabase.CreateAsset(asset, path);
    }


    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string name = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
