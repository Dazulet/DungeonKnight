using UnityEngine;

namespace DungeonKnight
{
    [CreateAssetMenu(menuName = "Dungeon Knight/Shooter Stats", fileName = "ShooterStats")]
    public class ShooterStats : SightedEnemyStats
    {
        [Header("Watch")]
        [Min(0f)] public float returnSpeed = 1.5f;
        [Range(0f, 90f)] public float watchSweepAngle = 40f;
        [Min(0.1f)] public float watchSweepPeriod = 3.2f;

        [Header("Spacing")]
        [Min(0f)] public float minDistance = 2.5f;
        [Min(0f)] public float maxDistance = 7f;
        [Min(0f)] public float retreatSpeed = 1.7f;
        [Min(0f)] public float approachSpeed = 1.8f;
        [Min(0f)] public float sidestepSpeed = 1.6f;
        [Min(0f)] public float minShootDistance = 2f;
        [Min(0f)] public float maxRetreatTime = 1.2f;

        [Header("Attack")]
        [Range(0.3f, 0.8f)] public float aimDuration = 0.7f;
        [Min(0f)] public float lockBeforeFire = 0.25f;
        [Range(0f, 1f)] public float aimLeadFactor = 0.6f;
        [Min(0f)] public float recoveryDuration = 0.4f;
        [Min(0f)] public float shotCooldown = 1.4f;
        [Min(0f)] public float firstShotDelay = 0.4f;
        [Min(0f)] public float projectileSpeed = 6.5f;
        [Min(0f)] public float projectileRange = 9f;
        [Min(1)] public int attackDamage = 1;

        [Header("Search")]
        [Min(0f)] public float searchDuration = 2.5f;
        [Range(0f, 90f)] public float searchSweepAngle = 60f;
        [Min(0.1f)] public float searchSweepPeriod = 1.6f;
        [Min(0f)] public float hitAlertDistance = 4f;

        void OnValidate()
        {
            lockBeforeFire = Mathf.Min(lockBeforeFire, aimDuration);
            maxDistance = Mathf.Max(maxDistance, minDistance);
            minShootDistance = Mathf.Min(minShootDistance, minDistance);
        }
    }
}
