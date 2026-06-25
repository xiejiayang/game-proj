using UnityEngine;
using TMPro;
using Dujiangyan.Data;
using Dujiangyan.Systems;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 实时资源消耗 HUD：料 / 工 / 时
    /// </summary>
    public class ResourceHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI materialLabel;
        [SerializeField] private TextMeshProUGUI laborLabel;
        [SerializeField] private TextMeshProUGUI timeLabel;

        private void Start()
        {
            if (PuzzleSystem.Instance?.Runtime != null)
            {
                UpdateDisplay(PuzzleSystem.Instance.Runtime.consumedResource);
            }
        }

        private void Update()
        {
            if (PuzzleSystem.Instance?.Runtime != null)
                UpdateDisplay(PuzzleSystem.Instance.Runtime.consumedResource);
        }

        private void UpdateDisplay(ResourceCost cost)
        {
            if (materialLabel != null) materialLabel.text = $"料 {cost.material}";
            if (laborLabel != null) laborLabel.text = $"工 {cost.labor}";
            if (timeLabel != null) timeLabel.text = $"时 {cost.time}";
        }
    }
}
