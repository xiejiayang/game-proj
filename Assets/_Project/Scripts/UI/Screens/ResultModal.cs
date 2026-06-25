using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dujiangyan.Data;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 结算弹窗：成功/失败第一屏 + 碑廊第二屏，带淡入/切换动画
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
        private CanvasGroup canvasGroup;
        private CanvasGroup screen1CG;
        private CanvasGroup screen2CG;
        private Coroutine currentAnim;

        private void Awake()
        {
            levelUI = GetComponentInParent<LevelUI>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            screen1CG = screen1 != null ? screen1.GetComponent<CanvasGroup>() : null;
            screen2CG = screen2 != null ? screen2.GetComponent<CanvasGroup>() : null;

            if (retryButtonS1 != null)
                retryButtonS1.onClick.AddListener(() => levelUI?.OnRetry());
            if (retryButtonS2 != null)
                retryButtonS2.onClick.AddListener(() => levelUI?.OnRetry());
            if (galleryButton != null)
                galleryButton.onClick.AddListener(() => ShowGallery());
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(() => levelUI?.OnRetry()); // placeholder

            gameObject.SetActive(false);
        }

        public void Show(PuzzleResult result)
        {
            gameObject.SetActive(true);
            StopAnim();

            screen1.SetActive(true);
            screen2.SetActive(true);
            if (screen1CG != null)
            {
                screen1CG.alpha = 1f;
                screen1CG.blocksRaycasts = true;
            }
            if (screen2CG != null)
            {
                screen2CG.alpha = 0f;
                screen2CG.blocksRaycasts = false;
            }

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

            currentAnim = StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, 0.3f, true));
        }

        public void ShowGallery()
        {
            galleryContent.text = "堵不如疏\n\n官方筑墙硬挡，水越涨越高；李冰用竹笼、杩槎把水分向两岸，村子才活下来。治水不是与水争地，而是顺势而为。";
            StopAnim();
            currentAnim = StartCoroutine(CrossfadeScreens(screen1CG, screen2CG, 0.3f));
        }

        public void Hide()
        {
            if (!gameObject.activeInHierarchy) return;
            StopAnim();
            currentAnim = StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f, 0.2f, false, () => gameObject.SetActive(false)));
        }

        private void StopAnim()
        {
            if (currentAnim != null)
            {
                StopCoroutine(currentAnim);
                currentAnim = null;
            }
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, bool enableRaycasts, System.Action onComplete = null)
        {
            cg.blocksRaycasts = enableRaycasts;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            cg.alpha = to;
            onComplete?.Invoke();
            currentAnim = null;
        }

        private IEnumerator CrossfadeScreens(CanvasGroup fromCG, CanvasGroup toCG, float duration)
        {
            if (fromCG != null)
            {
                fromCG.blocksRaycasts = false;
                fromCG.interactable = false;
            }
            if (toCG != null)
            {
                toCG.blocksRaycasts = true;
                toCG.interactable = true;
            }

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                if (fromCG != null) fromCG.alpha = 1f - p;
                if (toCG != null) toCG.alpha = p;
                yield return null;
            }
            if (fromCG != null) fromCG.alpha = 0f;
            if (toCG != null) toCG.alpha = 1f;
            currentAnim = null;
        }
    }
}
