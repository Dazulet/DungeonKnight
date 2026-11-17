using System;
using System.IO;
using UnityEngine;

namespace DungeonKnight
{
    public enum SaveLoadResult { NotLoaded, Loaded, Repaired, Missing, Empty, Corrupted, Incompatible, Unreadable }

    public class SaveManager : MonoBehaviour
    {
        public const string FileName = "save.json";
        public const string BrokenFileName = "save.broken.json";

        static SaveManager instance;
        static SaveData data;

        public static SaveData Data
        {
            get
            {
                if (data == null) Load();
                return data;
            }
        }

        public static GameSettings Settings => Data.settings;
        public static SaveLoadResult LastLoadResult { get; private set; }
        public static bool RecoveredFromBadFile => LastLoadResult == SaveLoadResult.Empty || LastLoadResult == SaveLoadResult.Corrupted
            || LastLoadResult == SaveLoadResult.Incompatible || LastLoadResult == SaveLoadResult.Unreadable;
        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);
        static string BrokenPath => Path.Combine(Application.persistentDataPath, BrokenFileName);
        static string TempPath => FilePath + ".tmp";

        static int LevelCount
        {
            get
            {
                LevelCatalog catalog = LevelCatalog.Instance;
                return catalog != null ? catalog.Count : SaveData.FirstLevel;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            instance = null;
            data = null;
            LastLoadResult = SaveLoadResult.NotLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject(nameof(SaveManager));
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SaveManager>();
            Load();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        void OnApplicationQuit() => Save();

        public static bool IsUnlocked(int level) => Data.IsUnlocked(level);

        public static bool IsCompleted(int level) => Data.IsCompleted(level);

        public static LevelRecord RecordFor(int level) => Data.RecordFor(level);

        public static bool CompleteLevel(int level, int score, float time)
        {
            bool newBest = Data.RegisterWin(level, score, time, LevelCount);
            Save();
            return newBest;
        }

        public static void SetScreenShake(bool enabled)
        {
            Settings.screenShake = enabled;
            Save();
        }

        public static void SetAutoAim(bool enabled)
        {
            Settings.autoAim = enabled;
            Save();
        }

        public static void ResetProgress()
        {
            Data.ResetProgress();
            Save();
        }

        public static void Load()
        {
            data = Read(out SaveLoadResult result);
            LastLoadResult = result;
            Debug.Log($"SaveManager: {result} {FilePath}");
            if (result != SaveLoadResult.Loaded) Save();
        }

        public static bool Save()
        {
            if (data == null) return false;
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                data.version = SaveData.CurrentVersion;
                File.WriteAllText(TempPath, JsonUtility.ToJson(data, true));
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(TempPath, FilePath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not write the save file: {e.Message}");
                return false;
            }
        }

        static SaveData Read(out SaveLoadResult result)
        {
            if (!File.Exists(FilePath) && File.Exists(TempPath))
            {
                try { File.Move(TempPath, FilePath); }
                catch (Exception e) { Debug.LogWarning($"Could not restore the unfinished save: {e.Message}"); }
            }
            if (!File.Exists(FilePath))
            {
                result = SaveLoadResult.Missing;
                return SaveData.CreateDefault();
            }

            string json;
            try { json = File.ReadAllText(FilePath); }
            catch (Exception e) { return Recover(SaveLoadResult.Unreadable, e.Message, out result); }
            if (string.IsNullOrWhiteSpace(json)) return Recover(SaveLoadResult.Empty, "the file is empty", out result);

            SaveData loaded;
            try { loaded = JsonUtility.FromJson<SaveData>(json); }
            catch (Exception e) { return Recover(SaveLoadResult.Corrupted, e.Message, out result); }

            if (loaded == null) return Recover(SaveLoadResult.Corrupted, "no JSON object", out result);
            if (loaded.version > SaveData.CurrentVersion)
                return Recover(SaveLoadResult.Incompatible, $"version {loaded.version} is newer than {SaveData.CurrentVersion}", out result);
            if (loaded.version < SaveData.FirstVersion)
                return Recover(SaveLoadResult.Corrupted, $"version {loaded.version} is not valid", out result);

            result = loaded.Validate(LevelCount) ? SaveLoadResult.Repaired : SaveLoadResult.Loaded;
            return loaded;
        }

        static SaveData Recover(SaveLoadResult reason, string detail, out SaveLoadResult result)
        {
            result = reason;
            Debug.LogWarning($"The save file was not used ({reason}: {detail}). The game starts with default progress. A copy was kept as {BrokenFileName}.");
            try { File.Copy(FilePath, BrokenPath, true); }
            catch (Exception e) { Debug.LogWarning($"Could not keep a copy of the broken save: {e.Message}"); }
            return SaveData.CreateDefault();
        }
    }
}
