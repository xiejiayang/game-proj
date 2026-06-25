using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dujiangyan.Data;
using Dujiangyan.Systems;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 底部 Toolbar：竹笼 / 杩槎 选择项，带库存计数与选中态
    /// </summary>
    public class Toolbar : MonoBehaviour
    {
        [System.Serializable]
        public class ToolbarItem
        {
            public string blockId;
            public Button button;
            public TextMeshProUGUI countLabel;
            public Image background;
        }

        [SerializeField] private ToolbarItem[] items;
        [SerializeField] private Color normalColor = new Color(1, 1, 1, 0.4f);
        [SerializeField] private Color selectedColor = new Color(0.18f, 0.35f, 0.29f, 0.6f);
        [SerializeField] private Color disabledColor = new Color(0.72f, 0.72f, 0.72f, 0.4f);

        private string selectedId;

        private void Start()
        {
            foreach (var item in items)
            {
                var localItem = item;
                item.button.onClick.AddListener(() => Select(localItem.blockId));
            }

            if (PuzzleSystem.Instance?.Runtime != null)
            {
                UpdateInventory(PuzzleSystem.Instance.Runtime.inventory);
                PuzzleSystem.Instance.OnInventoryChanged += UpdateInventory;
                PuzzleSystem.Instance.OnSimulationStarted += OnSimulationStarted;
                PuzzleSystem.Instance.OnEditingStarted += OnEditingStarted;
            }
        }

        private void OnDestroy()
        {
            if (PuzzleSystem.Instance != null)
            {
                PuzzleSystem.Instance.OnInventoryChanged -= UpdateInventory;
                PuzzleSystem.Instance.OnSimulationStarted -= OnSimulationStarted;
                PuzzleSystem.Instance.OnEditingStarted -= OnEditingStarted;
            }
        }

        private void Select(string blockId)
        {
            selectedId = blockId;
            BlockPlacement.Instance?.SelectBlock(blockId);
            UpdateVisuals();
        }

        private void UpdateInventory(BlockInventory inventory)
        {
            foreach (var item in items)
            {
                int count = 0;
                if (item.blockId == "bamboo") count = inventory.bamboo;
                else if (item.blockId == "maocha") count = inventory.maocha;
                else if (item.blockId == "wall") count = inventory.wall;

                if (item.countLabel != null)
                    item.countLabel.text = count.ToString();

                item.button.interactable = count > 0;
            }
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            foreach (var item in items)
            {
                if (item.background == null) continue;
                if (!item.button.interactable)
                    item.background.color = disabledColor;
                else if (item.blockId == selectedId)
                    item.background.color = selectedColor;
                else
                    item.background.color = normalColor;
            }
        }

        private void OnSimulationStarted()
        {
            foreach (var item in items)
                item.button.interactable = false;
            UpdateVisuals();
        }

        private void OnEditingStarted()
        {
            if (PuzzleSystem.Instance?.Runtime != null)
                UpdateInventory(PuzzleSystem.Instance.Runtime.inventory);
            selectedId = null;
            UpdateVisuals();
        }
    }
}
