using UnityEngine;

namespace DungeonKnight
{
    [CreateAssetMenu(menuName = "Dungeon Knight/Chaser Stats", fileName = "ChaserStats")]
    public class ChaserStats : EnemyStats
    {
        [Header("Movement")]
        [Min(0f)] public float moveSpeed = 1.7f;
        [Min(0f)] public float acceleration = 12f;
        [Min(0f)] public float wobble;

        [Header("Contact attack")]
        [Min(0f)] public float contactRange = 0.7f;
        [Min(1)] public int contactDamage = 1;
        [Min(0f)] public float attackCooldown = 0.9f;
        [Min(0f)] public float firstAttackDelay = 0.5f;
    }
}
