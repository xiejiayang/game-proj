using UnityEngine;
using Dujiangyan.Data;
using Dujiangyan.Systems;

namespace Dujiangyan.UI
{
    /// <summary>
    /// L1 场景 UI 交互入口
    /// </summary>
    public class LevelUI : MonoBehaviour
    {
        [SerializeField] private HintPill hintPill;
        [SerializeField] private HintDialog hintDialog;
        [SerializeField] private ResultModal resultModal;
        [SerializeField] private PauseMenu pauseMenu;

        public void SelectBamboo() => BlockPlacement.Instance?.SelectBlock("bamboo");
        public void SelectMaocha() => BlockPlacement.Instance?.SelectBlock("maocha");
        public void RotatePending() => BlockPlacement.Instance?.RotatePending();

        public void StartSimulation()
        {
            var puzzle = PuzzleSystem.Instance;
            if (puzzle == null) return;

            // 空操作提示
            if (puzzle.Runtime != null && puzzle.Runtime.placedBlocks.Count <= puzzle.PreplacedBlockCount)
            {
                hintPill?.Show("先在河里摆上构件", 2.5f);
                return;
            }

            puzzle.StartSimulation();
        }

        public void Undo()
        {
            bool undone = PuzzleSystem.Instance?.Undo() ?? false;
            if (!undone)
                hintPill?.Show("没有可撤销的操作", 2f);
        }

        private void OnEnable()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.OnSaveFailed += OnSaveFailed;
        }

        private void OnDisable()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.OnSaveFailed -= OnSaveFailed;
        }

        private void OnSaveFailed(string message)
        {
            hintPill?.Show(message, 4f);
        }

        public void ResetLevel()
        {
            PuzzleSystem.Instance?.Reset();
            resultModal?.Hide();
        }

        public void TogglePause()
        {
            if (pauseMenu == null) return;
            if (pauseMenu.IsVisible)
                pauseMenu.Hide();
            else
                pauseMenu.Show();
        }

        public void ShowResult(PuzzleResult result)
        {
            resultModal?.Show(result);
        }

        public void ShowHint(HintNode node)
        {
            hintDialog?.Show(node);
        }

        public void OnRetry()
        {
            ResetLevel();
        }

        public void OnShowGallery()
        {
            resultModal?.ShowGallery();
        }
    }
}
