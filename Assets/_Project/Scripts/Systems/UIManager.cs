using System.Collections.Generic;
using UnityEngine;
using Dujiangyan.Data;
using Dujiangyan.UI;

namespace Dujiangyan.Systems
{
    /// <summary>
    /// UI 服务：管理页面与弹窗栈
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private Transform screenRoot;
        [SerializeField] private Transform modalRoot;

        private readonly Dictionary<ScreenType, GameObject> screenRegistry = new Dictionary<ScreenType, GameObject>();
        private readonly Dictionary<ModalType, GameObject> modalRegistry = new Dictionary<ModalType, GameObject>();
        private readonly Stack<GameObject> modalStack = new Stack<GameObject>();

        private GameObject currentScreen;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterScreen(ScreenType type, GameObject screen)
        {
            if (screenRegistry.ContainsKey(type))
                screenRegistry[type] = screen;
            else
                screenRegistry.Add(type, screen);
        }

        public void RegisterModal(ModalType type, GameObject modal)
        {
            if (modalRegistry.ContainsKey(type))
                modalRegistry[type] = modal;
            else
                modalRegistry.Add(type, modal);
        }

        public void ShowScreen(ScreenType screen)
        {
            if (currentScreen != null)
                currentScreen.SetActive(false);

            if (screenRegistry.TryGetValue(screen, out GameObject prefab) && prefab != null)
            {
                currentScreen = prefab;
                currentScreen.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[UIManager] Screen not registered: {screen}");
            }
        }

        public void HideScreen(ScreenType screen)
        {
            if (screenRegistry.TryGetValue(screen, out GameObject prefab) && prefab != null)
                prefab.SetActive(false);

            if (currentScreen == prefab)
                currentScreen = null;
        }

        public void ShowModal(ModalType modal)
        {
            if (!modalRegistry.TryGetValue(modal, out GameObject prefab) || prefab == null)
            {
                Debug.LogWarning($"[UIManager] Modal not registered: {modal}");
                return;
            }

            GameObject instance = Instantiate(prefab, modalRoot);
            instance.SetActive(true);
            modalStack.Push(instance);
        }

        public void HideTopModal()
        {
            if (modalStack.Count == 0) return;
            GameObject top = modalStack.Pop();
            if (top != null)
                Destroy(top);
        }

        public void HideAllModals()
        {
            while (modalStack.Count > 0)
            {
                GameObject top = modalStack.Pop();
                if (top != null)
                    Destroy(top);
            }
        }

        public void ShowResult(PuzzleResult result)
        {
            Debug.Log($"[UIManager] ShowResult success={result.isSuccess} frugal={result.isFrugal} reason={result.failReason}");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
