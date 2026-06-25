using UnityEngine;
using Dujiangyan.Systems;

namespace Dujiangyan.UI
{
    /// <summary>
    /// L1 灰盒场景 UI 交互：工具栏选择、旋转、放水、重置
    /// </summary>
    public class LevelUI : MonoBehaviour
    {
        public void SelectBamboo()
        {
            BlockPlacement.Instance?.SelectBlock("bamboo");
        }

        public void SelectMaocha()
        {
            BlockPlacement.Instance?.SelectBlock("maocha");
        }

        public void RotatePending()
        {
            BlockPlacement.Instance?.RotatePending();
        }

        public void StartSimulation()
        {
            PuzzleSystem.Instance?.StartSimulation();
        }

        public void ResetLevel()
        {
            PuzzleSystem.Instance?.Reset();
        }
    }
}
