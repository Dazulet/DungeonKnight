using UnityEngine;

namespace DungeonKnight
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField, Min(0f)] float moveSpeed = 1.7f;
        [SerializeField, Min(0f)] float acceleration = 12f;
        [SerializeField, Min(0f)] float contactRange = 0.7f;
        [SerializeField, Min(0f)] float attackCooldown = 0.9f;
        [SerializeField, Min(1)] int contactDamage = 1;
        [SerializeField, Min(0f)] float wobble;
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
        }

        public void Init(PlayerHealth player)
        {
            target = player;
            nextAttackTime = Time.time + 0.5f;
        }

        void FixedUpdate()
        {
            Vector2 desired = Vector2.zero;
            if (target != null && target.IsAlive)
            {
                Vector2 toPlayer = (Vector2)target.transform.position - rb.position;
                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    desired = toPlayer.normalized * moveSpeed;
                    if (wobble > 0f)
                    {
                        Vector2 side = new Vector2(-toPlayer.y, toPlayer.x).normalized;
                        desired += side * Mathf.Sin((Time.time - spawnTime) * 6f + wobbleSeed) * wobble;
                    }
                }
            }
#if UNITY_6000_0_OR_NEWER
            Vector2 current = rb.linearVelocity;
#else
            Vector2 current = rb.velocity;
#endif
            rb.SetVelocity(Vector2.MoveTowards(current, desired, acceleration * Time.fixedDeltaTime));
        }

        void Update()
        {
            if (target == null || !target.IsAlive) return;
            Vector2 toPlayer = target.transform.position - transform.position;
            if (body != null && Mathf.Abs(toPlayer.x) > 0.05f) body.flipX = toPlayer.x < 0f;

            if (Time.time >= nextAttackTime && toPlayer.sqrMagnitude <= contactRange * contactRange)
            {
                if (target.TakeDamage(contactDamage)) nextAttackTime = Time.time + attackCooldown;
            }
        }
    }
}
