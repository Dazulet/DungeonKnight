using UnityEngine;

namespace DungeonKnight
{
    [CreateAssetMenu(menuName = "Dungeon Knight/Charger Stats", fileName = "ChargerStats")]
    public class ChargerStats : SightedEnemyStats
    {
        [Header("Prowl")]
        [Min(0f)] public float prowlSpeed = 1.2f;
        [Min(0f)] public float prowlRadius = 2.2f;
        [Min(0f)] public float prowlPause = 1f;
        [Range(1f, 180f)] public float prowlTurnStep = 135f;

        [Header("Track")]
        [Min(0f)] public float trackSpeed = 1.5f;
        [Min(0f)] public float chargeTriggerRange = 5f;
        [Min(0f)] public float stalkDistance = 4f;

        [Header("Charge")]
        [Range(0.3f, 0.8f)] public float windUpDuration = 0.65f;
        [Min(0f)] public float chargeSpeed = 11f;
        [Min(0f)] public float chargeDistance = 6.5f;
        [Min(0f)] public float hitRadius = 0.6f;
        [Min(1)] public int attackDamage = 1;

        [Header("Recover")]
        [Min(0f)] public float recoveryDuration = 0.8f;
        [Min(0f)] public float wallStunDuration = 1f;
        [Min(0f)] public float chargeCooldown = 2f;
        [Min(0f)] public float hitAlertLookDuration = 1.2f;

        void OnValidate()
        {
            stalkDistance = Mathf.Min(stalkDistance, chargeTriggerRange);
        }
    }
}
