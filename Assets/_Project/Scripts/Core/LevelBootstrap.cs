using UnityEngine;
using Dujiangyan.Systems;
using Dujiangyan.UI;
using TMPro;

namespace Dujiangyan.Core
{
    /// <summary>
    /// L1 灰盒场景启动器：初始化服务、关卡与 UI
    /// </summary>
    public class LevelBootstrap : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI inventoryText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private LevelUI levelUI;

        private void Start()
        {
            EnsureServices();
            PuzzleSystem.Instance.InitLevel("L1");
            PuzzleSystem.Instance.OnInventoryChanged += OnInventoryChanged;
            PuzzleSystem.Instance.OnLevelSettled += OnLevelSettled;
            UpdateInventoryUI(PuzzleSystem.Instance.Runtime.inventory);
            UpdateStatusUI("编辑阶段");
        }

        private void OnDestroy()
        {
            if (PuzzleSystem.Instance != null)
            {
                PuzzleSystem.Instance.OnInventoryChanged -= OnInventoryChanged;
                PuzzleSystem.Instance.OnLevelSettled -= OnLevelSettled;
            }
        }

        private void EnsureServices()
        {
            if (InputSystem.Instance == null) CreateService<InputSystem>("InputSystem");
            if (AudioSystem.Instance == null) CreateService<AudioSystem>("AudioSystem");
            if (SaveSystem.Instance == null) CreateService<SaveSystem>("SaveSystem");
            if (UIManager.Instance == null) CreateService<UIManager>("UIManager");
            if (PuzzleSystem.Instance == null) CreateService<PuzzleSystem>("PuzzleSystem");
            if (WaterSimulation.Instance == null) CreateService<WaterSimulation>("WaterSimulation");
            if (BlockPlacement.Instance == null) CreateService<BlockPlacement>("BlockPlacement");
        }

        private void CreateService<T>(string name) where T : MonoBehaviour
        {
            var go = new GameObject(name);
            go.AddComponent<T>();
        }

        private void OnInventoryChanged(Dujiangyan.Data.BlockInventory inventory)
        {
            UpdateInventoryUI(inventory);
        }

        private void UpdateInventoryUI(Dujiangyan.Data.BlockInventory inventory)
        {
            if (inventoryText != null)
                inventoryText.text = $"竹笼: {inventory.bamboo}  杩槎: {inventory.maocha}  石墙: {inventory.wall}";
        }

        private void UpdateStatusUI(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
        }

        private void OnLevelSettled(Dujiangyan.Data.PuzzleResult result)
        {
            string msg = result.isSuccess
                ? $"成功！{(result.isFrugal ? "节俭" : "非节俭")}"
                : $"失败：{result.failReason}";
            UpdateStatusUI(msg);
            levelUI?.ShowResult(result);
        }
    }
}
