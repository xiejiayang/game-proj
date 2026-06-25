using UnityEngine;

namespace Dujiangyan.Systems
{
    /// <summary>
    /// 音频服务：音乐与音效分轨，音量可持久化
    /// </summary>
    public class AudioSystem : MonoBehaviour
    {
        public static AudioSystem Instance { get; private set; }

        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.7f;

        private AudioSource musicSource;
        private AudioSource sfxSource;

        public float MusicVolume => musicVolume;
        public float SFXVolume => sfxVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
        }

        private void Start()
        {
            if (SaveSystem.Instance != null)
            {
                var profile = SaveSystem.Instance.LoadProfile();
                SetMusicVolume(profile.settings.musicVolume);
                SetSFXVolume(profile.settings.sfxVolume);
            }
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null || musicSource == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume;
            PersistVolume();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null)
                sfxSource.volume = sfxVolume;
            PersistVolume();
        }

        private void PersistVolume()
        {
            if (SaveSystem.Instance == null) return;
            var profile = SaveSystem.Instance.LoadProfile();
            profile.settings.musicVolume = musicVolume;
            profile.settings.sfxVolume = sfxVolume;
            SaveSystem.Instance.SaveProfile(profile);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
