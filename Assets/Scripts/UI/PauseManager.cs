using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonKnight
{
    
    public class PauseManager : MonoBehaviour
    {
        [SerializeField] GameManager game;
        [SerializeField] GameObject pausePanel;
        [SerializeField] Button pauseButton;
        [SerializeField] Button resumeButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button menuButton;
        [Tooltip("HUD controls hidden while paused or after game over.")]
        [SerializeField] GameObject[] gameplayControls;

        void Awake()
        {
            RefCheck.Valid(this, game, nameof(game));
            RefCheck.Valid(this, pausePanel, nameof(pausePanel));
            if (pauseButton != null) pauseButton.onClick.AddListener(() => game.SetPaused(true));
            if (resumeButton != null) resumeButton.onClick.AddListener(() => game.SetPaused(false));
            if (restartButton != null) restartButton.onClick.AddListener(() => game.Restart());
            if (menuButton != null) menuButton.onClick.AddListener(() => game.ReturnToMenu());
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        void OnEnable()
        {
            if (game != null) game.StateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            if (game != null) game.StateChanged -= OnStateChanged;
        }

        void Update()
        {
            if (game != null && KeyboardInput.PausePressed) game.TogglePause();
        }

        void OnStateChanged(GameState state)
        {
            bool paused = state == GameState.Paused;
            if (pausePanel != null) pausePanel.SetActive(paused);
            if (gameplayControls != null)
                foreach (GameObject control in gameplayControls)
                    if (control != null) control.SetActive(state == GameState.Playing);

            if (paused && resumeButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
    }
}
