using System;
using System.Collections.Generic;

namespace DungeonKnight
{
    [Serializable]
    public class LevelRecord
    {
        public int level;
        public int bestScore;
        public float bestTime;
    }

    [Serializable]
    public class GameSettings
    {
        public const float DefaultMusicVolume = 0.8f;
        public const float DefaultSfxVolume = 0.8f;

        public bool screenShake = true;
        public bool autoAim = true;
        public bool effectsOff;
        public bool showFps;
        public float musicVolume = DefaultMusicVolume;
        public float sfxVolume = DefaultSfxVolume;

        public bool ValidateVolumes()
        {
            float music = CleanVolume(musicVolume, DefaultMusicVolume);
            float sfx = CleanVolume(sfxVolume, DefaultSfxVolume);
            bool changed = music != musicVolume || sfx != sfxVolume;
            musicVolume = music;
            sfxVolume = sfx;
            return changed;
        }

        static float CleanVolume(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return fallback;
            return Math.Max(0f, Math.Min(1f, value));
        }
    }

    [Serializable]
    public class SaveData
    {
        public const int FirstVersion = 1;
        public const int CurrentVersion = 1;
        public const int FirstLevel = 1;

        public int version;
        public List<int> unlockedLevels = new List<int> { FirstLevel };
        public List<int> completedLevels = new List<int>();
        public List<LevelRecord> bestScores = new List<LevelRecord>();
        public GameSettings settings = new GameSettings();

        public static SaveData CreateDefault() => new SaveData { version = CurrentVersion };

        public bool IsUnlocked(int level) => level == FirstLevel || unlockedLevels.Contains(level);

        public bool IsCompleted(int level) => completedLevels.Contains(level);

        public LevelRecord RecordFor(int level) => bestScores.Find(record => record.level == level);

        public bool RegisterWin(int level, int score, float time, int levelCount)
        {
            if (!completedLevels.Contains(level)) completedLevels.Add(level);
            Unlock(level);
            if (level + 1 <= levelCount) Unlock(level + 1);

            LevelRecord record = RecordFor(level);
            if (record == null)
            {
                bestScores.Add(new LevelRecord { level = level, bestScore = score, bestTime = time });
                return true;
            }
            if (record.bestTime <= 0f || time < record.bestTime) record.bestTime = time;
            if (score <= record.bestScore) return false;
            record.bestScore = score;
            return true;
        }

        public void ResetProgress()
        {
            unlockedLevels = new List<int> { FirstLevel };
            completedLevels = new List<int>();
            bestScores = new List<LevelRecord>();
        }

        public bool Validate(int levelCount)
        {
            int lastLevel = Math.Max(FirstLevel, levelCount);
            bool changed = false;
            if (unlockedLevels == null) { unlockedLevels = new List<int>(); changed = true; }
            if (completedLevels == null) { completedLevels = new List<int>(); changed = true; }
            if (bestScores == null) { bestScores = new List<LevelRecord>(); changed = true; }
            if (settings == null) { settings = new GameSettings(); changed = true; }
            changed |= settings.ValidateVolumes();

            changed |= KeepLevelsInRange(completedLevels, lastLevel);
            changed |= KeepLevelsInRange(unlockedLevels, lastLevel);
            if (!unlockedLevels.Contains(FirstLevel)) { unlockedLevels.Add(FirstLevel); changed = true; }
            foreach (int level in completedLevels)
            {
                if (!unlockedLevels.Contains(level)) { unlockedLevels.Add(level); changed = true; }
                if (level + 1 <= lastLevel && !unlockedLevels.Contains(level + 1)) { unlockedLevels.Add(level + 1); changed = true; }
            }
            unlockedLevels.Sort();
            completedLevels.Sort();

            var seen = new HashSet<int>();
            for (int i = 0; i < bestScores.Count; i++)
            {
                LevelRecord record = bestScores[i];
                if (record == null || !completedLevels.Contains(record.level) || !seen.Add(record.level))
                {
                    bestScores.RemoveAt(i--);
                    changed = true;
                    continue;
                }
                if (record.bestScore < 0) { record.bestScore = 0; changed = true; }
                if (float.IsNaN(record.bestTime) || float.IsInfinity(record.bestTime) || record.bestTime < 0f)
                {
                    record.bestTime = 0f;
                    changed = true;
                }
            }
            return changed;
        }

        static bool KeepLevelsInRange(List<int> levels, int lastLevel)
        {
            int before = levels.Count;
            var seen = new HashSet<int>();
            levels.RemoveAll(level => level < FirstLevel || level > lastLevel || !seen.Add(level));
            return levels.Count != before;
        }

        void Unlock(int level)
        {
            if (!unlockedLevels.Contains(level)) unlockedLevels.Add(level);
        }
    }
}
