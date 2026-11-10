using UnityEngine;

namespace DungeonKnight
{
    public class AttackTelegraph : MonoBehaviour
    {
        enum Shape { Circle, Line }

        [SerializeField] SpriteRenderer outline;
        [SerializeField] SpriteRenderer fill;
        [SerializeField] Sprite circleOutlineSprite;
        [SerializeField] Sprite circleFillSprite;
        [SerializeField] Sprite lineSprite;
        [SerializeField, Min(0.01f)] float lineWidth = 0.22f;
        [SerializeField] Color warningColor = new Color(1f, 0.22f, 0.18f, 0.9f);
        [SerializeField] Color lockedColor = new Color(1f, 0.82f, 0.25f, 1f);
        [SerializeField] Color strikeColor = new Color(1f, 0.95f, 0.85f, 1f);
        [SerializeField, Min(0f)] float strikeFlashDuration = 0.12f;
        [SerializeField, Min(0f)] float pulseSpeed = 16f;
        [SerializeField, Range(0f, 1f)] float pulseStrength = 0.25f;

        Shape shape;
        Vector2 anchor;
        float angle;
        float size;
        float width;
        float progress;
        bool locked;
        float flashTimeLeft;
        bool visible;

        public bool IsVisible => visible;

        void Awake()
        {
            RefCheck.Valid(this, outline, nameof(outline));
            RefCheck.Valid(this, fill, nameof(fill));
            if (outline != null && circleOutlineSprite == null) circleOutlineSprite = outline.sprite;
            if (fill != null && circleFillSprite == null) circleFillSprite = fill.sprite;
            SetRenderersEnabled(false);
        }

        public void ShowCircle(Vector2 center, float radius)
        {
            Show(Shape.Circle, center, 0f, radius * 2f, 0f);
        }

        public void ShowLine(Vector2 start, Vector2 direction, float length)
        {
            Show(Shape.Line, start, DirectionToAngle(direction), length, lineWidth);
        }

        public void ShowLane(Vector2 start, Vector2 direction, float length, float laneWidth)
        {
            Show(Shape.Line, start, DirectionToAngle(direction), length, laneWidth);
        }

        public void UpdateLine(Vector2 start, Vector2 direction, float length)
        {
            if (!visible || shape != Shape.Line) return;
            anchor = start;
            angle = DirectionToAngle(direction);
            size = length;
            ApplyLayout();
        }

        public void SetLocked(bool isLocked)
        {
            locked = isLocked;
            if (visible && flashTimeLeft <= 0f) ApplyColors();
        }

        public void SetProgress(float value)
        {
            if (!visible || flashTimeLeft > 0f) return;
            progress = Mathf.Clamp01(value);
            ApplyLayout();
            ApplyColors();
        }

        public void Flash()
        {
            if (!visible) return;
            flashTimeLeft = strikeFlashDuration;
            progress = 1f;
            ApplyLayout();
            outline.color = strikeColor;
            fill.color = strikeColor;
            if (strikeFlashDuration <= 0f) Hide();
        }

        public void Hide()
        {
            visible = false;
            locked = false;
            flashTimeLeft = 0f;
            SetRenderersEnabled(false);
        }

        void Show(Shape newShape, Vector2 position, float newAngle, float newSize, float newWidth)
        {
            if (outline == null || fill == null) return;
            shape = newShape;
            anchor = position;
            angle = newAngle;
            size = newSize;
            width = newWidth;
            progress = 0f;
            locked = false;
            flashTimeLeft = 0f;
            visible = true;
            bool isLine = shape == Shape.Line;
            outline.sprite = isLine ? lineSprite : circleOutlineSprite;
            fill.sprite = isLine ? lineSprite : circleFillSprite;
            SetRenderersEnabled(true);
            ApplyLayout();
            ApplyColors();
        }

        void LateUpdate()
        {
            if (!visible) return;
            transform.SetPositionAndRotation(anchor, Quaternion.Euler(0f, 0f, angle));
            if (flashTimeLeft <= 0f) return;
            flashTimeLeft -= Time.deltaTime;
            if (flashTimeLeft <= 0f) Hide();
        }

        void ApplyLayout()
        {
            transform.SetPositionAndRotation(anchor, Quaternion.Euler(0f, 0f, angle));
            if (shape == Shape.Circle)
            {
                outline.transform.localPosition = Vector3.zero;
                fill.transform.localPosition = Vector3.zero;
                outline.transform.localScale = ScaleFor(outline, size, size);
                fill.transform.localScale = ScaleFor(fill, size * progress, size * progress);
                return;
            }
            float filled = size * progress;
            outline.transform.localPosition = new Vector3(size * 0.5f, 0f, 0f);
            fill.transform.localPosition = new Vector3(filled * 0.5f, 0f, 0f);
            outline.transform.localScale = ScaleFor(outline, size, width);
            fill.transform.localScale = ScaleFor(fill, filled, width);
        }

        void ApplyColors()
        {
            Color baseColor = locked ? lockedColor : warningColor;
            float pulse = 1f - pulseStrength * (0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed));
            Color outlineColor = baseColor;
            outlineColor.a *= shape == Shape.Line ? 0.45f * pulse : pulse;
            outline.color = outlineColor;
            Color fillColor = baseColor;
            fillColor.a *= Mathf.Lerp(0.35f, 0.8f, progress);
            if (shape == Shape.Line) fillColor.a = baseColor.a;
            fill.color = fillColor;
        }

        void SetRenderersEnabled(bool show)
        {
            if (outline != null) outline.enabled = show;
            if (fill != null) fill.enabled = show;
        }

        static float DirectionToAngle(Vector2 direction) => Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        static Vector3 ScaleFor(SpriteRenderer renderer, float width, float height)
        {
            if (renderer.sprite == null) return new Vector3(width, height, 1f);
            Vector2 spriteSize = renderer.sprite.bounds.size;
            return new Vector3(width / spriteSize.x, height / spriteSize.y, 1f);
        }
    }
}
