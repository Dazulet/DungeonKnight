using System;
using System.Collections;
using UnityEngine;

namespace DungeonKnight
{
    public enum GameState { Playing, Paused, GameOver }

    
    public class GameManager : MonoBehaviour
    {
        [SerializeField] PlayerHealth player;
        [SerializeField, Min(0f)] float gameOverDelay = 0.9f;

        bool playerDead;

        public GameState State { get; private set; } = GameState.Playing;
        public event Action<GameState> StateChanged;

        void Awake()
        {
            Time.timeScale = 1f;
            RefCheck.Valid(this, player, nameof(player));
        }

        void OnEnable()
        {
            if (player != null) player.Died += OnPlayerDied;
        }

        void OnDisable()
        {
            if (player != null) player.Died -= OnPlayerDied;
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        public void SetPaused(bool paused)
        {
            if (playerDead || State == GameState.GameOver) return;
            GameState target = paused ? GameState.Paused : GameState.Playing;
            if (target == State) return;
            State = target;
            Time.timeScale = paused ? 0f : 1f;
            StateChanged?.Invoke(State);
        }

        public void TogglePause() => SetPaused(State == GameState.Playing);

        public void Restart() => SceneLoader.ReloadCurrent();

        public void ReturnToMenu() => SceneLoader.Load(SceneNames.MainMenu);

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && State == GameState.Playing) SetPaused(true);
        }

        void OnPlayerDied()
        {
            if (playerDead) return;
            playerDead = true;
            if (State == GameState.Paused) SetPausedUnchecked(GameState.Playing);
            StartCoroutine(GameOverAfterDelay());
        }

        IEnumerator GameOverAfterDelay()
        {
            yield return new WaitForSecondsRealtime(gameOverDelay);
            SetPausedUnchecked(GameState.GameOver);
            Time.timeScale = 0f;
        }

        void SetPausedUnchecked(GameState state)
        {
            State = state;
            Time.timeScale = state == GameState.Playing ? 1f : 0f;
            StateChanged?.Invoke(State);
        }
    }
}
