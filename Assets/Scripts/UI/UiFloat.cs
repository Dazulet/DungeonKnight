using UnityEngine;

namespace DungeonKnight
{
    [RequireComponent(typeof(RectTransform))]
    public class UiFloat : MonoBehaviour
    {
        [SerializeField, Min(0f)] float amplitude = 10f;
        [SerializeField, Min(0f)] float period = 2.4f;
        [SerializeField, Range(0f, 1f)] float phase;

        RectTransform rect;
        Vector2 basePosition;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            basePosition = rect.anchoredPosition;
        }

        void Update()
        {
            if (period <= 0f) return;
            float t = (Time.unscaledTime / period + phase) * Mathf.PI * 2f;
            rect.anchoredPosition = basePosition + new Vector2(0f, Mathf.Round(Mathf.Sin(t) * amplitude));
        }
    }
}
