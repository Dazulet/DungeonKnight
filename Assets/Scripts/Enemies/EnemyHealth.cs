using System.Collections.Generic;
using UnityEngine;

namespace DungeonKnight
{
    public class EnemyHealth : MonoBehaviour
    {
        static readonly List<EnemyHealth> active = new List<EnemyHealth>();

        [SerializeField] EnemyStats stats;
        [SerializeField] HitFlash hitFlash;
        [SerializeField] GameObject deathEffect;
        [SerializeField] GameObject hitParticles;
        [SerializeField] GameObject deathParticles;
        [SerializeField] CoinPickup coinPrefab;
        [SerializeField] ScoreManager score;
        [SerializeField] Transform player;

        Rigidbody2D rb;

        public EnemyStats Stats => stats;
        public int Health { get; private set; }
        public bool IsAlive => Health > 0;
        public static int ActiveCount => active.Count;

        public event System.Action<EnemyHealth> Died;
        public event System.Action<Vector2> Damaged;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            Health = RefCheck.Valid(this, stats, nameof(stats)) ? stats.maxHealth : 1;
        }

        void OnEnable() => active.Add(this);
        void OnDisable() => active.Remove(this);

        public void Init(ScoreManager scoreManager, Transform playerTransform)
        {
            score = scoreManager;
            player = playerTransform;
        }

        public void TakeDamage(int amount, Vector2 push)
        {
            if (!IsAlive) return;
            Health -= amount;
            if (hitFlash != null) hitFlash.Flash();
            if (rb != null) rb.AddForce(push, ForceMode2D.Impulse);
            if (Health <= 0)
            {
                Die();
                return;
            }
            JuiceFx.Spawn(hitParticles, transform.position);
            Damaged?.Invoke(push);
        }

        void Die()
        {
            if (score != null && stats != null) score.AddScore(stats.scoreValue);
            Died?.Invoke(this);
            if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
            JuiceFx.Spawn(deathParticles, transform.position);
            CameraShake.Impact();
            HitStop.Trigger();
            if (coinPrefab != null && stats != null && Random.value < stats.coinDropChance)
            {
                CoinPickup coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
                coin.Init(score, player);
            }
            Destroy(gameObject);
        }

        public static EnemyHealth FindNearest(Vector2 from, float maxDistance)
        {
            EnemyHealth best = null;
            float bestSqr = maxDistance * maxDistance;
            foreach (EnemyHealth e in active)
            {
                if (!e.IsAlive) continue;
                float sqr = ((Vector2)e.transform.position - from).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = e;
                }
            }
            return best;
        }
    }
}
