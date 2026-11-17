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
        [SerializeField] Button nextButton;
        [SerializeField] Button menuButton;

        void Awake()
        {
            RefCheck.Valid(this, game, nameof(game));
            RefCheck.Valid(this, panel, nameof(panel));
            if (retryButton != null) retryButton.onClick.AddListener(() => game.Restart());
            if (menuButton != null) menuButton.onClick.AddListener(() => game.ReturnToMenu());
            if (nextButton != null) nextButton.onClick.AddListener(() => LevelSession.TryStart(NextLevel()));
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
            bool hasNext = nextButton != null && NextLevel() != null;
            if (nextButton != null) nextButton.gameObject.SetActive(hasNext);
            panel.SetActive(true);
            Button selected = hasNext ? nextButton : retryButton;
            if (selected != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(selected.gameObject);
        }

        string ResultText()
        {
            int points = score != null ? score.Score : 0;
            if (goal == null) return $"SCORE {points}\nCOINS {(score != null ? score.Coins : 0)}";
            int seconds = Mathf.FloorToInt(goal.ElapsedTime);
            string time = goal.TimedOut ? "TIME UP" : $"TIME {seconds / 60}:{seconds % 60:00}";
            return $"{time}   TRY {goal.Try}\nSCORE {points}";
        }

        LevelData NextLevel()
        {
            if (goal == null || goal.Level == null || !goal.IsComplete) return null;
            LevelCatalog catalog = LevelCatalog.Instance;
            LevelData next = catalog != null ? catalog.Find(goal.Level.levelNumber + 1) : null;
            return next != null && SaveManager.IsUnlocked(next.levelNumber) ? next : null;
        }
    }
}
