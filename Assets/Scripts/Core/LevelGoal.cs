using System;
using UnityEngine;

namespace DungeonKnight
{
    public class LevelGoal : MonoBehaviour
    {
        [SerializeField] GameManager game;
        [SerializeField] ScoreManager score;

        float finishTime = -1f;
        int defeated;

        public LevelData Level { get; private set; }
        public int Total { get; private set; }
        public int Remaining => Mathf.Max(0, Total - defeated);
        public int Try { get; private set; }
        public bool IsComplete { get; private set; }
        public bool TimedOut { get; private set; }
        public bool IsNewBest { get; private set; }
        public float ElapsedTime => IsComplete ? finishTime : Time.timeSinceLevelLoad;
        public float TimeLimit => Level != null ? Level.timeLimit : 0f;
        public float TimeLeft => Mathf.Max(0f, TimeLimit - ElapsedTime);

        public event Action<int> RemainingChanged;

        void Awake()
        {
            RefCheck.Valid(this, game, nameof(game));
            RefCheck.Valid(this, score, nameof(score));
        }

        public void Begin(LevelData level, int enemyCount)
        {
            Level = level;
            Total = enemyCount;
            defeated = 0;
            Try = TryCounter.BeginAttempt(level.levelNumber);
            if (Total == 0) Debug.LogError($"{level.name} has no enemies to defeat.", level);
            RemainingChanged?.Invoke(Remaining);
        }

        public void Track(EnemyHealth enemy)
        {
            if (enemy != null) enemy.Died += OnEnemyDied;
        }

        void Update()
        {
            if (Level == null || IsComplete || TimedOut || TimeLimit <= 0f) return;
            if (ElapsedTime < TimeLimit) return;
            TimedOut = game != null && game.FailLevel();
            enabled = false;
        }

        void OnEnemyDied(EnemyHealth enemy)
        {
            enemy.Died -= OnEnemyDied;
            defeated++;
            RemainingChanged?.Invoke(Remaining);
            if (Remaining == 0) Complete();
        }

        void Complete()
        {
            if (IsComplete || TimedOut || game == null || !game.CompleteLevel()) return;
            IsComplete = true;
            finishTime = Time.timeSinceLevelLoad;
            TryCounter.Reset();
            IsNewBest = SaveManager.CompleteLevel(Level.levelNumber, score != null ? score.Score : 0, finishTime);
        }
    }
}
