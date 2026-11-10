using UnityEngine;

namespace DungeonKnight
{
    public class EnemyBolt : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] float radius = 0.12f;
        [SerializeField] LayerMask hitMask = (1 << GameLayers.Player) | (1 << GameLayers.Obstacle);
        [SerializeField] GameObject hitEffect;

        Vector2 direction = Vector2.right;
        float speed;
        int damage;
        float distanceLeft;
        bool launched;
        bool spent;

        public float Radius => radius;

        public void Launch(Vector2 launchDirection, float launchSpeed, int launchDamage, float range)
        {
            direction = launchDirection.sqrMagnitude > 0f ? launchDirection.normalized : Vector2.right;
            speed = launchSpeed;
            damage = launchDamage;
            distanceLeft = range;
            launched = true;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        void Update()
        {
            if (!launched || spent) return;
            float travel = Mathf.Min(speed * Time.deltaTime, distanceLeft);
            if (travel <= 0f)
            {
                if (distanceLeft <= 0f) Break();
                return;
            }

            Vector2 position = transform.position;
            RaycastHit2D hit = Physics2D.CircleCast(position, radius, direction, travel, hitMask);
            if (hit.collider != null)
            {
                transform.position = hit.centroid;
                Impact(hit.collider);
                return;
            }

            transform.position = position + direction * travel;
            distanceLeft -= travel;
            if (distanceLeft <= 0f) Break();
        }

        void Impact(Collider2D other)
        {
            if (other.gameObject.layer == GameLayers.Player)
            {
                var player = other.GetComponentInParent<PlayerHealth>();
                if (player != null) player.TakeDamage(damage);
            }
            Break();
        }

        void Break()
        {
            spent = true;
            if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
