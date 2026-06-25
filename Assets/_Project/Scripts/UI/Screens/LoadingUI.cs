using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 加载页：异步加载 Level_L1，进度条反映真实进度
    /// </summary>
    public class LoadingUI : MonoBehaviour
    {
        [SerializeField] private Image fillBar;
        [SerializeField] private TextMeshProUGUI hintLabel;

        public void SetBar(Image bar)
        {
            fillBar = bar;
        }

        private void Start()
        {
            if (hintLabel != null)
                hintLabel.text = "研墨中…";
            StartCoroutine(LoadLevel());
        }

        private IEnumerator LoadLevel()
        {
            var operation = SceneManager.LoadSceneAsync("Level_L1");
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                if (fillBar != null)
                    fillBar.fillAmount = operation.progress / 0.9f;
                yield return null;
            }

            if (fillBar != null)
                fillBar.fillAmount = 1f;

            yield return new WaitForSeconds(0.3f);
            operation.allowSceneActivation = true;
        }
    }
}
