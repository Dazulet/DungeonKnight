using System.Collections.Generic;
using UnityEngine;

namespace DungeonKnight
{
    public class EnemyHealth : MonoBehaviour
    {
        static readonly List<EnemyHealth> active = new List<EnemyHealth>();

        [SerializeField, Min(1)] int maxHealth = 3;
        [SerializeField, Min(0)] int scoreValue = 100;
        [SerializeField, Range(0f, 1f)] float coinDropChance = 0.4f;
        [SerializeField] HitFlash hitFlash;
        [SerializeField] GameObject deathEffect;
        [SerializeField] CoinPickup coinPrefab;
        [SerializeField] ScoreManager score;
        [SerializeField] Transform player;

        Rigidbody2D rb;

        public int Health { get; private set; }
        public bool IsAlive => Health > 0;
        public static int ActiveCount => active.Count;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            Health = maxHealth;
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
            if (Health <= 0) Die();
        }

        void Die()
        {
            if (score != null) score.AddScore(scoreValue);
            if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
            if (coinPrefab != null && Random.value < coinDropChance)
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
