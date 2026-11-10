using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonKnight
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] GameManager game;
        [SerializeField] ScoreManager score;
        [SerializeField] LevelGoal goal;
        [SerializeField] GameState showOn = GameState.GameOver;
        [SerializeField] GameObject panel;
        [SerializeField] Text resultText;
        [SerializeField] Button retryButton;
        [SerializeField] Button menuButton;

        void Awake()
        {
            RefCheck.Valid(this, game, nameof(game));
            RefCheck.Valid(this, panel, nameof(panel));
            if (retryButton != null) retryButton.onClick.AddListener(() => game.Restart());
            if (menuButton != null) menuButton.onClick.AddListener(() => game.ReturnToMenu());
            if (panel != null) panel.SetActive(false);
        }

        void OnEnable()
        {
            if (game != null) game.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            if (game != null) game.StateChanged -= OnStateChanged;
        }

        void OnStateChanged(GameState state)
        {
            if (state != showOn || panel == null) return;
            if (resultText != null) resultText.text = ResultText();
            panel.SetActive(true);
            if (retryButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }

        string ResultText()
        {
            int points = score != null ? score.Score : 0;
            if (goal == null) return $"SCORE {points}\nCOINS {(score != null ? score.Coins : 0)}";
            int seconds = Mathf.FloorToInt(goal.ElapsedTime);
            return $"TIME {seconds / 60}:{seconds % 60:00}   TRY {goal.Try}\nSCORE {points}";
        }
    }
}
