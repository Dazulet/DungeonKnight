using UnityEngine;

namespace DungeonKnight
{
    public class HitFlash : MonoBehaviour
    {
        static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        [SerializeField] SpriteRenderer target;
        [SerializeField] Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);
        [SerializeField, Min(0.01f)] float duration = 0.12f;
        [SerializeField, Range(0f, 0.5f)] float squash = 0.2f;

        Color baseColor;
        Color initialColor;
        Vector3 baseScale;
        float timeLeft;
        bool hasFlashShader;
        MaterialPropertyBlock block;

        public bool UsesFlashShader => hasFlashShader;
        public float FlashAmount { get; private set; }

        void Awake()
        {
            if (!RefCheck.Valid(this, target, nameof(target))) return;
            baseColor = target.color;
            initialColor = baseColor;
            baseScale = target.transform.localScale;
            Material material = target.sharedMaterial;
            hasFlashShader = material != null && material.HasProperty(FlashAmountId);
            if (hasFlashShader) block = new MaterialPropertyBlock();
        }

        public void Flash() => timeLeft = duration;

        public void ResetBaseColor() => SetBaseColor(initialColor);

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
            bool whiteFlash = hasFlashShader && JuiceFx.Enabled;
            target.color = whiteFlash ? baseColor : Color.Lerp(baseColor, flashColor, k);
            SetFlashAmount(whiteFlash ? k : 0f);
            target.transform.localScale = new Vector3(
                baseScale.x * (1f + squash * k),
                baseScale.y * (1f - squash * k),
                baseScale.z);
        }

        void SetFlashAmount(float amount)
        {
            if (!hasFlashShader || Mathf.Approximately(amount, FlashAmount)) return;
            FlashAmount = amount;
            target.GetPropertyBlock(block);
            block.SetFloat(FlashAmountId, amount);
            target.SetPropertyBlock(block);
        }
    }
}
