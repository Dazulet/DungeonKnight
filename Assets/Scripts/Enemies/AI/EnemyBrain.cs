using System;
using UnityEngine;

namespace DungeonKnight
{
    public abstract class EnemyBrain<TState> : MonoBehaviour where TState : struct, Enum
    {
        [SerializeField] protected PlayerHealth target;
        [SerializeField] protected EnemyHealth health;
        [SerializeField] protected EnemyMotor motor;
        [SerializeField] bool logStateChanges;
        [SerializeField] string debugState;

        EnemyStateMachine<TState> machine;

        protected abstract TState InitialState { get; }

        public TState State => machine != null ? machine.Current : InitialState;
        protected float TimeInState => machine != null ? machine.TimeInState : 0f;
        protected bool IsAlive => health == null || health.IsAlive;
        protected bool HasLivingTarget => target != null && target.IsAlive;
        protected Vector2 Position => motor.Position;
        protected Vector2 TargetPosition => target.transform.position;

        public void Init(PlayerHealth player) => target = player;

        protected virtual void Awake()
        {
            RefCheck.Valid(this, health, nameof(health));
            RefCheck.Valid(this, motor, nameof(motor));
            machine = new EnemyStateMachine<TState>(InitialState);
            debugState = InitialState.ToString();
        }

        void Update()
        {
            if (motor == null) return;
            if (!IsAlive)
            {
                motor.Stop();
                return;
            }
            Tick();
        }

        protected abstract void Tick();

        protected virtual void OnStateChanged(TState previous, TState next) { }

        protected bool ChangeState(TState next)
        {
            TState previous = machine.Current;
            if (!machine.TryChange(next)) return false;
            debugState = next.ToString();
            if (logStateChanges) Debug.Log($"{name}: {previous} -> {next}", this);
            OnStateChanged(previous, next);
            return true;
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
