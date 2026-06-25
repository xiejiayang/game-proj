using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dujiangyan.Data;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 老河工提示对话框：打字机效果 + 5s 自动关闭
    /// </summary>
    public class HintDialog : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI speakerLabel;
        [SerializeField] private TextMeshProUGUI contentLabel;
        [SerializeField] private Button closeButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float typeSpeed = 0.04f;
        [SerializeField] private float autoCloseDelay = 5f;

        private Coroutine currentRoutine;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        public void Show(HintNode node)
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            if (speakerLabel != null)
                speakerLabel.text = node.speaker;

            if (currentRoutine != null)
                StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(TypeText(node.text));
        }

        public void Hide()
        {
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        private IEnumerator TypeText(string text)
        {
            contentLabel.text = "";
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
