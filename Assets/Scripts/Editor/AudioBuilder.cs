using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace DungeonKnight.EditorTools
{
    public static class AudioBuilder
    {
        public const string AudioRoot = "Assets/Audio";
        public const string SfxRoot = AudioRoot + "/Sfx";
        public const string MusicRoot = AudioRoot + "/Music";
        public const string MixerPath = AudioRoot + "/GameMixer.mixer";
        public const string LibraryPath = "Assets/Resources/AudioLibrary.asset";
        public const string MasterGroup = "Master";
        public const string MusicGroup = "Music";
        public const string MusicPauseGroup = "Music Pause";
        public const string SfxGroup = "SFX";
        public const string NormalSnapshot = "Normal";
        public const string PausedSnapshot = "Paused";
        public const float PausedMusicDb = -10f;
        public const float OpenCutoff = 22000f;
        public const float PausedCutoff = 900f;
        const string LowpassEffect = "Lowpass Simple";
        const string CutoffParameter = "Cutoff freq";
        const string CompressorEffect = "Compressor";

        const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [MenuItem("Dungeon Knight/Build Sound Pass", priority = 10)]
        public static void Build()
        {
            AssetDatabase.Refresh();
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null) mixer = CreateMixer();
            if (mixer == null) return;
            if (mixer.updateMode != AudioMixerUpdateMode.UnscaledTime)
            {
                mixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
                EditorUtility.SetDirty(mixer);
            }
            BuildLibrary(mixer);
            AssetDatabase.SaveAssets();
            Debug.Log($"Dungeon Knight: sound pass assets ready ({MixerPath}, {LibraryPath}).");
        }

        static AudioMixer CreateMixer()
        {
            BuilderUtil.EnsureFolder(AudioRoot);
            Type controllerType = FindType("UnityEditor.Audio.AudioMixerController");
            object controller = Invoke(controllerType, null, "CreateMixerControllerAtPath", MixerPath);
            object master = Property(controller, "masterGroup");
            EnsureView(controller, master);

            object music = AddGroup(controller, MusicGroup, master);
            object musicPause = AddGroup(controller, MusicPauseGroup, music);
            object sfx = AddGroup(controller, SfxGroup, master);

            object lowpass = AddEffect(controller, musicPause, LowpassEffect);
            object compressor = AddEffect(controller, master, CompressorEffect);

            object normal = Property(controller, "TargetSnapshot");
            ((Object)normal).name = NormalSnapshot;
            Invoke(controller.GetType(), controller, "CloneNewSnapshotFromTarget", false);
            var snapshots = (Array)Property(controller, "snapshots");
            object paused = snapshots.GetValue(snapshots.Length - 1);
            ((Object)paused).name = PausedSnapshot;

            SetVolume(controller, musicPause, normal, 0f);
            SetVolume(controller, musicPause, paused, PausedMusicDb);
            SetEffectValue(controller, lowpass, normal, CutoffParameter, OpenCutoff);
            SetEffectValue(controller, lowpass, paused, CutoffParameter, PausedCutoff);
            foreach (object snapshot in new[] { normal, paused })
            {
                SetEffectValue(controller, compressor, snapshot, "Threshold", -6f);
                SetEffectValue(controller, compressor, snapshot, "Attack", 5f);
                SetEffectValue(controller, compressor, snapshot, "Release", 120f);
                SetEffectValue(controller, compressor, snapshot, "Make up gain", 0f);
            }

            Expose(controller, GroupVolumePath(music), AudioLibrary.MusicVolumeParameter);
            Expose(controller, GroupVolumePath(sfx), AudioLibrary.SfxVolumeParameter);
            Expose(controller, GroupVolumePath(musicPause), AudioLibrary.MusicPauseVolumeParameter);
            Expose(controller, EffectParameterPath(musicPause, lowpass, CutoffParameter), AudioLibrary.MusicPauseCutoffParameter);

            SetProperty(controller, "TargetSnapshot", normal);
            SetProperty(controller, "startSnapshot", normal);
            var mixer = (AudioMixer)controller;
            mixer.updateMode = AudioMixerUpdateMode.UnscaledTime;
            EditorUtility.SetDirty(mixer);
            foreach (Object sub in AssetDatabase.LoadAllAssetsAtPath(MixerPath)) EditorUtility.SetDirty(sub);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(MixerPath, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
        }

        static void EnsureView(object controller, object master)
        {
            PropertyInfo viewsProperty = controller.GetType().GetProperty("views", Any);
            var views = (Array)viewsProperty.GetValue(controller);
            if (views != null && views.Length > 0) return;
            Type viewType = viewsProperty.PropertyType.GetElementType();
            object view = Activator.CreateInstance(viewType);
            object groupId = Property(master, "groupID");
            Array guids = Array.CreateInstance(groupId.GetType(), 1);
            guids.SetValue(groupId, 0);
            viewType.GetField("guids", Any).SetValue(view, guids);
            viewType.GetField("name", Any).SetValue(view, "View");
            Array created = Array.CreateInstance(viewType, 1);
            created.SetValue(view, 0);
            viewsProperty.SetValue(controller, created);
            SetProperty(controller, "currentViewIndex", 0);
        }

        static object AddGroup(object controller, string name, object parent)
        {
            object group = Invoke(controller.GetType(), controller, "CreateNewGroup", name, false);
            Invoke(controller.GetType(), controller, "AddChildToParent", group, parent);
            Invoke(controller.GetType(), controller, "AddGroupToCurrentView", group);
            return group;
        }

        static object AddEffect(object controller, object group, string effectName)
        {
            Type effectType = FindType("UnityEditor.Audio.AudioMixerEffectController");
            ConstructorInfo constructor = effectType.GetConstructor(Any, null, new[] { typeof(string) }, null);
            object effect = constructor.Invoke(new object[] { effectName });
            MethodInfo addSubAsset = controller.GetType().GetMethods(Any).FirstOrDefault(m => m.Name == "AddNewSubAsset" && m.GetParameters().Length == 2);
            if (addSubAsset != null) addSubAsset.Invoke(controller, new object[] { effect, false });
            else AssetDatabase.AddObjectToAsset((Object)effect, (Object)controller);
            Invoke(effectType, effect, "PreallocateGUIDs");
            var effects = (Array)Property(group, "effects");
            Invoke(group.GetType(), group, "InsertEffect", effect, effects.Length);
            return effect;
        }

        static void SetVolume(object controller, object group, object snapshot, float value)
            => Invoke(group.GetType(), group, "SetValueForVolume", controller, snapshot, value);

        static void SetEffectValue(object controller, object effect, object snapshot, string parameter, float value)
            => Invoke(effect.GetType(), effect, "SetValueForParameter", controller, snapshot, parameter, value);

        static object GroupVolumePath(object group)
        {
            object guid = Invoke(group.GetType(), group, "GetGUIDForVolume");
            Type pathType = FindType("UnityEditor.Audio.AudioGroupParameterPath");
            return Activator.CreateInstance(pathType, Any, null, new[] { group, guid }, null);
        }

        static object EffectParameterPath(object group, object effect, string parameter)
        {
            object guid = Invoke(effect.GetType(), effect, "GetGUIDForParameter", parameter);
            Type pathType = FindType("UnityEditor.Audio.AudioEffectParameterPath");
            return Activator.CreateInstance(pathType, Any, null, new[] { group, effect, guid }, null);
        }

        static void Expose(object controller, object path, string name)
        {
            Invoke(controller.GetType(), controller, "AddExposedParameter", path);
            object guid = path.GetType().GetField("parameter", Any)?.GetValue(path) ?? Property(path, "parameter");
            var exposed = (Array)Property(controller, "exposedParameters");
            for (int i = 0; i < exposed.Length; i++)
            {
                object item = exposed.GetValue(i);
                FieldInfo guidField = item.GetType().GetField("guid", Any);
                if (!Equals(guidField.GetValue(item), guid)) continue;
                item.GetType().GetField("name", Any).SetValue(item, name);
                exposed.SetValue(item, i);
            }
            SetProperty(controller, "exposedParameters", exposed);
        }

        static void BuildLibrary(AudioMixer mixer)
        {
            BuilderUtil.EnsureFolder("Assets/Resources");
            var library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AudioLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            library.mixer = mixer;
            library.musicGroup = Group(mixer, MusicPauseGroup);
            library.sfxGroup = Group(mixer, SfxGroup);
            library.normalSnapshot = mixer.FindSnapshot(NormalSnapshot);
            library.pausedSnapshot = mixer.FindSnapshot(PausedSnapshot);
            library.menuMusic = Clip(MusicRoot, "music_menu");
            library.levelMusic = Clip(MusicRoot, "music_level");
            library.musicFadeTime = 1.5f;
            library.snapshotFadeTime = 0.3f;
            library.musicSourceVolume = 0.7f;
            library.sfxVoices = 12;
            library.cues = new[]
            {
                Cue(SoundId.Footstep, 0.55f, 0.92f, 1.08f, 2, 0.12f, false, "footstep_1", "footstep_2", "footstep_3"),
                Cue(SoundId.PlayerShoot, 0.5f, 0.92f, 1.08f, 3, 0.05f, false, "player_shoot_1", "player_shoot_2"),
                Cue(SoundId.EnemyHit, 0.75f, 0.92f, 1.08f, 3, 0.04f, true, "enemy_hit_1", "enemy_hit_2"),
                Cue(SoundId.EnemyDeath, 0.85f, 0.92f, 1.08f, 3, 0.05f, true, "enemy_death"),
                Cue(SoundId.PlayerHurt, 0.9f, 0.95f, 1.05f, 1, 0.15f, false, "player_hurt"),
                Cue(SoundId.CoinPickup, 0.7f, 0.92f, 1.08f, 3, 0.04f, true, "coin_1", "coin_2"),
                Cue(SoundId.PatrollerWindUp, 0.85f, 0.96f, 1.04f, 3, 0f, true, "patroller_windup"),
                Cue(SoundId.PatrollerSlam, 0.9f, 0.92f, 1.08f, 3, 0.03f, true, "patroller_slam"),
                Cue(SoundId.ShooterAim, 0.7f, 0.98f, 1.02f, 3, 0f, true, "shooter_aim"),
                Cue(SoundId.ShooterFire, 0.75f, 0.92f, 1.08f, 3, 0.03f, true, "shooter_fire"),
                Cue(SoundId.ChargerWindUp, 0.85f, 0.96f, 1.04f, 2, 0f, true, "charger_windup"),
                Cue(SoundId.ChargerDash, 0.8f, 0.92f, 1.08f, 2, 0.03f, true, "charger_dash"),
                Cue(SoundId.ChaserBite, 0.75f, 0.92f, 1.08f, 2, 0.08f, true, "chaser_bite"),
                Cue(SoundId.UiClick, 0.8f, 0.96f, 1.04f, 2, 0.04f, false, "ui_click"),
                Cue(SoundId.Victory, 0.9f, 1f, 1f, 1, 0.5f, false, "victory"),
                Cue(SoundId.Defeat, 0.9f, 1f, 1f, 1, 0.5f, false, "defeat"),
            };
            EditorUtility.SetDirty(library);

            foreach (SoundId id in Enum.GetValues(typeof(SoundId)))
                if (library.Find(id) == null) Debug.LogError($"AudioLibrary has no cue for {id}.");
            if (library.musicGroup == null || library.sfxGroup == null) Debug.LogError("GameMixer is missing the Music Pause or SFX group.");
            if (library.normalSnapshot == null || library.pausedSnapshot == null) Debug.LogError("GameMixer is missing the Normal or Paused snapshot.");
        }

        static AudioMixerGroup Group(AudioMixer mixer, string name)
            => mixer.FindMatchingGroups(string.Empty).FirstOrDefault(g => g.name == name);

        static SoundCue Cue(SoundId id, float volume, float minPitch, float maxPitch, int maxVoices, float minInterval, bool pan, params string[] clips)
        {
            var list = new List<AudioClip>();
            foreach (string clip in clips)
            {
                AudioClip loaded = Clip(SfxRoot, clip);
                if (loaded != null) list.Add(loaded);
            }
            return new SoundCue
            {
                id = id,
                clips = list.ToArray(),
                volume = volume,
                minPitch = minPitch,
                maxPitch = maxPitch,
                maxVoices = maxVoices,
                minInterval = minInterval,
                pan = pan,
            };
        }

        static AudioClip Clip(string folder, string name)
        {
            string path = $"{folder}/{name}.wav";
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) Debug.LogError($"Audio clip not found at {path}. Run Tools/generate_audio.py first.");
            return clip;
        }

        static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            throw new MissingMemberException($"Unity type {fullName} was not found.");
        }

        static object Invoke(Type type, object target, string method, params object[] args)
        {
            foreach (MethodInfo candidate in type.GetMethods(Any))
            {
                if (candidate.Name != method) continue;
                ParameterInfo[] parameters = candidate.GetParameters();
                if (parameters.Length != args.Length) continue;
                bool match = true;
                for (int i = 0; i < args.Length && match; i++)
                    match = args[i] == null ? !parameters[i].ParameterType.IsValueType : parameters[i].ParameterType.IsInstanceOfType(args[i]);
                if (match) return candidate.Invoke(target, args);
            }
            throw new MissingMethodException(type.FullName, method);
        }

        static object Property(object target, string name)
        {
            PropertyInfo property = target.GetType().GetProperty(name, Any);
            if (property == null) throw new MissingMemberException(target.GetType().FullName, name);
            return property.GetValue(target);
        }

        static void SetProperty(object target, string name, object value)
        {
            PropertyInfo property = target.GetType().GetProperty(name, Any);
            if (property == null) throw new MissingMemberException(target.GetType().FullName, name);
            property.SetValue(target, value);
        }
    }
}
