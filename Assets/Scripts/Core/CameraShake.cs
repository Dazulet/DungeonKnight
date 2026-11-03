using UnityEngine;

namespace DungeonKnight
{
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] PlayerHealth player;
        [SerializeField, Min(0f)] float hitMagnitude = 0.15f;
        [SerializeField, Min(0f)] float deathMagnitude = 0.35f;
        [SerializeField, Min(0.01f)] float duration = 0.2f;

        Vector3 basePosition;
        float timeLeft;
        float magnitude;

        void Awake() => basePosition = transform.localPosition;

        void OnEnable()
        {
            if (!RefCheck.Valid(this, player, nameof(player))) return;
            player.Damaged += OnDamaged;
            player.Died += OnDied;
        }

        void OnDisable()
        {
            if (player == null) return;
            player.Damaged -= OnDamaged;
            player.Died -= OnDied;
        }

        void OnDamaged() => Shake(hitMagnitude);

        void OnDied() => Shake(deathMagnitude);

        public void Shake(float strength)
        {
            magnitude = Mathf.Max(magnitude * (timeLeft / duration), strength);
            timeLeft = duration;
        }

        void LateUpdate()
        {
            if (timeLeft <= 0f) return;
            timeLeft -= Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(timeLeft / duration);
            Vector2 offset = Random.insideUnitCircle * magnitude * k;
            transform.localPosition = timeLeft > 0f ? basePosition + (Vector3)offset : basePosition;
        }
    }
}
