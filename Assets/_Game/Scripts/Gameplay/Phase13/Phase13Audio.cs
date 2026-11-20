using UnityEngine;

namespace DienGame.Gameplay.Phase13
{
    public enum Phase13Cue { Shot, Slash, Hit, PlayerHit, LevelUp, Boss, Victory, Defeat }

    /// <summary>
    /// Small reusable 2D SFX mixer. Sounds are original synthesized WAVs in Resources.
    /// Cooldowns prevent a swarm of enemies from creating hundreds of overlapping voices.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Phase13Audio : MonoBehaviour
    {
        private const string VolumeKey = "DienGame_Phase13_SfxVolume";
        private static Phase13Audio s_instance;
        private readonly AudioClip[] _clips = new AudioClip[8];
        private readonly float[] _lastCue = new float[8];
        private readonly AudioSource[] _sources = new AudioSource[4];
        private int _nextSource;
        private static readonly float[] Cooldowns = { .13f, .15f, .10f, .30f, .40f, .75f, .80f, .80f };
        public static float Volume => Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, .32f));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetInstance() => s_instance = null;

        public static void SetVolume(float value)
        {
            PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
        public static void Play(Phase13Cue cue)
        {
            if (!Application.isPlaying || Volume < .01f) return;
            if (s_instance == null)
            {
                GameObject obj = new GameObject("DienGame_Phase13_Audio");
                DontDestroyOnLoad(obj);
                s_instance = obj.AddComponent<Phase13Audio>();
            }
            s_instance.PlayCue(cue);
        }
        private void Awake()
        {
            for (int i = 0; i < _lastCue.Length; i++) _lastCue[i] = -100f;
            for (int i = 0; i < _sources.Length; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                source.priority = 96;
                _sources[i] = source;
            }
        }
        private void PlayCue(Phase13Cue cue)
        {
            int id = (int)cue;
            if (id < 0 || id >= _clips.Length) return;
            float now = Time.unscaledTime;
            if (now - _lastCue[id] < Cooldowns[id]) return;
            _lastCue[id] = now;
            if (_clips[id] == null)
                _clips[id] = Resources.Load<AudioClip>("Phase13Audio/" + cue);
            AudioClip clip = _clips[id];
            if (clip == null) return;
            AudioSource channel = _sources[_nextSource++ % _sources.Length];
            channel.Stop();
            channel.PlayOneShot(clip, Volume);
        }
    }
}
