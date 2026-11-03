using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonKnight.EditorTools
{
    public static class UiFactory
    {
        public const int UiLayer = 5;

        const float SlicedPixelMultiplier = 100f / 96f;

        public static readonly Color TextColor = new Color(0.96f, 0.93f, 0.85f);
        public static readonly Color Gold = new Color(1f, 0.82f, 0.3f);
        public static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        public static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        public static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);
        public static readonly Vector2 TopRight = new Vector2(1f, 1f);
        public static readonly Vector2 BottomLeft = new Vector2(0f, 0f);
        public static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);
        public static readonly Vector2 BottomRight = new Vector2(1f, 0f);

        public static Canvas Canvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UiLayer };
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            scaler.referencePixelsPerUnit = 100f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static RectTransform SafeArea(Transform canvas)
        {
            RectTransform rt = Stretch(canvas, "SafeArea");
            rt.gameObject.AddComponent<SafeAreaController>();
            return rt;
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, Vector2? pivot = null)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UiLayer };
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot ?? anchor;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Area(Transform parent, string name, Vector2 min, Vector2 max)
        {
            RectTransform rt = Rect(parent, name, Center, Vector2.zero, Vector2.zero, Center);
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static RectTransform AreaAround(Transform parent, string name, Vector2 center, Vector2 size)
            => Area(parent, name, center - size * 0.5f, center + size * 0.5f);

        public static RectTransform Stretch(Transform parent, string name) => Area(parent, name, Vector2.zero, Vector2.one);

        public static Image Image(RectTransform rt, Sprite sprite, Color? color = null, bool raycast = false)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color ?? Color.white;
            image.raycastTarget = raycast;
            if (sprite != null && sprite.border != Vector4.zero)
            {
                image.type = UnityEngine.UI.Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = SlicedPixelMultiplier;
            }
            return image;
        }

        public static Image TouchZone(RectTransform rt) => Image(rt, null, new Color(0f, 0f, 0f, 0f), raycast: true);

        public static Text Text(Transform parent, string name, string content, int size, Vector2 anchor, Vector2 position,
            Vector2 rectSize, TextAnchor alignment = TextAnchor.MiddleCenter, Color? color = null, float outline = 4f)
        {
            RectTransform rt = Rect(parent, name, anchor, position, rectSize);
            var text = rt.gameObject.AddComponent<Text>();
            text.font = BuilderUtil.PixelFont();
            text.text = content;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color ?? TextColor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            if (outline > 0f)
            {
                var shadow = rt.gameObject.AddComponent<Outline>();
                shadow.effectColor = new Color(0.05f, 0.03f, 0.08f, 1f);
                shadow.effectDistance = new Vector2(outline, -outline);
            }
            return text;
        }

        static Button MakeButton(RectTransform rt)
        {
            Image image = Image(rt, BuilderUtil.Sprite("UI/ui_button"), raycast: true);
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.95f, 0.82f);
            colors.selectedColor = new Color(1f, 0.9f, 0.6f);
            colors.pressedColor = new Color(0.72f, 0.66f, 0.62f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            colors.fadeDuration = 0.06f;
            button.colors = colors;
            rt.gameObject.AddComponent<ButtonFeedback>();
            return button;
        }

        public static Button Button(Transform parent, string name, string label, Vector2 anchor, Vector2 position,
            Vector2 size, int fontSize = 60)
        {
            RectTransform rt = Rect(parent, name, anchor, position, size);
            Button button = MakeButton(rt);
            Text(rt, "Label", label, fontSize, Center, new Vector2(0f, 3f), size);
            return button;
        }

        public static Button IconButton(Transform parent, string name, Sprite icon, Vector2 anchor, Vector2 position,
            Vector2 size, Vector2 iconSize)
        {
            RectTransform rt = Rect(parent, name, anchor, position, size);
            Button button = MakeButton(rt);
            Image(Rect(rt, "Icon", Center, Vector2.zero, iconSize), icon);
            return button;
        }

        public static GameObject EventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            return go;
        }

        public static Camera Camera(Color background, float size)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0f, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = size;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 50f;
            go.AddComponent<AudioListener>();
            return cam;
        }
    }
}
