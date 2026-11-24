using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace DungeonKnight
{
    public readonly struct SoundHandle
    {
        public readonly int voice;
        public readonly int serial;

        public SoundHandle(int voice, int serial)
        {
            this.voice = voice;
            this.serial = serial;
        }

        public bool IsValid => serial > 0;
    }

    public class GameAudio : MonoBehaviour
    {
        public const float MinLinear = 0.0001f;
        const float StopFadeTime = 0.04f;
        const float PanWidth = 0.6f;

        class Voice
        {
            public AudioSource source;
            public SoundId id;
            public int serial;
            public float startTime;
            public float endTime;
            public float volume;
            public bool stopping;
        }

        static GameAudio instance;

        readonly Dictionary<SoundId, SoundCue> cues = new Dictionary<SoundId, SoundCue>();
        readonly Dictionary<SoundId, float> lastPlayed = new Dictionary<SoundId, float>();
        readonly Dictionary<SoundId, int> nextClip = new Dictionary<SoundId, int>();
        readonly AudioSource[] music = new AudioSource[2];
        readonly float[] musicLevel = new float[2];
        readonly bool[] musicOn = new bool[2];

        AudioLibrary library;
        Voice[] voices;
        int activeMusic = -1;
        int serial;
        GameManager game;
        Camera view;
        bool sceneHandled;

        public static GameAudio Instance => instance;
        public static AudioLibrary Library => instance != null ? instance.library : null;
        public static bool IsPaused { get; private set; }
        public static int MusicStarts { get; private set; }
        public static int PlayCount { get; private set; }
        public static AudioClip CurrentMusic => instance != null && instance.activeMusic >= 0 ? instance.music[instance.activeMusic].clip : null;

        public static event System.Action<SoundId> Played;

        public static float ToDecibels(float linear) => 20f * Mathf.Log10(Mathf.Max(linear, MinLinear));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            instance = null;
            IsPaused = false;
            MusicStarts = 0;
            PlayCount = 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (instance != null) return;
            var lib = Resources.Load<AudioLibrary>(AudioLibrary.ResourcePath);
            if (lib == null)
            {
                Debug.LogWarning("GameAudio: no AudioLibrary in Resources. The game runs without sound.");
                return;
            }
            var go = new GameObject(nameof(GameAudio));
            DontDestroyOnLoad(go);
            instance = go.AddComponent<GameAudio>();
            instance.Init(lib);
        }

        void Init(AudioLibrary lib)
        {
            library = lib;
            if (lib.cues != null)
                foreach (SoundCue cue in lib.cues)
                    if (cue != null) cues[cue.id] = cue;

            voices = new Voice[Mathf.Max(1, lib.sfxVoices)];
            for (int i = 0; i < voices.Length; i++)
                voices[i] = new Voice { source = CreateSource($"Sfx {i}", lib.sfxGroup) };
            for (int i = 0; i < music.Length; i++)
            {
                music[i] = CreateSource($"Music {i}", lib.musicGroup);
                music[i].loop = true;
                music[i].priority = 0;
                music[i].volume = 0f;
            }
            SceneManager.sceneLoaded += OnSceneLoaded;
            SaveManager.VolumeChanged += ApplyVolumes;
        }

        AudioSource CreateSource(string sourceName, AudioMixerGroup group)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.outputAudioMixerGroup = group;
            return source;
        }

        void Start()
        {
            ApplyVolumes();
            if (!sceneHandled) OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SaveManager.VolumeChanged -= ApplyVolumes;
            if (game != null) game.StateChanged -= OnGameStateChanged;
            if (instance == this) instance = null;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) return;
            sceneHandled = true;
            view = null;
            if (game != null) game.StateChanged -= OnGameStateChanged;
            game = FindAnyObjectByType<GameManager>();
            if (game != null) game.StateChanged += OnGameStateChanged;
            SetPaused(game != null && game.State != GameState.Playing);
            ApplyVolumes();
            AudioClip clip = MusicFor(scene.name);
            if (clip != null) PlayMusic(clip);
        }

        AudioClip MusicFor(string sceneName)
        {
            if (sceneName == SceneNames.MainMenu) return library.menuMusic;
            if (sceneName == SceneNames.Dungeon || sceneName == SceneNames.Endless) return library.levelMusic;
            return null;
        }

        void OnGameStateChanged(GameState state) => SetPaused(state != GameState.Playing);

        void SetPaused(bool paused)
        {
            IsPaused = paused;
            AudioMixerSnapshot snapshot = paused ? library.pausedSnapshot : library.normalSnapshot;
            if (snapshot != null) snapshot.TransitionTo(library.snapshotFadeTime);
        }

        void ApplyVolumes()
        {
            if (library == null || library.mixer == null) return;
            GameSettings settings = SaveManager.Settings;
            library.mixer.SetFloat(AudioLibrary.MusicVolumeParameter, ToDecibels(settings.musicVolume));
            library.mixer.SetFloat(AudioLibrary.SfxVolumeParameter, ToDecibels(settings.sfxVolume));
        }

        public static void PlayMusic(AudioClip clip)
        {
            if (instance == null || clip == null) return;
            instance.StartMusic(clip);
        }

        void StartMusic(AudioClip clip)
        {
            if (activeMusic >= 0 && musicOn[activeMusic] && music[activeMusic].clip == clip) return;
            int other = activeMusic < 0 ? 0 : 1 - activeMusic;
            if (musicOn[other] && music[other].clip == clip)
            {
                activeMusic = other;
                return;
            }
            AudioSource source = music[other];
            source.Stop();
            source.clip = clip;
            source.volume = 0f;
            musicLevel[other] = 0f;
            source.Play();
            musicOn[other] = true;
            activeMusic = other;
            MusicStarts++;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float step = library.musicFadeTime > 0f ? dt / library.musicFadeTime : 1f;
            for (int i = 0; i < music.Length; i++)
            {
                float target = i == activeMusic ? 1f : 0f;
                musicLevel[i] = Mathf.MoveTowards(musicLevel[i], target, step);
                music[i].volume = Mathf.Sin(musicLevel[i] * Mathf.PI * 0.5f) * library.musicSourceVolume;
                if (musicOn[i] && target <= 0f && musicLevel[i] <= 0f)
                {
                    music[i].Stop();
                    musicOn[i] = false;
                }
            }

            float now = Time.unscaledTime;
            foreach (Voice voice in voices)
            {
                if (!voice.stopping) continue;
                voice.source.volume -= voice.volume * dt / StopFadeTime;
                if (voice.source.volume > 0f && now < voice.endTime) continue;
                voice.source.Stop();
                voice.stopping = false;
                voice.endTime = 0f;
            }
        }

        public static SoundHandle Play(SoundId id) => instance != null ? instance.PlayCue(id, false, Vector3.zero) : default;

        public static SoundHandle PlayAt(SoundId id, Vector3 position) => instance != null ? instance.PlayCue(id, true, position) : default;

        public static void Stop(SoundHandle handle)
        {
            if (instance == null || !handle.IsValid || handle.voice < 0 || handle.voice >= instance.voices.Length) return;
            Voice voice = instance.voices[handle.voice];
            if (voice.serial != handle.serial || Time.unscaledTime >= voice.endTime) return;
            voice.stopping = true;
        }

        public static bool IsPlaying(SoundHandle handle)
        {
            if (instance == null || !handle.IsValid || handle.voice < 0 || handle.voice >= instance.voices.Length) return false;
            Voice voice = instance.voices[handle.voice];
            return voice.serial == handle.serial && !voice.stopping && Time.unscaledTime < voice.endTime;
        }

        public static int ActiveVoices(SoundId id)
        {
            if (instance == null) return 0;
            int count = 0;
            float now = Time.unscaledTime;
            foreach (Voice voice in instance.voices)
                if (voice.id == id && voice.serial > 0 && now < voice.endTime) count++;
            return count;
        }

        SoundHandle PlayCue(SoundId id, bool positioned, Vector3 position)
        {
            if (!cues.TryGetValue(id, out SoundCue cue) || cue.clips == null || cue.clips.Length == 0) return default;
            float now = Time.unscaledTime;
            if (lastPlayed.TryGetValue(id, out float last) && now - last < cue.minInterval) return default;
            int index = PickVoice(id, cue.maxVoices, now);
            if (index < 0) return default;
            AudioClip clip = NextClip(id, cue);
            if (clip == null) return default;

            lastPlayed[id] = now;
            Voice voice = voices[index];
            AudioSource source = voice.source;
            float pitch = Random.Range(Mathf.Min(cue.minPitch, cue.maxPitch), Mathf.Max(cue.minPitch, cue.maxPitch));
            source.Stop();
            source.clip = clip;
            source.pitch = pitch;
            source.volume = cue.volume;
            source.panStereo = cue.pan && positioned ? PanFor(position) : 0f;
            voice.id = id;
            voice.serial = ++serial;
            voice.startTime = now;
            voice.endTime = now + clip.length / Mathf.Max(0.01f, pitch);
            voice.volume = cue.volume;
            voice.stopping = false;
            source.Play();
            PlayCount++;
            Played?.Invoke(id);
            return new SoundHandle(index, voice.serial);
        }

        int PickVoice(SoundId id, int maxVoices, float now)
        {
            int free = -1;
            int oldest = -1;
            int oldestSame = -1;
            int same = 0;
            for (int i = 0; i < voices.Length; i++)
            {
                Voice voice = voices[i];
                if (now >= voice.endTime)
                {
                    if (free < 0) free = i;
                    continue;
                }
                if (voice.id == id)
                {
                    same++;
                    if (oldestSame < 0 || voice.startTime < voices[oldestSame].startTime) oldestSame = i;
                }
                if (oldest < 0 || voice.startTime < voices[oldest].startTime) oldest = i;
            }
            if (same >= Mathf.Max(1, maxVoices)) return oldestSame;
            return free >= 0 ? free : oldest;
        }

        AudioClip NextClip(SoundId id, SoundCue cue)
        {
            if (cue.clips.Length == 1) return cue.clips[0];
            nextClip.TryGetValue(id, out int i);
            int pick = (i + Random.Range(0, cue.clips.Length - 1)) % cue.clips.Length;
            nextClip[id] = pick + 1;
            return cue.clips[pick];
        }

        float PanFor(Vector3 position)
        {
            if (view == null) view = Camera.main;
            if (view == null || !view.orthographic) return 0f;
            float halfWidth = view.orthographicSize * view.aspect;
            if (halfWidth <= 0f) return 0f;
            return Mathf.Clamp((position.x - view.transform.position.x) / halfWidth, -1f, 1f) * PanWidth;
        }
    }
}
