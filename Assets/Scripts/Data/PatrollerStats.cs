using UnityEngine;

namespace DungeonKnight
{
    [CreateAssetMenu(menuName = "Dungeon Knight/Patroller Stats", fileName = "PatrollerStats")]
    public class PatrollerStats : SightedEnemyStats
    {
        [Header("Movement")]
        [Min(0f)] public float patrolSpeed = 1.4f;
        [Min(0f)] public float chaseSpeed = 2.6f;
        [Min(0f)] public float waypointPause = 0.6f;
        [Min(0f)] public float searchSpeed = 1.8f;

        [Header("Attack")]
        [Min(0f)] public float attackRange = 1.3f;
        [Min(0f)] public float holdDistance = 0.8f;
        [Min(0f)] public float strikeReach = 0.7f;
        [Min(0.1f)] public float strikeRadius = 0.85f;
        [Min(1)] public int attackDamage = 1;
        [Range(0.3f, 0.8f)] public float windUpDuration = 0.55f;
        [Min(0f)] public float recoveryDuration = 0.45f;
        [Min(0f)] public float attackCooldown = 1.1f;

        [Header("Search")]
        [Min(0f)] public float lookAroundDuration = 2.2f;
        [Min(0.05f)] public float lookTurnInterval = 0.55f;
        [Range(0f, 180f)] public float lookTurnAngle = 90f;
        [Min(0f)] public float hitInvestigateDistance = 3f;

        void OnValidate()
        {
            holdDistance = Mathf.Min(holdDistance, attackRange);
        }
    }
}
