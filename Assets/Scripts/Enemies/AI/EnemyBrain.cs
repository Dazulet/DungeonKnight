using System;
using UnityEngine;

namespace DungeonKnight
{
    public abstract class EnemyBrain<TState> : MonoBehaviour, ILevelEnemy where TState : struct, Enum
    {
        [SerializeField] protected PlayerHealth target;
        [SerializeField] protected EnemyHealth health;
        [SerializeField] protected EnemyMotor motor;
        [SerializeField] protected EnemyAlertIcon alertIcon;
        [SerializeField] HitFlash hitFlash;
        [SerializeField] EnemyStateLabel stateLabel;
        [SerializeField] Color windUpTint = new Color(1f, 0.55f, 0.45f, 1f);
        [SerializeField] bool logStateChanges;
        [SerializeField] string debugState;

        EnemyStateMachine<TState> machine;
        Camera viewCamera;
        bool hitPending;
        Vector2 pendingHitDirection;

        protected abstract TState InitialState { get; }
        protected abstract SightedEnemyStats SightStats { get; }

        public TState State => machine != null ? machine.Current : InitialState;
        protected float TimeInState => machine != null ? machine.TimeInState : 0f;
        protected bool IsAlive => health == null || health.IsAlive;
        protected bool HasLivingTarget => target != null && target.IsAlive;
        protected Vector2 Position => motor.Position;
        protected Vector2 TargetPosition => target.transform.position;

        protected bool CanStartAttack => HasLivingTarget && Time.timeSinceLevelLoad >= SightStats.attackGracePeriod
            && IsOnScreen(Position) && IsOnScreen(TargetPosition);

        public virtual void Configure(PlayerHealth player, EnemySpawn spawn) => target = player;

        protected bool IsOnScreen(Vector2 point)
        {
            if (viewCamera == null) viewCamera = Camera.main;
            if (viewCamera == null) return true;
            Vector3 viewport = viewCamera.WorldToViewportPoint(point);
            float margin = SightStats.attackScreenMargin;
            return viewport.x >= margin && viewport.x <= 1f - margin
                && viewport.y >= margin && viewport.y <= 1f - margin;
        }

        protected virtual void Awake()
        {
            RefCheck.Valid(this, health, nameof(health));
            RefCheck.Valid(this, motor, nameof(motor));
            machine = new EnemyStateMachine<TState>(InitialState);
            debugState = InitialState.ToString();
            if (stateLabel != null) stateLabel.SetState(debugState);
        }

        protected virtual void OnEnable()
        {
            if (health != null) health.Damaged += OnDamaged;
        }

        protected virtual void OnDisable()
        {
            if (health != null) health.Damaged -= OnDamaged;
        }

        void Update()
        {
            if (motor == null || SightStats == null) return;
            if (!IsAlive)
            {
                motor.Stop();
                return;
            }
            if (hitPending)
            {
                hitPending = false;
                OnHit(pendingHitDirection);
            }
            Tick();
        }

        protected abstract void Tick();

        protected virtual void OnStateChanged(TState previous, TState next) { }

        protected virtual void OnHit(Vector2 towardShooter) { }

        void OnDamaged(Vector2 push)
        {
            if (!IsAlive || machine == null || push.sqrMagnitude < 0.0001f) return;
            pendingHitDirection = -push.normalized;
            hitPending = true;
        }

        protected bool ChangeState(TState next)
        {
            TState previous = machine.Current;
            if (!machine.TryChange(next)) return false;
            debugState = next.ToString();
            if (stateLabel != null) stateLabel.SetState(debugState);
            if (logStateChanges) Debug.Log($"{name}: {previous} -> {next}", this);
            OnStateChanged(previous, next);
            return true;
        }

        protected void RestartState() => machine.Restart();

        protected void IconAlert()
        {
            if (alertIcon != null) alertIcon.ShowAlert();
        }

        protected void IconSuspicious(float duration)
        {
            if (alertIcon != null) alertIcon.ShowSuspicious(duration);
        }

        protected void IconClear()
        {
            if (alertIcon != null) alertIcon.Clear();
        }

        protected void TintBody(Color color)
        {
            if (hitFlash != null) hitFlash.SetBaseColor(color);
        }

        protected void ShowWindUpTint() => TintBody(windUpTint);

        protected void ClearBodyTint()
        {
            if (hitFlash != null) hitFlash.ResetBaseColor();
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (!Application.isPlaying || machine == null) return;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.9f, machine.Current.ToString());
        }
#endif
    }
}
