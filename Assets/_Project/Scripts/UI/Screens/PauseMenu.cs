using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dujiangyan.Systems;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 暂停菜单：继续、重试、设置音量、返回标题
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button titleButton;

        public bool IsVisible => canvasGroup.alpha > 0.1f;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (resumeButton != null)
                resumeButton.onClick.AddListener(Hide);
            if (retryButton != null)
                retryButton.onClick.AddListener(() =>
                {
                    Hide();
                    PuzzleSystem.Instance?.Reset();
                });
            if (titleButton != null)
                titleButton.onClick.AddListener(() => Debug.Log("[PauseMenu] Return to title (placeholder)."));

            if (musicSlider != null)
                musicSlider.onValueChanged.AddListener(v => AudioSystem.Instance?.SetMusicVolume(v));
            if (sfxSlider != null)
                sfxSlider.onValueChanged.AddListener(v => AudioSystem.Instance?.SetSFXVolume(v));
        }

        private void Start()
        {
            if (musicSlider != null && AudioSystem.Instance != null)
                musicSlider.value = AudioSystem.Instance.MusicVolume;
            if (sfxSlider != null && AudioSystem.Instance != null)
                sfxSlider.value = AudioSystem.Instance.SFXVolume;
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            PuzzleSystem.Instance?.PauseSimulation();
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            PuzzleSystem.Instance?.ResumeSimulation();
        }
    }
}
