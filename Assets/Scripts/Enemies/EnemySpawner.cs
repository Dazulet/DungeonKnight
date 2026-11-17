using System.Collections;
using UnityEngine;

namespace DungeonKnight
{
    
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] EndlessData data;
        [SerializeField] PlayerHealth player;
        [SerializeField] ScoreManager score;
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] GameObject spawnEffect;
        [SerializeField, Min(0.05f)] float clearanceRadius = 0.4f;

        float runTime;

        void Awake()
        {
            RefCheck.Valid(this, data, nameof(data));
            RefCheck.Valid(this, player, nameof(player));
            RefCheck.Valid(this, score, nameof(score));
            if (spawnPoints == null || spawnPoints.Length == 0)
                Debug.LogError("EnemySpawner has no spawn points.", this);
            if (data != null && (data.enemyTypes == null || data.enemyTypes.Length == 0))
                Debug.LogError("EndlessData has no enemy types.", this);
        }

        IEnumerator Start()
        {
            if (data == null) yield break;
            yield return new WaitForSeconds(data.firstSpawnDelay);
            for (int i = 0; i < data.initialCount; i++) TrySpawn();
            while (true)
            {
                float t = Mathf.Clamp01(runTime / data.rampDuration);
                yield return new WaitForSeconds(Mathf.Lerp(data.startInterval, data.minInterval, t));
                if (player == null || !player.IsAlive) yield break;
                if (EnemyHealth.ActiveCount < data.maxAlive) TrySpawn();
            }
        }

        void Update() => runTime += Time.deltaTime;

        bool TrySpawn()
        {
            if (player == null || spawnPoints == null || spawnPoints.Length == 0) return false;
            GameObject prefab = PickPrefab();
            if (prefab == null) return false;

            int start = Random.Range(0, spawnPoints.Length);
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform point = spawnPoints[(start + i) % spawnPoints.Length];
                if (point == null || !IsSafe(point.position)) continue;
                StartCoroutine(SpawnAt(prefab, point.position));
                return true;
            }
            return false;
        }

        bool IsSafe(Vector2 position)
        {
            if (Vector2.Distance(position, player.transform.position) < data.minPlayerDistance) return false;
            int blockers = GameLayers.Mask(GameLayers.Obstacle);
            return Physics2D.OverlapCircle(position, clearanceRadius, blockers) == null;
        }

        IEnumerator SpawnAt(GameObject prefab, Vector2 position)
        {
            if (spawnEffect != null) Instantiate(spawnEffect, position, Quaternion.identity);
            yield return new WaitForSeconds(data.spawnDelay);
            if (player == null || !player.IsAlive) yield break;

            GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
            if (enemy.TryGetComponent(out EnemyController controller)) controller.Init(player);
            if (enemy.TryGetComponent(out EnemyHealth health)) health.Init(score, player.transform);
        }

        GameObject PickPrefab()
        {
            if (data == null || data.enemyTypes == null) return null;
            float total = 0f;
            foreach (EndlessData.EnemyType type in data.enemyTypes)
                if (type.prefab != null && runTime >= type.unlockTime) total += type.weight;
            if (total <= 0f) return null;

            float roll = Random.value * total;
            foreach (EndlessData.EnemyType type in data.enemyTypes)
            {
                if (type.prefab == null || runTime < type.unlockTime) continue;
                roll -= type.weight;
                if (roll <= 0f) return type.prefab;
            }
            return null;
        }

        void OnDrawGizmos()
        {
            if (spawnPoints == null) return;
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.8f);
            foreach (Transform p in spawnPoints)
                if (p != null) Gizmos.DrawWireSphere(p.position, clearanceRadius);
        }
    }
}
