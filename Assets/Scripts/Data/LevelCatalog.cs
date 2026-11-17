using UnityEngine;

namespace DungeonKnight
{
    [CreateAssetMenu(menuName = "Dungeon Knight/Level Catalog", fileName = "LevelCatalog")]
    public class LevelCatalog : ScriptableObject
    {
        public const string ResourcePath = "LevelCatalog";

        public LevelData[] levels;

        static LevelCatalog instance;

        public static LevelCatalog Instance
        {
            get
            {
                if (instance == null) instance = Resources.Load<LevelCatalog>(ResourcePath);
                return instance;
            }
        }

        public int Count => levels != null ? levels.Length : 0;

        public LevelData First => Count > 0 ? levels[0] : null;

        public LevelData Find(int levelNumber)
        {
            if (levels == null) return null;
            foreach (LevelData level in levels)
                if (level != null && level.levelNumber == levelNumber) return level;
            return null;
        }
    }
}
