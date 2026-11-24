using System;
using UnityEngine;
using UnityEngine.Audio;

namespace DungeonKnight
{
    public enum SoundId
    {
        Footstep,
        PlayerShoot,
        EnemyHit,
        EnemyDeath,
        PlayerHurt,
        CoinPickup,
        PatrollerWindUp,
        PatrollerSlam,
        ShooterAim,
        ShooterFire,
        ChargerWindUp,
        ChargerDash,
        ChaserBite,
        UiClick,
        Victory,
        Defeat,
    }

    [Serializable]
    public class SoundCue
    {
        public SoundId id;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 1.5f)] public float minPitch = 1f;
        [Range(0.5f, 1.5f)] public float maxPitch = 1f;
        [Min(1)] public int maxVoices = 2;
        [Min(0f)] public float minInterval = 0.03f;
        public bool pan = true;
    }

    [CreateAssetMenu(menuName = "Dungeon Knight/Audio Library", fileName = "AudioLibrary")]
    public class AudioLibrary : ScriptableObject
    {
        public const string ResourcePath = "AudioLibrary";
        public const string MusicVolumeParameter = "MusicVolume";
        public const string SfxVolumeParameter = "SfxVolume";
        public const string MusicPauseVolumeParameter = "MusicPauseVolume";
        public const string MusicPauseCutoffParameter = "MusicPauseCutoff";

        public AudioMixer mixer;
        public AudioMixerGroup musicGroup;
        public AudioMixerGroup sfxGroup;
        public AudioMixerSnapshot normalSnapshot;
        public AudioMixerSnapshot pausedSnapshot;
        public AudioClip menuMusic;
        public AudioClip levelMusic;
        [Min(0f)] public float musicFadeTime = 1.5f;
        [Min(0f)] public float snapshotFadeTime = 0.3f;
        [Range(0f, 1f)] public float musicSourceVolume = 0.7f;
        [Min(1)] public int sfxVoices = 12;
        public SoundCue[] cues;

        public SoundCue Find(SoundId id)
        {
            if (cues == null) return null;
            foreach (SoundCue cue in cues)
                if (cue != null && cue.id == id) return cue;
            return null;
        }
    }
}
