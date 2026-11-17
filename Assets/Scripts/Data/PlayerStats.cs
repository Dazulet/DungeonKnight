using UnityEngine;

namespace DungeonKnight
{
    [CreateAssetMenu(menuName = "Dungeon Knight/Player Stats", fileName = "PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Health")]
        [Min(1)] public int maxHearts = 3;
        [Min(0f)] public float invulnerableTime = 1.2f;

        [Header("Movement")]
        [Min(0f)] public float moveSpeed = 5f;
        [Min(0f)] public float acceleration = 40f;

        [Header("Shooting")]
        [Min(0.05f)] public float fireCooldown = 0.28f;
        [Min(0f)] public float autoAimRange = 9f;
        [Range(0f, 15f)] public float spreadDegrees = 3f;

        [Header("Magic bolt")]
        [Min(0f)] public float boltSpeed = 11f;
        [Min(1)] public int boltDamage = 1;
        [Min(0.1f)] public float boltLifetime = 1.6f;
        [Min(0f)] public float boltKnockback = 3f;
    }
}
