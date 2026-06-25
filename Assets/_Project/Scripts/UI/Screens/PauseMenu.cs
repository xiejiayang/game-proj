using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dujiangyan.Systems;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 暂停菜单：继续、重试、设置音量、返回标题，带淡入/按钮交错动画
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button titleButton;

        public bool IsVisible => canvasGroup != null && canvasGroup.alpha > 0.1f;

        private Button[] animatedButtons;
        private Vector2[] buttonOriginalPositions;
        private Coroutine currentAnim;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            CollectButtons();

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
            HideInstant();
        }

        private void CollectButtons()
        {
            animatedButtons = GetComponentsInChildren<Button>();
            buttonOriginalPositions = new Vector2[animatedButtons.Length];
            for (int i = 0; i < animatedButtons.Length; i++)
            {
                var rect = animatedButtons[i].GetComponent<RectTransform>();
                if (rect != null)
                    buttonOriginalPositions[i] = rect.anchoredPosition;
            }
        }

        public void Show()
        {
            if (currentAnim != null)
                StopCoroutine(currentAnim);

            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            ResetButtonPositions();

            PuzzleSystem.Instance?.PauseSimulation();
            currentAnim = StartCoroutine(ShowSequence());
        }

        public void Hide()
        {
            if (currentAnim != null)
                StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(HideSequence());
        }

        private void HideInstant()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        private void ResetButtonPositions()
        {
            for (int i = 0; i < animatedButtons.Length; i++)
            {
                if (animatedButtons[i] == null) continue;
                var rect = animatedButtons[i].GetComponent<RectTransform>();
                if (rect != null)
                    rect.anchoredPosition = buttonOriginalPositions[i] + Vector2.down * 50f;
            }
        }

        private IEnumerator ShowSequence()
        {
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / 0.2f);
                yield return null;
            }
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            for (int i = 0; i < animatedButtons.Length; i++)
            {
                if (animatedButtons[i] != null)
                {
                    var rect = animatedButtons[i].GetComponent<RectTransform>();
                    if (rect != null)
                        StartCoroutine(AnimateButton(rect, buttonOriginalPositions[i], 0.15f, i * 0.05f));
                }
            }
            currentAnim = null;
        }

        private IEnumerator HideSequence()
        {
            float t = 0f;
            float startAlpha = canvasGroup.alpha;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(t / 0.2f));
                yield return null;
            }
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            PuzzleSystem.Instance?.ResumeSimulation();
            currentAnim = null;
        }

        private IEnumerator AnimateButton(RectTransform rect, Vector2 target, float duration, float delay)
        {
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            Vector2 start = rect.anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                float ease = 1f - Mathf.Pow(1f - p, 3f);
                rect.anchoredPosition = Vector2.Lerp(start, target, ease);
                yield return null;
            }
            rect.anchoredPosition = target;
        }
    }
}
