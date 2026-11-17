using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonKnight
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] Button playButton;
        [SerializeField] Button endlessButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button quitButton;
        [SerializeField] LevelSelectPanel levelSelect;
        [SerializeField] SettingsPanel settings;
        [SerializeField] Text saveNotice;

        static bool noticeShown;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLaunch() => noticeShown = false;

        void Awake()
        {
            Time.timeScale = 1f;
            if (RefCheck.Valid(this, playButton, nameof(playButton)))
                playButton.onClick.AddListener(Play);
            if (RefCheck.Valid(this, endlessButton, nameof(endlessButton)))
                endlessButton.onClick.AddListener(PlayEndless);
            if (RefCheck.Valid(this, settingsButton, nameof(settingsButton)))
                settingsButton.onClick.AddListener(OpenSettings);
            if (RefCheck.Valid(this, quitButton, nameof(quitButton)))
                quitButton.onClick.AddListener(Quit);
            if (RefCheck.Valid(this, settings, nameof(settings)))
                settings.Closed += () => Select(settingsButton);
            if (RefCheck.Valid(this, levelSelect, nameof(levelSelect)))
                levelSelect.Closed += () => Select(playButton);
        }

        void Start()
        {
            Select(playButton);
            ShowSaveNotice();
        }

        void Update()
        {
            if (!KeyboardInput.PausePressed) return;
            if (settings != null && settings.IsOpen) settings.Close();
            else if (levelSelect != null && levelSelect.IsOpen) levelSelect.Close();
            else Quit();
        }

        void ShowSaveNotice()
        {
            if (saveNotice == null) return;
            bool show = SaveManager.RecoveredFromBadFile && !noticeShown;
            saveNotice.gameObject.SetActive(show);
            if (show) noticeShown = true;
        }

        public void OpenSettings()
        {
            if (saveNotice != null) saveNotice.gameObject.SetActive(false);
            if (settings != null) settings.Open();
        }

        static void Select(Button button)
        {
            if (button != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        public void Play()
        {
            if (levelSelect != null) levelSelect.Open();
        }

        public void PlayEndless() => SceneLoader.Load(SceneNames.Endless);

        public void Quit()
        {
#if UNITY_EDITOR
            Debug.Log("Quit pressed. Application.Quit does nothing in the editor, so Play Mode is stopped instead.");
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
