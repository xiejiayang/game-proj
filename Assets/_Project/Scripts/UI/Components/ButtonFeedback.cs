using UnityEngine;
using UnityEngine.EventSystems;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 按钮点击反馈：100ms 缩放至 0.97
    /// </summary>
    public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float pressScale = 0.97f;
        [SerializeField] private float pressDuration = 0.1f;

        private Vector3 originalScale;
        private Coroutine currentAnim;

        private void Awake()
        {
            originalScale = transform.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (currentAnim != null)
                StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(AnimateScale(originalScale * pressScale, pressDuration));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (currentAnim != null)
                StopCoroutine(currentAnim);
            currentAnim = StartCoroutine(AnimateScale(originalScale, pressDuration));
        }

        private System.Collections.IEnumerator AnimateScale(Vector3 target, float duration)
        {
            Vector3 start = transform.localScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / duration);
                transform.localScale = Vector3.Lerp(start, target, p);
                yield return null;
            }
            transform.localScale = target;
            currentAnim = null;
        }
    }
}
