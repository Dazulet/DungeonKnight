using UnityEngine;

namespace DungeonKnight
{
   
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] PlayerStats stats;
        [SerializeField] PlayerInputReader input;
        [SerializeField] PlayerController controller;
        [SerializeField] PlayerHealth health;
        [SerializeField] Projectile projectilePrefab;
        [SerializeField] Transform muzzle;

        float nextShotTime;

        void Awake()
        {
            RefCheck.Valid(this, stats, nameof(stats));
            RefCheck.Valid(this, input, nameof(input));
            RefCheck.Valid(this, controller, nameof(controller));
            RefCheck.Valid(this, projectilePrefab, nameof(projectilePrefab));
            if (muzzle == null) muzzle = transform;
        }

        void Update()
        {
            if (stats == null || input == null || controller == null || projectilePrefab == null) return;
            if (health != null && !health.IsAlive) return;
            if (!input.ShootHeld || Time.time < nextShotTime) return;
            nextShotTime = Time.time + stats.fireCooldown;
            Fire();
        }

        void Fire()
        {
            Vector2 origin = muzzle.position;
            float aimRange = SaveManager.Settings.autoAim ? stats.autoAimRange : 0f;
            EnemyHealth target = EnemyHealth.FindNearest(origin, aimRange);
            Vector2 direction = target != null ? (Vector2)target.transform.position - origin : controller.Facing;
            if (direction.sqrMagnitude < 0.0001f) direction = controller.Facing;
            direction = Quaternion.Euler(0f, 0f, Random.Range(-stats.spreadDegrees, stats.spreadDegrees)) * direction.normalized;

            controller.FaceTowards(direction);
            GameAudio.Play(SoundId.PlayerShoot);
            Projectile bolt = Instantiate(projectilePrefab, origin, Quaternion.identity);
            bolt.Launch(direction, stats.boltSpeed, stats.boltDamage, stats.boltLifetime, stats.boltKnockback);
        }
    }
}
