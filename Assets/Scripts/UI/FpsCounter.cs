using UnityEngine;
using UnityEngine.UI;

namespace DungeonKnight
{
    public class FpsCounter : MonoBehaviour
    {
        public const int TargetFrameRate = 60;

        [SerializeField] Text label;
        [SerializeField, Min(0.1f)] float interval = 0.5f;

        int frames;
        float elapsed;
        float worstFrame;

        public float Fps { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void SetFrameRate() => Application.targetFrameRate = TargetFrameRate;

        void Awake() => RefCheck.Valid(this, label, nameof(label));

        void OnEnable()
        {
            SaveManager.SettingsChanged += Apply;
            Apply();
        }

        void OnDisable() => SaveManager.SettingsChanged -= Apply;

        void Apply()
        {
            if (label == null) return;
            bool show = SaveManager.Settings.showFps;
            label.enabled = show;
            if (show) label.text = "FPS --";
            frames = 0;
            elapsed = 0f;
            worstFrame = 0f;
        }

        void Update()
        {
            if (label == null || !label.enabled) return;
            float dt = Time.unscaledDeltaTime;
            frames++;
            elapsed += dt;
            worstFrame = Mathf.Max(worstFrame, dt);
            if (elapsed < interval) return;
            Fps = frames / elapsed;
            int low = worstFrame > 0f ? Mathf.RoundToInt(1f / worstFrame) : 0;
            label.text = $"FPS {Mathf.RoundToInt(Fps)}  LOW {low}  FX {(JuiceFx.Enabled ? "ON" : "OFF")}";
            frames = 0;
            elapsed = 0f;
            worstFrame = 0f;
        }
    }
}
