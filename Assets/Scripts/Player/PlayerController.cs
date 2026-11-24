using UnityEngine;

namespace DungeonKnight
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] PlayerStats stats;
        [SerializeField] PlayerInputReader input;
        [SerializeField] PlayerHealth health;
        [SerializeField] SpriteRenderer body;
        [SerializeField] SpriteFlipbook animator;
        [SerializeField] Sprite[] idleFrames;
        [SerializeField] Sprite[] walkFrames;
        [SerializeField, Min(0.05f)] float stepInterval = 0.3f;

        Rigidbody2D rb;
        Vector2 velocity;
        float stepTimer;

        public Vector2 Facing { get; private set; } = Vector2.right;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            RefCheck.Valid(this, stats, nameof(stats));
            RefCheck.Valid(this, input, nameof(input));
            RefCheck.Valid(this, health, nameof(health));
            RefCheck.Valid(this, body, nameof(body));
            RefCheck.Valid(this, animator, nameof(animator));
        }

        void Update()
        {
            if (input == null || body == null || animator == null) return;
            bool alive = health == null || health.IsAlive;
            Vector2 move = alive ? input.Move : Vector2.zero;
            if (move.sqrMagnitude > 0.01f)
            {
                Facing = move.normalized;
                if (Mathf.Abs(move.x) > 0.1f) body.flipX = move.x < 0f;
            }
            bool walking = move.sqrMagnitude > 0.01f;
            animator.Play(walking ? walkFrames : idleFrames, walking ? 10f : 3f);
            UpdateSteps(walking);
        }

        void UpdateSteps(bool walking)
        {
            if (!walking)
            {
                stepTimer = 0f;
                return;
            }
            if (Time.deltaTime <= 0f) return;
            stepTimer -= Time.deltaTime;
            if (stepTimer > 0f) return;
            stepTimer = stepInterval;
            GameAudio.Play(SoundId.Footstep);
        }

        void FixedUpdate()
        {
            if (input == null || stats == null) return;
            bool alive = health == null || health.IsAlive;
            Vector2 target = alive ? Vector2.ClampMagnitude(input.Move, 1f) * stats.moveSpeed : Vector2.zero;
            velocity = Vector2.MoveTowards(velocity, target, stats.acceleration * Time.fixedDeltaTime);
            rb.SetVelocity(velocity);
        }

        public void FaceTowards(Vector2 direction)
        {
            if (body != null && Mathf.Abs(direction.x) > 0.05f) body.flipX = direction.x < 0f;
        }
    }
}
