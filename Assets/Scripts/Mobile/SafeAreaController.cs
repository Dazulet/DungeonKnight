using UnityEngine;

namespace DungeonKnight
{
 
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaController : MonoBehaviour
    {
        RectTransform rect;
        Rect appliedArea;
        Vector2Int appliedSize;

        void OnEnable()
        {
            rect = GetComponent<RectTransform>();
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != appliedArea || Screen.width != appliedSize.x || Screen.height != appliedSize.y)
                Apply();
        }

        void Apply()
        {
            if (rect == null || Screen.width <= 0 || Screen.height <= 0) return;
            Rect area = Screen.safeArea;
            appliedArea = area;
            appliedSize = new Vector2Int(Screen.width, Screen.height);

            Vector2 min = area.position;
            Vector2 max = area.position + area.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
