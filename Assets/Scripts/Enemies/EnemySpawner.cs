using System.Collections;
using UnityEngine;

namespace DungeonKnight
{
    
    public class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct EnemyType
        {
            public GameObject prefab;
            [Min(0f)] public float weight;
            [Tooltip("Seconds into the run before this enemy can appear.")]
            [Min(0f)] public float unlockTime;
        }

        [SerializeField] PlayerHealth player;
        [SerializeField] ScoreManager score;
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] EnemyType[] enemyTypes;
        [SerializeField] GameObject spawnEffect;
        [SerializeField, Min(0)] int initialCount = 3;
        [SerializeField, Min(1)] int maxAlive = 8;
        [SerializeField, Min(0.1f)] float startInterval = 2.5f;
        [SerializeField, Min(0.1f)] float minInterval = 0.9f;
        [SerializeField, Min(1f)] float rampDuration = 90f;
        [SerializeField, Min(0f)] float minPlayerDistance = 4.5f;
        [SerializeField, Min(0.05f)] float clearanceRadius = 0.4f;
        [SerializeField, Min(0f)] float spawnDelay = 0.35f;

        float runTime;

        void Awake()
        {
            RefCheck.Valid(this, player, nameof(player));
            RefCheck.Valid(this, score, nameof(score));
            if (spawnPoints == null || spawnPoints.Length == 0)
                Debug.LogError("EnemySpawner has no spawn points.", this);
            if (enemyTypes == null || enemyTypes.Length == 0)
                Debug.LogError("EnemySpawner has no enemy types.", this);
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(0.6f);
            for (int i = 0; i < initialCount; i++) TrySpawn();
            while (true)
            {
                float t = Mathf.Clamp01(runTime / rampDuration);
                yield return new WaitForSeconds(Mathf.Lerp(startInterval, minInterval, t));
                if (player == null || !player.IsAlive) yield break;
                if (EnemyHealth.ActiveCount < maxAlive) TrySpawn();
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
            if (Vector2.Distance(position, player.transform.position) < minPlayerDistance) return false;
            int blockers = GameLayers.Mask(GameLayers.Obstacle);
            return Physics2D.OverlapCircle(position, clearanceRadius, blockers) == null;
        }

        IEnumerator SpawnAt(GameObject prefab, Vector2 position)
        {
            if (spawnEffect != null) Instantiate(spawnEffect, position, Quaternion.identity);
            yield return new WaitForSeconds(spawnDelay);
            if (player == null || !player.IsAlive) yield break;

            GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
            if (enemy.TryGetComponent(out EnemyController controller)) controller.Init(player);
            if (enemy.TryGetComponent(out EnemyHealth health)) health.Init(score, player.transform);
        }

        GameObject PickPrefab()
        {
            float total = 0f;
            foreach (EnemyType type in enemyTypes)
                if (type.prefab != null && runTime >= type.unlockTime) total += type.weight;
            if (total <= 0f) return null;

            float roll = Random.value * total;
            foreach (EnemyType type in enemyTypes)
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
