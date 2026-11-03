using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonKnight
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] GameManager game;
        [SerializeField] ScoreManager score;
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
            if (state != GameState.GameOver || panel == null) return;
            if (resultText != null && score != null)
                resultText.text = $"SCORE {score.Score}\nCOINS {score.Coins}";
            panel.SetActive(true);
            if (retryButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }
    }
}
