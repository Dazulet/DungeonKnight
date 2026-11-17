using UnityEngine;

namespace DungeonKnight
{
    [CreateAssetMenu(menuName = "Dungeon Knight/Endless Data", fileName = "EndlessData")]
    public class EndlessData : ScriptableObject
    {
        [System.Serializable]
        public struct EnemyType
        {
            public GameObject prefab;
            [Min(0f)] public float weight;
            [Min(0f)] public float unlockTime;
        }

        public EnemyType[] enemyTypes;

        [Header("Pacing")]
        [Min(0f)] public float firstSpawnDelay = 0.6f;
        [Min(0)] public int initialCount = 3;
        [Min(1)] public int maxAlive = 8;
        [Min(0.1f)] public float startInterval = 2.5f;
        [Min(0.1f)] public float minInterval = 0.9f;
        [Min(1f)] public float rampDuration = 90f;

        [Header("Placement")]
        [Min(0f)] public float minPlayerDistance = 4.5f;
        [Min(0f)] public float spawnDelay = 0.35f;
    }
}
