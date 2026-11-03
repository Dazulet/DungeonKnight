using UnityEngine;
using UnityEngine.UI;

namespace DungeonKnight
{
    
    public class TorchFlicker : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.5f)] float scaleAmount = 0.08f;
        [SerializeField, Range(0f, 1f)] float alphaAmount = 0.25f;
        [SerializeField, Min(0f)] float speed = 3f;

        SpriteRenderer spriteRenderer;
        Graphic graphic;
        Vector3 baseScale;
        Color baseColor;
        float seed;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            graphic = GetComponent<Graphic>();
            baseScale = transform.localScale;
            baseColor = spriteRenderer != null ? spriteRenderer.color : graphic != null ? graphic.color : Color.white;
            seed = Random.value * 100f;
        }

        void Update()
        {
            float n = Mathf.PerlinNoise(seed, Time.unscaledTime * speed);
            transform.localScale = baseScale * (1f + (n - 0.5f) * 2f * scaleAmount);
            Color c = baseColor;
            c.a = baseColor.a * (1f - alphaAmount * n);
            if (spriteRenderer != null) spriteRenderer.color = c;
            else if (graphic != null) graphic.color = c;
        }
    }
}
