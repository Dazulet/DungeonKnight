using UnityEngine;

namespace DungeonKnight
{
    public static class LevelSession
    {
        static LevelData selected;

        public static LevelData Current
        {
            get
            {
                if (selected != null) return selected;
                LevelCatalog catalog = LevelCatalog.Instance;
                return catalog != null ? catalog.First : null;
            }
        }

        public static bool TryStart(LevelData level)
        {
            if (level == null || !SaveManager.IsUnlocked(level.levelNumber)) return false;
            selected = level;
            SceneLoader.Load(SceneNames.Dungeon);
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLaunch() => selected = null;
    }
}
