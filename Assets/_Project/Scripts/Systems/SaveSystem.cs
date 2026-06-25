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
            string json = JsonUtility.ToJson(profile);
            PlayerPrefs.SetString(ProfileKey, json);
            PlayerPrefs.Save();
        }

        public PlayerProfile LoadProfile()
        {
            if (PlayerPrefs.HasKey(ProfileKey))
            {
                string json = PlayerPrefs.GetString(ProfileKey);
                var profile = JsonUtility.FromJson<PlayerProfile>(json);
                if (profile != null)
                    return profile;
            }
            return new PlayerProfile();
        }

        public void SaveLevelProgress(string levelId, PuzzleRuntime runtime)
        {
            if (runtime == null) return;
            string key = $"Dujiangyan_LevelProgress_{levelId}";
            string json = JsonUtility.ToJson(runtime);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public PuzzleRuntime LoadLevelProgress(string levelId)
        {
            string key = $"Dujiangyan_LevelProgress_{levelId}";
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                return JsonUtility.FromJson<PuzzleRuntime>(json);
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
