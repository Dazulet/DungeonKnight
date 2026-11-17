using UnityEngine;

namespace DungeonKnight
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour, ILevelEnemy
    {
        [SerializeField] ChaserStats stats;
        [SerializeField] SpriteRenderer body;

        Rigidbody2D rb;
        PlayerHealth target;
        float nextAttackTime;
        float wobbleSeed;
        float spawnTime;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            wobbleSeed = Random.value * 10f;
            spawnTime = Time.time;
            RefCheck.Valid(this, stats, nameof(stats));
        }

        public void Init(PlayerHealth player)
        {
            target = player;
            nextAttackTime = Time.time + (stats != null ? stats.firstAttackDelay : 0f);
        }

        public void Configure(PlayerHealth player, EnemySpawn spawn) => Init(player);

        void FixedUpdate()
        {
            if (stats == null) return;
            Vector2 desired = Vector2.zero;
            if (target != null && target.IsAlive)
            {
                Vector2 toPlayer = (Vector2)target.transform.position - rb.position;
                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    desired = toPlayer.normalized * stats.moveSpeed;
                    if (stats.wobble > 0f)
                    {
                        Vector2 side = new Vector2(-toPlayer.y, toPlayer.x).normalized;
                        desired += side * Mathf.Sin((Time.time - spawnTime) * 6f + wobbleSeed) * stats.wobble;
                    }
                }
            }
#if UNITY_6000_0_OR_NEWER
            Vector2 current = rb.linearVelocity;
#else
            Vector2 current = rb.velocity;
#endif
            rb.SetVelocity(Vector2.MoveTowards(current, desired, stats.acceleration * Time.fixedDeltaTime));
        }

        void Update()
        {
            if (stats == null || target == null || !target.IsAlive) return;
            Vector2 toPlayer = target.transform.position - transform.position;
            if (body != null && Mathf.Abs(toPlayer.x) > 0.05f) body.flipX = toPlayer.x < 0f;

            if (Time.time >= nextAttackTime && toPlayer.sqrMagnitude <= stats.contactRange * stats.contactRange)
            {
                if (target.TakeDamage(stats.contactDamage)) nextAttackTime = Time.time + stats.attackCooldown;
            }
        }
    }
}
