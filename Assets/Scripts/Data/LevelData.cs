using System;
using UnityEngine;

namespace DungeonKnight
{
    [Serializable]
    public class EnemySpawn
    {
        public GameObject prefab;
        public Vector2 position;
        public Vector2 facing = Vector2.right;
        public Vector2[] patrolRoute;
        [Min(0f)] public float spawnDelay;
    }

    [CreateAssetMenu(menuName = "Dungeon Knight/Level Data", fileName = "Level_00")]
    public class LevelData : ScriptableObject
    {
        [Min(1)] public int levelNumber = 1;
        public string displayName = "LEVEL 1";

        [Header("Objective: defeat every enemy before the time runs out")]
        [Min(10f)] public float timeLimit = 120f;

        [Header("Enemies")]
        public EnemySpawn[] enemies;

        public int EnemyCount => enemies != null ? enemies.Length : 0;
    }
}
