using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonKnight
{
 
    public class MobileJoystick : MonoBehaviour, IMoveSource, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform stickBase;
        [SerializeField] RectTransform knob;
        [SerializeField, Range(0f, 0.5f)] float deadZone = 0.12f;
        [SerializeField] Graphic baseGraphic;
        [SerializeField, Range(0f, 1f)] float idleAlpha = 0.55f;
        [SerializeField, Range(0f, 1f)] float activeAlpha = 0.9f;

        int pointerId = int.MinValue;
        Vector2 value;

        public Vector2 Move => value;
        bool Active => pointerId != int.MinValue;

        void Awake()
        {
            RefCheck.Valid(this, stickBase, nameof(stickBase));
            RefCheck.Valid(this, knob, nameof(knob));
            SetAlpha(idleAlpha);
        }

        void OnDisable() => Release();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Active) return;
            pointerId = eventData.pointerId;
            SetAlpha(activeAlpha);
            UpdateStick(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == pointerId) UpdateStick(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == pointerId) Release();
        }

        void UpdateStick(PointerEventData eventData)
        {
            if (stickBase == null || knob == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    stickBase, eventData.position, eventData.pressEventCamera, out Vector2 local))
                return;

            float radius = stickBase.rect.width * 0.5f;
            Vector2 offset = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = offset;

            Vector2 normalized = offset / radius;
            float magnitude = normalized.magnitude;
            value = magnitude < deadZone
                ? Vector2.zero
                : normalized.normalized * Mathf.InverseLerp(deadZone, 1f, magnitude);
        }

        void Release()
        {
            pointerId = int.MinValue;
            value = Vector2.zero;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
            SetAlpha(idleAlpha);
        }

        void SetAlpha(float alpha)
        {
            if (baseGraphic == null) return;
            Color c = baseGraphic.color;
            c.a = alpha;
            baseGraphic.color = c;
        }
    }
}
