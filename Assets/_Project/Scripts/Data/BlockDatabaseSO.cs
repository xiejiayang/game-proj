using System.Collections.Generic;
using UnityEngine;

namespace Dujiangyan.Data
{
    /// <summary>
    /// 构件数据库，用于运行时按 ID 查找 BlockConfigSO
    /// </summary>
    [CreateAssetMenu(fileName = "BlockDatabase", menuName = "Dujiangyan/BlockDatabase")]
    public class BlockDatabaseSO : ScriptableObject
    {
        public List<BlockConfigSO> blocks = new List<BlockConfigSO>();

        public BlockConfigSO GetBlock(string id)
        {
            if (string.IsNullOrEmpty(id) || blocks == null)
                return null;

            foreach (var block in blocks)
            {
                if (block != null && block.id == id)
                    return block;
            }
            return null;
        }
    }
}
