using UnityEngine;

namespace DungeonKnight
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField, Min(0f)] float speed = 11f;
        [SerializeField, Min(1)] int damage = 1;
        [SerializeField, Min(0.1f)] float lifetime = 1.6f;
        [SerializeField, Min(0f)] float knockback = 3f;
        [SerializeField] GameObject hitEffect;

        Rigidbody2D rb;
        Vector2 direction = Vector2.right;
        bool spent;

        void Awake() => rb = GetComponent<Rigidbody2D>();

        public void Launch(Vector2 dir)
        {
            direction = dir.sqrMagnitude > 0f ? dir.normalized : Vector2.right;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            rb.SetVelocity(direction * speed);
            Destroy(gameObject, lifetime);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (spent) return;
            int layer = other.gameObject.layer;
            if (layer == GameLayers.Enemy)
            {
                var enemy = other.GetComponentInParent<EnemyHealth>();
                if (enemy == null || !enemy.IsAlive) return;
                enemy.TakeDamage(damage, direction * knockback);
                Break();
            }
            else if (layer == GameLayers.Obstacle)
            {
                Break();
            }
        }

        void Break()
        {
            spent = true;
            if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
