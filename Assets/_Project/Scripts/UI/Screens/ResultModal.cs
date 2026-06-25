using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dujiangyan.Data;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 结算弹窗：成功/失败第一屏 + 碑廊第二屏
    /// </summary>
    public class ResultModal : MonoBehaviour
    {
        [SerializeField] private GameObject screen1;
        [SerializeField] private GameObject screen2;

        [Header("Screen 1 - Success")]
        [SerializeField] private GameObject successGroup;
        [SerializeField] private TextMeshProUGUI successTitle;
        [SerializeField] private TextMeshProUGUI successDesc;
        [SerializeField] private Button galleryButton;

        [Header("Screen 1 - Fail")]
        [SerializeField] private GameObject failGroup;
        [SerializeField] private TextMeshProUGUI failTitle;
        [SerializeField] private TextMeshProUGUI failDesc;

        [Header("Common")]
        [SerializeField] private TextMeshProUGUI sealLabel;
        [SerializeField] private Button retryButtonS1;
        [SerializeField] private Button retryButtonS2;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private TextMeshProUGUI galleryContent;

        private LevelUI levelUI;

        private void Awake()
        {
            levelUI = GetComponentInParent<LevelUI>();
            if (retryButtonS1 != null)
                retryButtonS1.onClick.AddListener(() => levelUI?.OnRetry());
            if (retryButtonS2 != null)
                retryButtonS2.onClick.AddListener(() => levelUI?.OnRetry());
            if (galleryButton != null)
                galleryButton.onClick.AddListener(() => ShowGallery());
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(() => levelUI?.OnRetry()); // placeholder
        }

        public void Show(PuzzleResult result)
        {
            gameObject.SetActive(true);
            screen1.SetActive(true);
            screen2.SetActive(false);

            if (result.isSuccess)
            {
                successGroup.SetActive(true);
                failGroup.SetActive(false);
                sealLabel.text = "安";
                sealLabel.color = new Color(0.18f, 0.35f, 0.29f); // accent
                successTitle.text = "暂时安全";
                successDesc.text = result.isFrugal
                    ? "你用的料、工、时都很少，李冰也会点头。"
                    : "村子保住了，但还能更节俭些。";
                galleryButton.gameObject.SetActive(true);
            }
            else
            {
                successGroup.SetActive(false);
                failGroup.SetActive(true);
                sealLabel.text = "倒";
                sealLabel.color = new Color(0.55f, 0.23f, 0.23f); // danger
                failTitle.text = "村子仍被淹";
                failDesc.text = result.failReason switch
                {
                    FailReason.Flood => "水流进了村庄，试试把水分向两边。",
                    FailReason.Destroyed => "关键构件被冲毁了，留意耐久。",
                    FailReason.Timeout => "耗时太久，动作再快些。",
                    _ => "再想想李冰的治水智慧。"
                };
                galleryButton.gameObject.SetActive(false);
            }
        }

        public void ShowGallery()
        {
            screen1.SetActive(false);
            screen2.SetActive(true);
            galleryContent.text = "堵不如疏\n\n官方筑墙硬挡，水越涨越高；李冰用竹笼、杩槎把水分向两岸，村子才活下来。治水不是与水争地，而是顺势而为。";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
