using UnityEngine;

namespace DungeonKnight
{
    public abstract class SightedEnemyStats : EnemyStats
    {
        [Header("Vision")]
        [Min(0f)] public float viewDistance = 5.5f;
        [Range(1f, 360f)] public float fieldOfView = 110f;
        [Min(1f)] public float alertRangeMultiplier = 1.6f;
        [Min(0f)] public float alertDuration = 3f;

        [Header("Fair play")]
        [Min(0f)] public float attackGracePeriod = 1.5f;
        [Range(0f, 0.25f)] public float attackScreenMargin = 0.03f;
    }
}
