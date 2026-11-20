using UnityEngine;

namespace DienGame.Gameplay.Phase13
{
    /// <summary>
    /// One 2D looping BGM source across all scenes. Replace a single BGM.ogg/.wav/.mp3
    /// in Assets/_Game/Resources/Phase13Audio to change music.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Phase13BackgroundMusic : MonoBehaviour
    {
        private const string MusicResource = "Phase13Audio/BGM";
        private const string VolumeKey = "DienGame_Phase13_BgmVolume";
        private const string MuteKey = "DienGame_Phase13_BgmMuted";

        private static Phase13BackgroundMusic s_instance;
        private AudioSource _source;

        public static float Volume => Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 0.28f));
        public static bool IsMuted => PlayerPrefs.GetInt(MuteKey, 0) != 0;
        public static AudioSource CurrentSource => s_instance != null ? s_instance._source : null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState() => s_instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartMusicAutomatically()
        {
            if (!Application.isPlaying || s_instance != null) return;
            GameObject go = new GameObject("DienGame_Phase13_BGM");
            DontDestroyOnLoad(go);
            go.AddComponent<Phase13BackgroundMusic>();
        }

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);
            _source = GetComponent<AudioSource>();
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.spatialBlend = 0f;
            _source.dopplerLevel = 0f;
            _source.priority = 32;
            ApplyVolume();
            TryLoadAndPlay();
        }

        private static AudioClip FindMusicClip()
        {
            AudioClip clip = Resources.Load<AudioClip>(MusicResource);
            if (clip != null) return clip;

            // Also helps diagnose an import with an unexpected clip name.
            AudioClip[] clips = Resources.LoadAll<AudioClip>("Phase13Audio");
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null &&
                    string.Equals(clips[i].name, "BGM", System.StringComparison.OrdinalIgnoreCase))
                    return clips[i];
            }

#if UNITY_EDITOR
            // Editor-only recovery: use the actual imported asset if Resources has not
            // resolved the asset yet. A build must resolve Resources normally.
            string folder = "Assets/_Game/Resources/Phase13Audio/BGM";
            string[] extensions = { ".ogg", ".wav", ".mp3" };
            for (int i = 0; i < extensions.Length; i++)
            {
                clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(folder + extensions[i]);
                if (clip != null)
                {
                    Debug.LogWarning("[DienGame BGM] Using Editor AudioClip fallback. " +
                                     "Check for duplicate BGM files and reimport the audio before building.");
                    return clip;
                }
            }
#endif
            return null;
        }

        private void TryLoadAndPlay()
        {
            if (_source == null) return;
            AudioClip clip = FindMusicClip();
            if (clip == null)
            {
                Debug.LogWarning("[DienGame BGM] AudioClip not imported/resolved. " +
                    "Check Assets/_Game/Resources/Phase13Audio/BGM.ogg (or BGM.wav/mp3). " +
                    "Use DienGame > Audio > Background Music and SFX > Reimport audio files.");
                return;
            }
            _source.clip = clip;
            _source.loop = true;
            ApplyVolume();
            _source.Play();
        }

        public static void RetryPlayback()
        {
            if (!Application.isPlaying) return;
            if (s_instance == null)
            {
                StartMusicAutomatically();
                return;
            }
            if (s_instance._source == null) return;
            if (s_instance._source.isPlaying) s_instance._source.Stop();
            s_instance.TryLoadAndPlay();
        }

        private void OnDestroy()
        {
            if (s_instance == this) s_instance = null;
        }

        private void ApplyVolume()
        {
            if (_source != null) _source.volume = IsMuted ? 0f : Volume;
        }

        public static void SetVolume(float volume)
        {
            PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(volume));
            PlayerPrefs.Save();
            if (s_instance != null) s_instance.ApplyVolume();
        }

        public static void SetMuted(bool muted)
        {
            PlayerPrefs.SetInt(MuteKey, muted ? 1 : 0);
            PlayerPrefs.Save();
            if (s_instance != null) s_instance.ApplyVolume();
        }
    }
}
