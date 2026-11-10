using UnityEngine;

namespace DungeonKnight
{
    public class EnemyAlertIcon : MonoBehaviour
    {
        [SerializeField] SpriteRenderer icon;
        [SerializeField] Sprite alertSprite;
        [SerializeField] Sprite suspiciousSprite;
        [SerializeField, Min(0f)] float alertDuration = 0.8f;
        [SerializeField, Min(0f)] float popDuration = 0.15f;
        [SerializeField, Min(1f)] float popScale = 1.6f;

        Vector3 baseScale = Vector3.one;
        float alertUntil;
        float suspiciousUntil;
        float popTimeLeft;

        void Awake()
        {
            RefCheck.Valid(this, icon, nameof(icon));
            if (icon != null)
            {
                baseScale = icon.transform.localScale;
                icon.enabled = false;
            }
        }

        public void ShowAlert()
        {
            alertUntil = Time.time + alertDuration;
            suspiciousUntil = 0f;
            popTimeLeft = popDuration;
        }

        public void ShowSuspicious(float duration)
        {
            if (Time.time < alertUntil) return;
            suspiciousUntil = Time.time + duration;
            popTimeLeft = popDuration;
        }

        public void Clear()
        {
            alertUntil = 0f;
            suspiciousUntil = 0f;
        }

        void LateUpdate()
        {
            if (icon == null) return;
            bool alert = Time.time < alertUntil;
            bool suspicious = !alert && Time.time < suspiciousUntil;
            icon.enabled = alert || suspicious;
            if (!icon.enabled) return;
            icon.sprite = alert ? alertSprite : suspiciousSprite;
            popTimeLeft = Mathf.Max(0f, popTimeLeft - Time.deltaTime);
            float k = popDuration > 0f ? popTimeLeft / popDuration : 0f;
            icon.transform.localScale = baseScale * Mathf.Lerp(1f, popScale, k);
        }
    }
}
