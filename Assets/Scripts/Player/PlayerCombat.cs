using UnityEngine;

namespace DungeonKnight
{
   
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] PlayerInputReader input;
        [SerializeField] PlayerController controller;
        [SerializeField] PlayerHealth health;
        [SerializeField] Projectile projectilePrefab;
        [SerializeField] Transform muzzle;
        [SerializeField, Min(0.05f)] float cooldown = 0.28f;
        [SerializeField, Min(0f)] float autoAimRange = 9f;
        [SerializeField, Range(0f, 15f)] float spreadDegrees = 3f;

        float nextShotTime;

        void Awake()
        {
            RefCheck.Valid(this, input, nameof(input));
            RefCheck.Valid(this, controller, nameof(controller));
            RefCheck.Valid(this, projectilePrefab, nameof(projectilePrefab));
            if (muzzle == null) muzzle = transform;
        }

        void Update()
        {
            if (input == null || controller == null || projectilePrefab == null) return;
            if (health != null && !health.IsAlive) return;
            if (!input.ShootHeld || Time.time < nextShotTime) return;
            nextShotTime = Time.time + cooldown;
            Fire();
        }

        void Fire()
        {
            Vector2 origin = muzzle.position;
            EnemyHealth target = EnemyHealth.FindNearest(origin, autoAimRange);
            Vector2 direction = target != null ? (Vector2)target.transform.position - origin : controller.Facing;
            if (direction.sqrMagnitude < 0.0001f) direction = controller.Facing;
            direction = Quaternion.Euler(0f, 0f, Random.Range(-spreadDegrees, spreadDegrees)) * direction.normalized;

            controller.FaceTowards(direction);
            Projectile bolt = Instantiate(projectilePrefab, origin, Quaternion.identity);
            bolt.Launch(direction);
        }
    }
}
