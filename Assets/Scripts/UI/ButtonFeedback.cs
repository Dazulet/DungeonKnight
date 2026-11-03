using UnityEngine;
using UnityEngine.EventSystems;

namespace DungeonKnight
{
  
    public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField, Range(0.5f, 1f)] float pressedScale = 0.93f;
        [SerializeField, Min(1f)] float speed = 25f;

        float target = 1f;

        void OnDisable()
        {
            target = 1f;
            transform.localScale = Vector3.one;
        }

        public void OnPointerDown(PointerEventData eventData) => target = pressedScale;
        public void OnPointerUp(PointerEventData eventData) => target = 1f;
        public void OnPointerExit(PointerEventData eventData) => target = 1f;

        void Update()
        {
            float s = Mathf.Lerp(transform.localScale.x, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            transform.localScale = new Vector3(s, s, 1f);
        }
    }
}
