using UnityEngine;

namespace DungeonKnight
{
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] SpriteRenderer target;
        [SerializeField] Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);
        [SerializeField, Min(0.01f)] float duration = 0.12f;
        [SerializeField, Range(0f, 0.5f)] float squash = 0.2f;

        Color baseColor;
        Vector3 baseScale;
        float timeLeft;

        void Awake()
        {
            if (!RefCheck.Valid(this, target, nameof(target))) return;
            baseColor = target.color;
            baseScale = target.transform.localScale;
        }

        public void Flash() => timeLeft = duration;

        public void SetBaseColor(Color color)
        {
            baseColor = color;
            if (target != null && timeLeft <= 0f) target.color = color;
        }

        void Update()
        {
            if (target == null || timeLeft <= 0f) return;
            timeLeft -= Time.deltaTime;
            float k = Mathf.Clamp01(timeLeft / duration);
            target.color = Color.Lerp(baseColor, flashColor, k);
            target.transform.localScale = new Vector3(
                baseScale.x * (1f + squash * k),
                baseScale.y * (1f - squash * k),
                baseScale.z);
        }
    }
}
