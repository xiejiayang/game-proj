using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 标题页：开始治水
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        [SerializeField] private Button startButton;

        public void SetButton(Button btn)
        {
            startButton = btn;
            if (startButton != null)
                startButton.onClick.AddListener(StartGame);
        }

        private void Start()
        {
            if (startButton != null)
                startButton.onClick.AddListener(StartGame);
        }

        private void StartGame()
        {
            SceneManager.LoadScene("Intro_L1");
        }

        private void OnDestroy()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(StartGame);
        }
    }
}
