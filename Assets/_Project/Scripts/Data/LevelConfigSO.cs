using UnityEngine;

namespace Dujiangyan.Data
{
    /// <summary>
    /// 关卡只读配置资产
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig_L1", menuName = "Dujiangyan/LevelConfig")]
    public class LevelConfigSO : ScriptableObject
    {
        [Header("基础信息")]
        public string id = "L1";                 // 关卡唯一标识
        public int act = 1;                      // 幕号：1|2|3
        public string title = "堵";              // 关卡名称

        [Header("目标与资源")]
        public float targetDuration = 30f;       // 目标时长（秒）
        public ResourceCost resourceLimit;       // 料/工/时上限
        public ResourceCost frugalThreshold;     // 节俭通关阈值

        [Header("场景元素")]
        public TerrainConfig terrain;            // 地形配置
        public WaterSourceConfig waterSource;    // 水源参数
        public VillageConfig village;            // 村庄位置与存活条件

        [Header("预置构件")]
        public PreplacedBlock[] preplacedBlocks; // 关卡初始已放置的构件（如官方石墙）

        [Header("构件库存")]
        public BlockInventory inventory;         // 初始构件库存

        [Header("提示与叙事")]
        public HintNode[] hintTree;              // 提示树节点
        public NarrativeBeat[] narrative;        // 叙事节拍

        [Header("碑廊解锁")]
        public string[] galleryUnlocks;          // 通关必解锁碑廊条目
        public string[] hiddenGalleryUnlocks;    // 节俭解锁碑廊条目
    }
}
