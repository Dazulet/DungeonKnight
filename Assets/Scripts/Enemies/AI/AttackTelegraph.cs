using UnityEngine;

namespace DungeonKnight
{
    public class AttackTelegraph : MonoBehaviour
    {
        [SerializeField] SpriteRenderer outline;
        [SerializeField] SpriteRenderer fill;
        [SerializeField] Color warningColor = new Color(1f, 0.22f, 0.18f, 0.9f);
        [SerializeField] Color strikeColor = new Color(1f, 0.95f, 0.85f, 1f);
        [SerializeField, Min(0f)] float strikeFlashDuration = 0.12f;
        [SerializeField, Min(0f)] float pulseSpeed = 16f;
        [SerializeField, Range(0f, 1f)] float pulseStrength = 0.25f;

        Vector2 anchor;
        Vector3 fullFillScale = Vector3.one;
        float flashTimeLeft;
        bool visible;

        public bool IsVisible => visible;

        void Awake()
        {
            RefCheck.Valid(this, outline, nameof(outline));
            RefCheck.Valid(this, fill, nameof(fill));
            SetRenderersEnabled(false);
        }

        public void ShowCircle(Vector2 center, float radius)
        {
            if (outline == null || fill == null) return;
            anchor = center;
            transform.rotation = Quaternion.identity;
            float diameter = radius * 2f;
            outline.transform.localScale = ScaleFor(outline, diameter);
            fullFillScale = ScaleFor(fill, diameter);
            flashTimeLeft = 0f;
            visible = true;
            SetRenderersEnabled(true);
            SetProgress(0f);
            transform.position = anchor;
        }

        public void SetProgress(float progress)
        {
            if (!visible || flashTimeLeft > 0f) return;
            progress = Mathf.Clamp01(progress);
            fill.transform.localScale = fullFillScale * progress;
            float pulse = 1f - pulseStrength * (0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed));
            Color color = warningColor;
            color.a *= pulse;
            outline.color = color;
            Color fillColor = warningColor;
            fillColor.a *= Mathf.Lerp(0.35f, 0.8f, progress);
            fill.color = fillColor;
        }

        public void Flash()
        {
            if (!visible) return;
            flashTimeLeft = strikeFlashDuration;
            fill.transform.localScale = fullFillScale;
            outline.color = strikeColor;
            fill.color = strikeColor;
            if (strikeFlashDuration <= 0f) Hide();
        }

        public void Hide()
        {
            visible = false;
            flashTimeLeft = 0f;
            SetRenderersEnabled(false);
        }

        void LateUpdate()
        {
            if (!visible) return;
            transform.position = anchor;
            if (flashTimeLeft <= 0f) return;
            flashTimeLeft -= Time.deltaTime;
            if (flashTimeLeft <= 0f) Hide();
        }

        void SetRenderersEnabled(bool show)
        {
            if (outline != null) outline.enabled = show;
            if (fill != null) fill.enabled = show;
        }

        static Vector3 ScaleFor(SpriteRenderer renderer, float size)
        {
            if (renderer.sprite == null) return Vector3.one * size;
            Vector2 spriteSize = renderer.sprite.bounds.size;
            return new Vector3(size / spriteSize.x, size / spriteSize.y, 1f);
        }
    }
}
