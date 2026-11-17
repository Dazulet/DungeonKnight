using UnityEngine;

namespace DungeonKnight
{
    public abstract class EnemyStats : ScriptableObject
    {
        [Header("Health and reward")]
        [Min(1)] public int maxHealth = 3;
        [Min(0)] public int scoreValue = 100;
        [Range(0f, 1f)] public float coinDropChance = 0.4f;
    }
}
