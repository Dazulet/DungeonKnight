using UnityEngine;

namespace DungeonKnight
{
    public static class SortOrder
    {
        public const int Floor = 0;
        public const int FloorDecor = 2;
        public const int Shadow = 5;
        public const int Projectile = 2500;
        public const int Glow = 3000;
        public const int Overlay = 5000;

        public static int ForY(float feetY) => 1000 - Mathf.RoundToInt(feetY * 16f);
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class YSorter : MonoBehaviour
    {
        [Tooltip("Distance from this transform down to the sprite's feet, in world units.")]
        [SerializeField] float feetOffset = -0.4f;
        [SerializeField] int orderOffset;

        SpriteRenderer spriteRenderer;

        void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

        void LateUpdate()
        {
            spriteRenderer.sortingOrder = SortOrder.ForY(transform.position.y + feetOffset) + orderOffset;
        }
    }
}
