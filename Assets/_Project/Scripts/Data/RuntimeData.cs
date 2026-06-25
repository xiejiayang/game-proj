using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dujiangyan.Data
{
    /// <summary>
    /// 构件类型：竹笼 / 杩槎 / 石墙
    /// </summary>
    public enum BlockType
    {
        Bamboo,
        Maocha,
        Wall
    }

    /// <summary>
    /// 构件与水流交互方式
    /// </summary>
    public enum WaterInteraction
    {
        /// <summary>反弹并造成耐久伤害（石墙）</summary>
        Bounce,
        /// <summary>按法线方向分流（竹笼 / 杩槎）</summary>
        Split
    }

    /// <summary>
    /// 解谜状态机
    /// </summary>
    public enum PuzzleState
    {
        Editing,
        Simulating,
        Settling,
        Paused
    }

    /// <summary>
    /// 失败原因
    /// </summary>
    public enum FailReason
    {
        None,
        Flood,
        Destroyed,
        Timeout
    }

    /// <summary>
    /// 编辑操作类型，用于撤销栈
    /// </summary>
    public enum EditActionType
    {
        Place,
        Remove,
        Rotate
    }

    /// <summary>
    /// 预置方块配置（如 L1 官方石墙）
    /// </summary>
    [Serializable]
    public struct PreplacedBlock
    {
        public string blockId;
        public Vector3 position;
        public int rotStep;
        public bool isIndestructible;
    }

    /// <summary>
    /// 资源消耗：料 / 工 / 时
    /// </summary>
    [Serializable]
    public struct ResourceCost
    {
        public int material;
        public int labor;
        public int time;

        public static ResourceCost Zero => new ResourceCost { material = 0, labor = 0, time = 0 };

        public static ResourceCost operator +(ResourceCost a, ResourceCost b)
        {
            return new ResourceCost
            {
                material = a.material + b.material,
                labor = a.labor + b.labor,
                time = a.time + b.time
            };
        }

        public static ResourceCost operator -(ResourceCost a, ResourceCost b)
        {
            return new ResourceCost
            {
                material = a.material - b.material,
                labor = a.labor - b.labor,
                time = a.time - b.time
            };
        }

        public bool IsLessOrEqual(ResourceCost other)
        {
            return material <= other.material && labor <= other.labor && time <= other.time;
        }
    }

    /// <summary>
    /// 构件库存
    /// </summary>
    [Serializable]
    public struct BlockInventory
    {
        public int bamboo;
        public int maocha;
        public int wall;

        public int GetCount(BlockType type)
        {
            return type switch
            {
                BlockType.Bamboo => bamboo,
                BlockType.Maocha => maocha,
                BlockType.Wall => wall,
                _ => 0
            };
        }

        public void SetCount(BlockType type, int value)
        {
            switch (type)
            {
                case BlockType.Bamboo: bamboo = value; break;
                case BlockType.Maocha: maocha = value; break;
                case BlockType.Wall: wall = value; break;
            }
        }

        public BlockInventory Clone()
        {
            return new BlockInventory { bamboo = bamboo, maocha = maocha, wall = wall };
        }
    }

    /// <summary>
    /// 地形网格配置
    /// </summary>
    [Serializable]
    public struct TerrainConfig
    {
        public int width;
        public int depth;
        public float cellSize;
        public Vector3 origin;
        public GameObject terrainPrefab;
    }

    /// <summary>
    /// 水源发射器配置
    /// </summary>
    [Serializable]
    public struct WaterSourceConfig
    {
        public Vector3 position;
        public Vector3 emitDirection;
        public float emissionRate;
        public float particleSpeed;
        public float particleLifetime;
    }

    /// <summary>
    /// 村庄区域与存活条件
    /// </summary>
    [Serializable]
    public struct VillageConfig
    {
        public Vector3 center;
        public Vector2 size;
        public int floodThreshold;
    }

    /// <summary>
    /// 提示树节点
    /// </summary>
    [Serializable]
    public struct HintNode
    {
        public string id;
        public string speaker;
        [TextArea(2, 5)]
        public string text;
        /// <summary>累计失败多少次后触发该提示（L1 仅开放前两层）</summary>
        public int triggerAfterFails;
    }

    /// <summary>
    /// 叙事节拍
    /// </summary>
    [Serializable]
    public struct NarrativeBeat
    {
        public string id;
        [TextArea(2, 5)]
        public string text;
        public Sprite illustration;
        public bool skippable;
    }

    /// <summary>
    /// 玩家设置
    /// </summary>
    [Serializable]
    public struct GameSettings
    {
        [Range(0f, 1f)]
        public float musicVolume;
        [Range(0f, 1f)]
        public float sfxVolume;
        public int languageIndex;
    }

    /// <summary>
    /// 一次编辑操作，用于撤销栈
    /// </summary>
    [Serializable]
    public struct EditAction
    {
        public EditActionType actionType;
        public string blockId;
        public string instanceId;
        public Vector3 position;
        public int rotStep;
    }

    /// <summary>
    /// 放置/移动/旋转/移除操作的通用结果
    /// </summary>
    [Serializable]
    public struct OperationResult
    {
        public bool success;
        public string errorMessage;
    }

    /// <summary>
    /// 开始模拟的结果
    /// </summary>
    [Serializable]
    public struct SimulationResult
    {
        public bool success;
        public string errorMessage;
    }

    /// <summary>
    /// 运行时构件实例
    /// </summary>
    [Serializable]
    public class BlockInstance
    {
        public string instanceId;
        public string blockId;
        public Vector3 position;
        public int rotStep;
        public float health;
        public float maxHealth;
        public WaterInteraction interaction;
        public bool isIndestructible;
    }

    /// <summary>
    /// 运行时关卡数据（每局独立）
    /// </summary>
    [Serializable]
    public class PuzzleRuntime
    {
        public string levelId;
        public PuzzleState state;
        public List<BlockInstance> placedBlocks;
        public BlockInventory inventory;
        public ResourceCost consumedResource;
        public List<EditAction> undoStack;
        public float simulationTime;
        public int villageHitCount;
        public PuzzleResult result;

        public PuzzleRuntime()
        {
            placedBlocks = new List<BlockInstance>();
            inventory = new BlockInventory();
            consumedResource = ResourceCost.Zero;
            undoStack = new List<EditAction>();
            state = PuzzleState.Editing;
            result = new PuzzleResult();
        }
    }

    /// <summary>
    /// 结算结果
    /// </summary>
    [Serializable]
    public class PuzzleResult
    {
        public bool isSuccess;
        public bool isFrugal;
        public FailReason failReason;
        public ResourceCost consumedResource;
        public float simulationTime;
        public List<string> unlockedGallery;

        public PuzzleResult()
        {
            unlockedGallery = new List<string>();
        }
    }

    /// <summary>
    /// 玩家档案（持久化）
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        public string lastPlayedLevelId;
        public List<string> unlockedLevels;
        public List<string> unlockedGallery;
        public GameSettings settings;

        public PlayerProfile()
        {
            unlockedLevels = new List<string>();
            unlockedGallery = new List<string>();
            settings = new GameSettings
            {
                musicVolume = 0.7f,
                sfxVolume = 0.7f,
                languageIndex = 0
            };
        }
    }
}
