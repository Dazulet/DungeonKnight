using UnityEngine;

namespace DungeonKnight
{
    public class HitStop : MonoBehaviour
    {
        [SerializeField] GameManager game;
        [SerializeField, Min(0f)] float duration = 0.05f;

        static HitStop current;

        float resumeAt;

        public bool IsActive { get; private set; }
        public static bool Running => current != null && current.IsActive;

        void Awake() => RefCheck.Valid(this, game, nameof(game));

        void OnEnable()
        {
            current = this;
            if (game != null) game.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            if (game != null) game.StateChanged -= OnStateChanged;
            End();
            if (current == this) current = null;
        }

        public static void Trigger()
        {
            if (current != null) current.Begin();
        }

        public void Begin()
        {
            if (!JuiceFx.Enabled || duration <= 0f || game == null || game.State != GameState.Playing) return;
            if (!IsActive && Time.timeScale <= 0f) return;
            IsActive = true;
            resumeAt = Time.unscaledTime + duration;
            Time.timeScale = 0f;
        }

        void Update()
        {
            if (IsActive && Time.unscaledTime >= resumeAt) End();
        }

        void End()
        {
            if (!IsActive) return;
            IsActive = false;
            if (game != null && game.State == GameState.Playing) Time.timeScale = 1f;
        }

        void OnStateChanged(GameState state) => IsActive = false;
    }
}
