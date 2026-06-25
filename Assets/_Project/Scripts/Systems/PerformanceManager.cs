using UnityEngine;

namespace Dujiangyan.Systems
{
    /// <summary>
    /// 动态性能适配：根据帧率调整粒子数量与渲染质量
    /// </summary>
    public class PerformanceManager : MonoBehaviour
    {
        public static PerformanceManager Instance { get; private set; }

        [SerializeField] private float checkInterval = 2f;
        [SerializeField] private int targetFPS = 30;
        [SerializeField] private int lowFPSThreshold = 25;
        [SerializeField] private int highFPSThreshold = 45;
        [SerializeField] private int minParticles = 50;
        [SerializeField] private int maxParticles = 200;

        private float timer;
        private int frameCount;
        private int currentQualityLevel;
        private int currentParticleLimit;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = targetFPS;
            currentQualityLevel = QualitySettings.GetQualityLevel();
            currentParticleLimit = maxParticles;
        }

        private void Update()
        {
            frameCount++;
            timer += Time.unscaledDeltaTime;

            if (timer >= checkInterval)
            {
                float fps = frameCount / timer;
                frameCount = 0;
                timer = 0f;
                Adapt(fps);
            }
        }

        private void Adapt(float fps)
        {
            if (fps < lowFPSThreshold)
            {
                // 先降粒子上限
                if (currentParticleLimit > minParticles)
                {
                    currentParticleLimit = Mathf.Max(minParticles, currentParticleLimit / 2);
                    WaterSimulation.Instance?.SetMaxParticles(currentParticleLimit);
                    Debug.Log($"[PerformanceManager] FPS {fps:F1} -> lower particle limit to {currentParticleLimit}");
                    return;
                }

                // 粒子已最低则降画质
                if (currentQualityLevel > 0)
                {
                    currentQualityLevel--;
                    QualitySettings.SetQualityLevel(currentQualityLevel);
                    Debug.Log($"[PerformanceManager] FPS {fps:F1} -> quality level {currentQualityLevel}");
                }
            }
            else if (fps > highFPSThreshold && currentParticleLimit < maxParticles)
            {
                currentParticleLimit = Mathf.Min(maxParticles, currentParticleLimit + 25);
                WaterSimulation.Instance?.SetMaxParticles(currentParticleLimit);
                Debug.Log($"[PerformanceManager] FPS {fps:F1} -> raise particle limit to {currentParticleLimit}");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
