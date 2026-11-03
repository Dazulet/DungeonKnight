using System;
using UnityEngine;

namespace DungeonKnight
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] int maxHearts = 3;
        [SerializeField, Min(0f)] float invulnerableTime = 1.2f;
        [SerializeField, Min(0.01f)] float blinkInterval = 0.08f;
        [SerializeField] SpriteRenderer body;
        [SerializeField] HitFlash hitFlash;
        [SerializeField] GameObject deathEffect;

        float invulnerableUntil;

        public int MaxHearts => maxHearts;
        public int Hearts { get; private set; }
        public bool IsAlive => Hearts > 0;
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        public event Action<int, int> HeartsChanged;
        public event Action Damaged;
        public event Action Died;

        void Awake()
        {
            Hearts = maxHearts;
            RefCheck.Valid(this, body, nameof(body));
        }

        void Start() => HeartsChanged?.Invoke(Hearts, maxHearts);

        public bool TakeDamage(int amount)
        {
            if (!IsAlive || IsInvulnerable || amount <= 0) return false;
            Hearts = Mathf.Max(0, Hearts - amount);
            invulnerableUntil = Time.time + invulnerableTime;
            if (hitFlash != null) hitFlash.Flash();
            HeartsChanged?.Invoke(Hearts, maxHearts);
            Damaged?.Invoke();
            if (Hearts == 0) Die();
            return true;
        }

        void Die()
        {
            if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
            if (body != null) body.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
            Died?.Invoke();
        }

        void Update()
        {
            if (body == null || !IsAlive) return;
            body.enabled = !IsInvulnerable || Mathf.Repeat(Time.time, blinkInterval * 2f) > blinkInterval;
        }
    }
}
