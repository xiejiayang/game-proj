using UnityEngine;

namespace Dujiangyan.Data
{
    /// <summary>
    /// 构件只读配置资产
    /// </summary>
    [CreateAssetMenu(fileName = "BlockConfig_Bamboo", menuName = "Dujiangyan/BlockConfig")]
    public class BlockConfigSO : ScriptableObject
    {
        [Header("基础信息")]
        public string id = "bamboo";             // 构件 ID
        public string displayName = "竹笼";      // 显示名
        public BlockType type = BlockType.Bamboo;// 构件类型

        [Header("耐久与消耗")]
        public float maxHealth = 100f;           // 最大耐久
        public ResourceCost cost;                // 消耗资源

        [Header("水流交互")]
        public WaterInteraction interaction = WaterInteraction.Split;

        [Header("表现")]
        public GameObject prefab;                // 3D 预制体
    }
}
