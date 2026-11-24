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
        [SerializeField] Button effectsButton;
        [SerializeField] Text effectsLabel;
        [SerializeField] Button fpsButton;
        [SerializeField] Text fpsLabel;
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
            if (effectsButton != null) effectsButton.onClick.AddListener(() => JuiceFx.SetEnabled(!JuiceFx.Enabled));
            if (fpsButton != null) fpsButton.onClick.AddListener(() => SaveManager.SetShowFps(!SaveManager.Settings.showFps));
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        void OnEnable()
        {
            if (game != null) game.StateChanged += OnStateChanged;
            SaveManager.SettingsChanged += RefreshLabels;
            RefreshLabels();
        }

        void OnDisable()
        {
            if (game != null) game.StateChanged -= OnStateChanged;
            SaveManager.SettingsChanged -= RefreshLabels;
        }

        void RefreshLabels()
        {
            if (effectsLabel != null) effectsLabel.text = $"EFFECTS: {(JuiceFx.Enabled ? "ON" : "OFF")}";
            if (fpsLabel != null) fpsLabel.text = $"FPS: {(SaveManager.Settings.showFps ? "ON" : "OFF")}";
        }

        void Update()
        {
            if (game != null && KeyboardInput.PausePressed) game.TogglePause();
        }

        void OnStateChanged(GameState state)
        {
            bool paused = state == GameState.Paused;
            if (pausePanel != null) pausePanel.SetActive(paused);
            if (paused) RefreshLabels();
            if (gameplayControls != null)
                foreach (GameObject control in gameplayControls)
                    if (control != null) control.SetActive(state == GameState.Playing);

            if (paused && resumeButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
    }
}
