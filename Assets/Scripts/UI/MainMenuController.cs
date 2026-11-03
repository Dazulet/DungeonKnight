using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonKnight
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] Button playButton;
        [SerializeField] Button quitButton;

        void Awake()
        {
            Time.timeScale = 1f;
            if (RefCheck.Valid(this, playButton, nameof(playButton)))
                playButton.onClick.AddListener(Play);
            if (RefCheck.Valid(this, quitButton, nameof(quitButton)))
                quitButton.onClick.AddListener(Quit);
        }

        void Start()
        {
            if (playButton != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        void Update()
        {
            if (KeyboardInput.PausePressed) Quit();
        }

        public void Play() => SceneLoader.Load(SceneNames.Dungeon);

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
