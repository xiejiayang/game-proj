using UnityEngine;
using Dujiangyan.Data;
using Dujiangyan.UI;

namespace Dujiangyan.Systems
{
    /// <summary>
    /// 老河工提示触发器：根据失败次数触发对应提示节点
    /// </summary>
    public class HintSystem : MonoBehaviour
    {
        [SerializeField] private LevelUI levelUI;

        private void OnEnable()
        {
            if (PuzzleSystem.Instance != null)
                PuzzleSystem.Instance.OnLevelSettled += OnLevelSettled;
        }

        private void OnDisable()
        {
            if (PuzzleSystem.Instance != null)
                PuzzleSystem.Instance.OnLevelSettled -= OnLevelSettled;
        }

        private void OnLevelSettled(PuzzleResult result)
        {
            if (result.isSuccess) return;
            if (PuzzleSystem.Instance?.CurrentConfig == null) return;
            if (SaveSystem.Instance == null) return;

            var profile = SaveSystem.Instance.LoadProfile();
            string levelId = PuzzleSystem.Instance.CurrentConfig.id;
            int failCount = profile.GetFailureCount(levelId);

            foreach (var node in PuzzleSystem.Instance.CurrentConfig.hintTree)
            {
                if (node.triggerAfterFails == failCount)
                {
                    levelUI?.ShowHint(node);
                    break;
                }
            }
        }
    }
}
