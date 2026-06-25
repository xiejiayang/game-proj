using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dujiangyan.Data;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 老河工提示对话框：底部滑入 + 打字机效果 + 5s 自动关闭
    /// </summary>
    public class HintDialog : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI speakerLabel;
        [SerializeField] private TextMeshProUGUI contentLabel;
        [SerializeField] private Button closeButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float typeSpeed = 0.04f;
        [SerializeField] private float autoCloseDelay = 5f;

        private RectTransform rectTransform;
        private Vector2 targetAnchoredPos;
        private Coroutine currentRoutine;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            targetAnchoredPos = rectTransform.anchoredPosition;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            rectTransform.anchoredPosition = targetAnchoredPos + Vector2.down * 200f;
        }

        public void Show(HintNode node)
        {
            gameObject.SetActive(true);

            if (currentRoutine != null)
                StopCoroutine(currentRoutine);

            if (speakerLabel != null)
                speakerLabel.text = node.speaker;

            currentRoutine = StartCoroutine(ShowSequence(node.text));
        }

        public void Hide()
        {
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(HideSequence());
        }

        private IEnumerator ShowSequence(string text)
        {
            canvasGroup.blocksRaycasts = true;
            contentLabel.text = "";

            float t = 0f;
            Vector2 startPos = targetAnchoredPos + Vector2.down * 200f;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / 0.4f);
                float ease = 1f - Mathf.Pow(1f - p, 3f); // ease-out cubic
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetAnchoredPos, ease);
                canvasGroup.alpha = p;
                yield return null;
            }
            rectTransform.anchoredPosition = targetAnchoredPos;
            canvasGroup.alpha = 1f;

            yield return StartCoroutine(TypeText(text));
        }

        private IEnumerator HideSequence()
        {
            float t = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = targetAnchoredPos + Vector2.down * 120f;
            float startAlpha = canvasGroup.alpha;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / 0.25f);
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, p);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, p);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            rectTransform.anchoredPosition = targetAnchoredPos + Vector2.down * 200f;
            currentRoutine = null;
        }

        private IEnumerator TypeText(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                contentLabel.text += text[i];
                yield return new WaitForSeconds(typeSpeed);
            }
            yield return new WaitForSeconds(autoCloseDelay);
            Hide();
        }
    }
}
