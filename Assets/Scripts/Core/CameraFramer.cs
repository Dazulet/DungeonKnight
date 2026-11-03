using UnityEngine;

namespace DungeonKnight
{
 
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraFramer : MonoBehaviour
    {
        [Tooltip("Half of the arena height that must stay visible, plus margin.")]
        [SerializeField, Min(1f)] float minHalfHeight = 7f;
        [Tooltip("Half of the arena width that must stay visible, plus margin.")]
        [SerializeField, Min(1f)] float minHalfWidth = 11f;
        [SerializeField] SpriteRenderer screenOverlay;

        Camera cam;

        void OnEnable() => cam = GetComponent<Camera>();

        void LateUpdate()
        {
            if (cam == null || cam.aspect <= 0f) return;
            float size = Mathf.Max(minHalfHeight, minHalfWidth / cam.aspect);
            cam.orthographicSize = size;

            if (screenOverlay != null && screenOverlay.sprite != null)
            {
                Vector2 spriteSize = screenOverlay.sprite.bounds.size;
                float viewH = size * 2f;
                float viewW = viewH * cam.aspect;
                screenOverlay.transform.localScale = new Vector3(viewW / spriteSize.x, viewH / spriteSize.y, 1f);
            }
        }
    }
}
