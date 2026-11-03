using UnityEngine;
using UnityEngine.UI;

namespace DungeonKnight
{

    public class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] Sprite[] frames;
        [SerializeField, Min(0f)] float fps = 8f;
        [SerializeField] bool loop = true;
        [SerializeField] bool destroyOnFinish;
        [SerializeField] bool unscaledTime;
        [SerializeField] bool randomizeStart;

        SpriteRenderer spriteRenderer;
        Image image;
        float time;
        int shownIndex = -1;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            image = GetComponent<Image>();
            if (spriteRenderer == null && image == null)
                Debug.LogError($"SpriteFlipbook on '{name}' needs a SpriteRenderer or an Image.", this);
            if (randomizeStart && HasFrames && fps > 0f) time = Random.value * frames.Length / fps;
            Show(CurrentIndex());
        }

        bool HasFrames => frames != null && frames.Length > 0;

        void Update()
        {
            if (!HasFrames || fps <= 0f) return;
            time += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float length = frames.Length / fps;
            if (time >= length)
            {
                if (loop)
                {
                    time %= length;
                }
                else if (destroyOnFinish)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            Show(CurrentIndex());
        }

        public void Play(Sprite[] newFrames, float newFps)
        {
            if (newFrames == frames) return;
            frames = newFrames;
            fps = newFps;
            time = 0f;
            shownIndex = -1;
            Show(CurrentIndex());
        }

        int CurrentIndex()
        {
            if (!HasFrames) return -1;
            int index = Mathf.FloorToInt(time * fps);
            return Mathf.Clamp(index, 0, frames.Length - 1);
        }

        void Show(int index)
        {
            if (index < 0 || index == shownIndex) return;
            shownIndex = index;
            if (spriteRenderer != null) spriteRenderer.sprite = frames[index];
            else if (image != null) image.sprite = frames[index];
        }
    }
}
