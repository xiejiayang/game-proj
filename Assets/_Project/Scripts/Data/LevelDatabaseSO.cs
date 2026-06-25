using System.Collections.Generic;
using UnityEngine;

namespace Dujiangyan.Data
{
    /// <summary>
    /// 关卡数据库，用于运行时按 ID 查找 LevelConfigSO
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Dujiangyan/LevelDatabase")]
    public class LevelDatabaseSO : ScriptableObject
    {
        public List<LevelConfigSO> levels = new List<LevelConfigSO>();

        public LevelConfigSO GetLevel(string id)
        {
            if (string.IsNullOrEmpty(id) || levels == null)
                return null;

            foreach (var level in levels)
            {
                if (level != null && level.id == id)
                    return level;
            }
            return null;
        }
    }
}
