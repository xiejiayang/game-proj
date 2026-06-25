using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Dujiangyan.UI
{
    /// <summary>
    /// L1 引子页：叙事 + 进入关卡
    /// </summary>
    public class IntroUI : MonoBehaviour
    {
        [SerializeField] private Button skipButton;
        [SerializeField] private Button enterButton;

        public void SetButtons(Button skip, Button enter)
        {
            skipButton = skip;
            enterButton = enter;
            Wire();
        }

        private void Start()
        {
            Wire();
        }

        private void Wire()
        {
            if (skipButton != null)
                skipButton.onClick.AddListener(EnterLevel);
            if (enterButton != null)
                enterButton.onClick.AddListener(EnterLevel);
        }

        private void EnterLevel()
        {
            SceneManager.LoadScene("Level_L1");
        }

        private void OnDestroy()
        {
            if (skipButton != null)
                skipButton.onClick.RemoveListener(EnterLevel);
            if (enterButton != null)
                enterButton.onClick.RemoveListener(EnterLevel);
        }
    }
}
