using System;
using UnityEngine;
using Dujiangyan.Data;

namespace Dujiangyan.Systems
{
    /// <summary>
    /// 存档服务：切片阶段使用 PlayerPrefs（WebGL 映射为 localStorage）
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }
        public event Action<string> OnSaveFailed;

        private const string ProfileKey = "Dujiangyan_PlayerProfile";

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

        public void SaveProfile(PlayerProfile profile)
        {
            if (profile == null) return;
            try
            {
                string json = JsonUtility.ToJson(profile);
                PlayerPrefs.SetString(ProfileKey, json);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] SaveProfile failed: {ex.Message}");
                OnSaveFailed?.Invoke("存档失败，设置/进度未保存");
            }
        }

        public PlayerProfile LoadProfile()
        {
            try
            {
                if (PlayerPrefs.HasKey(ProfileKey))
                {
                    string json = PlayerPrefs.GetString(ProfileKey);
                    var profile = JsonUtility.FromJson<PlayerProfile>(json);
                    if (profile != null)
                        return profile;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] LoadProfile failed: {ex.Message}");
            }
            return new PlayerProfile();
        }

        public void SaveLevelProgress(string levelId, PuzzleRuntime runtime)
        {
            if (runtime == null) return;
            try
            {
                string key = $"Dujiangyan_LevelProgress_{levelId}";
                string json = JsonUtility.ToJson(runtime);
                PlayerPrefs.SetString(key, json);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] SaveLevelProgress failed: {ex.Message}");
                OnSaveFailed?.Invoke("关卡进度保存失败");
            }
        }

        public PuzzleRuntime LoadLevelProgress(string levelId)
        {
            try
            {
                string key = $"Dujiangyan_LevelProgress_{levelId}";
                if (PlayerPrefs.HasKey(key))
                {
                    string json = PlayerPrefs.GetString(key);
                    return JsonUtility.FromJson<PuzzleRuntime>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] LoadLevelProgress failed: {ex.Message}");
            }
            return null;
        }

        public void ClearLevelProgress(string levelId)
        {
            string key = $"Dujiangyan_LevelProgress_{levelId}";
            PlayerPrefs.DeleteKey(key);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
