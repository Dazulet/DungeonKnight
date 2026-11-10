using System;
using UnityEngine;

namespace DungeonKnight
{
    public class LevelGoal : MonoBehaviour
    {
        [SerializeField] GameManager game;
        [SerializeField] EnemyHealth[] enemies;

        float finishTime = -1f;

        public int Remaining { get; private set; }
        public int Total { get; private set; }
        public int Try { get; private set; }
        public bool IsComplete { get; private set; }
        public float ElapsedTime => IsComplete ? finishTime : Time.timeSinceLevelLoad;

        public event Action<int> RemainingChanged;

        void Awake()
        {
            RefCheck.Valid(this, game, nameof(game));
            Try = TryCounter.BeginAttempt();
            if (enemies == null || enemies.Length == 0) Debug.LogError("LevelGoal has no enemies to defeat.", this);
        }

        void Start()
        {
            if (enemies == null) return;
            foreach (EnemyHealth enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                enemy.Died += OnEnemyDied;
                Remaining++;
            }
            Total = Remaining;
            RemainingChanged?.Invoke(Remaining);
        }

        void OnEnemyDied(EnemyHealth enemy)
        {
            enemy.Died -= OnEnemyDied;
            Remaining = Mathf.Max(0, Remaining - 1);
            RemainingChanged?.Invoke(Remaining);
            if (Remaining == 0) Complete();
        }

        void Complete()
        {
            if (IsComplete || game == null || !game.CompleteLevel()) return;
            IsComplete = true;
            finishTime = Time.timeSinceLevelLoad;
            TryCounter.Reset();
        }
    }
}
