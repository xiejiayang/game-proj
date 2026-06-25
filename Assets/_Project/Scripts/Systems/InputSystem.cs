using System;
using UnityEngine;

namespace Dujiangyan.Systems
{
    /// <summary>
    /// 统一输入服务：封装鼠标与触摸输入，输出屏幕坐标
    /// </summary>
    public class InputSystem : MonoBehaviour
    {
        public static InputSystem Instance { get; private set; }

        public event Action<Vector2> OnPointerDown;
        public event Action<Vector2> OnPointerMove;
        public event Action<Vector2> OnPointerUp;

        public bool IsTouchDevice => Input.touchSupported;

        private bool isDragging;

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

        private void Update()
        {
            if (IsTouchDevice && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 pos = touch.position;

                if (touch.phase == TouchPhase.Began)
                {
                    isDragging = true;
                    OnPointerDown?.Invoke(pos);
                }
                else if (touch.phase == TouchPhase.Moved && isDragging)
                {
                    OnPointerMove?.Invoke(pos);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    isDragging = false;
                    OnPointerUp?.Invoke(pos);
                }
            }
            else
            {
                Vector2 mousePos = Input.mousePosition;

                if (Input.GetMouseButtonDown(0))
                {
                    isDragging = true;
                    OnPointerDown?.Invoke(mousePos);
                }
                else if (Input.GetMouseButton(0) && isDragging)
                {
                    OnPointerMove?.Invoke(mousePos);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    isDragging = false;
                    OnPointerUp?.Invoke(mousePos);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
