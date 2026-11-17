using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKnight
{
    [DefaultExecutionOrder(-100)]
    public class LevelLoader : MonoBehaviour
    {
        [SerializeField] LevelGoal goal;
        [SerializeField] PlayerHealth player;
        [SerializeField] ScoreManager score;
        [SerializeField] Transform enemyRoot;
        [SerializeField] GameObject spawnEffect;
        [SerializeField, Min(0f)] float spawnEffectLead = 0.5f;

        readonly Dictionary<string, int> namesUsed = new Dictionary<string, int>();

        public LevelData Level { get; private set; }

        bool CanSpawn => player != null && player.IsAlive && !goal.IsComplete && !goal.TimedOut;

        void Awake()
        {
            if (!RefCheck.Valid(this, goal, nameof(goal)) || !RefCheck.Valid(this, player, nameof(player))
                || !RefCheck.Valid(this, score, nameof(score))) return;

            Level = LevelSession.Current;
            if (Level == null)
            {
                Debug.LogError("No level to load. Add LevelData assets to Resources/LevelCatalog.", this);
                return;
            }

            var spawns = new List<EnemySpawn>();
            if (Level.enemies != null)
                foreach (EnemySpawn spawn in Level.enemies)
                {
                    if (spawn != null && spawn.prefab != null && spawn.prefab.GetComponent<EnemyHealth>() != null) spawns.Add(spawn);
                    else Debug.LogError($"{Level.name} has an enemy entry without an enemy prefab. It was skipped.", Level);
                }

            goal.Begin(Level, spawns.Count);
            foreach (EnemySpawn spawn in spawns)
            {
                string enemyName = NextName(spawn.prefab.name);
                if (spawn.spawnDelay > 0f) StartCoroutine(SpawnLater(spawn, enemyName));
                else Spawn(spawn, enemyName);
            }
        }

        string NextName(string baseName)
        {
            namesUsed.TryGetValue(baseName, out int used);
            namesUsed[baseName] = used + 1;
            return $"{baseName} {(char)('A' + used)}";
        }

        IEnumerator SpawnLater(EnemySpawn spawn, string enemyName)
        {
            float lead = Mathf.Min(spawnEffectLead, spawn.spawnDelay);
            yield return new WaitForSeconds(spawn.spawnDelay - lead);
            if (!CanSpawn) yield break;
            if (spawnEffect != null) Instantiate(spawnEffect, spawn.position, Quaternion.identity);
            yield return new WaitForSeconds(lead);
            if (CanSpawn) Spawn(spawn, enemyName);
        }

        void Spawn(EnemySpawn spawn, string enemyName)
        {
            GameObject enemy = Instantiate(spawn.prefab, spawn.position, Quaternion.identity, enemyRoot);
            enemy.name = enemyName;
            var health = enemy.GetComponent<EnemyHealth>();
            health.Init(score, player.transform);
            if (enemy.TryGetComponent(out ILevelEnemy levelEnemy)) levelEnemy.Configure(player, spawn);
            goal.Track(health);
        }
    }
}
