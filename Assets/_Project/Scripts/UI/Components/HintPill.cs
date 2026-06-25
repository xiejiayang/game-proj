using System.Collections;
using UnityEngine;
using TMPro;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 顶部提示胶囊，2.5s 后自动淡出
    /// </summary>
    public class HintPill : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.3f;

        private Coroutine currentFade;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }

        public void Show(string text, float duration)
        {
            if (label != null)
                label.text = text;

            if (currentFade != null)
                StopCoroutine(currentFade);
            currentFade = StartCoroutine(FadeSequence(duration));
        }

        private IEnumerator FadeSequence(float duration)
        {
            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(duration);

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = 1f - t / fadeDuration;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
    }
}
