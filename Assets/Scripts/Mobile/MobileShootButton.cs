using UnityEngine;
using UnityEngine.EventSystems;

namespace DungeonKnight
{
    public class MobileShootButton : MonoBehaviour, IShootSource, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform visual;
        [SerializeField, Range(0.5f, 1f)] float pressedScale = 0.9f;

        int pointerId = int.MinValue;

        public bool IsShooting => pointerId != int.MinValue;

        void OnDisable() => Release();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsShooting) return;
            pointerId = eventData.pointerId;
            if (visual != null) visual.localScale = Vector3.one * pressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == pointerId) Release();
        }

        void Release()
        {
            pointerId = int.MinValue;
            if (visual != null) visual.localScale = Vector3.one;
        }
    }
}
