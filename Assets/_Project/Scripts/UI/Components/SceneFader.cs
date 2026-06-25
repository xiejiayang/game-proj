using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 场景切换淡入淡出（400ms ease-in-out）
    /// </summary>
    public class SceneFader : MonoBehaviour
    {
        public static SceneFader Instance { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.4f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            StartCoroutine(Fade(1f, 0f));
        }

        public void LoadScene(string sceneName)
        {
            StopAllCoroutines();
            StartCoroutine(FadeAndLoad(sceneName));
        }

        private IEnumerator FadeAndLoad(string sceneName)
        {
            yield return StartCoroutine(Fade(0f, 1f));
            SceneManager.LoadScene(sceneName);
        }

        private IEnumerator Fade(float from, float to)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = to > 0.5f;
                float t = 0f;
                while (t < fadeDuration)
                {
                    t += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                    yield return null;
                }
                canvasGroup.alpha = to;
                canvasGroup.blocksRaycasts = to > 0.5f;
            }
        }
    }
}
