using UnityEngine;

namespace Dujiangyan.UI
{
    /// <summary>
    /// 将 Canvas 适配到屏幕安全区（刘海/圆角）
    /// </summary>
    public class SafeAreaFitter : MonoBehaviour
    {
        private void Awake()
        {
            var rect = GetComponent<RectTransform>();
            if (rect == null) return;

            Rect safe = Screen.safeArea;
            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
        }
    }
}
