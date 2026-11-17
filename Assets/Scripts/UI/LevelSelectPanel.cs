using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonKnight
{
    public class LevelSelectPanel : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] LevelButton buttonTemplate;
        [SerializeField] Button backButton;

        readonly List<LevelButton> buttons = new List<LevelButton>();
        readonly List<LevelData> levels = new List<LevelData>();

        public bool IsOpen => panel != null && panel.activeSelf;
        public event System.Action Closed;

        void Awake()
        {
            RefCheck.Valid(this, panel, nameof(panel));
            RefCheck.Valid(this, buttonTemplate, nameof(buttonTemplate));
            if (backButton != null) backButton.onClick.AddListener(Close);
            if (buttonTemplate != null) buttonTemplate.gameObject.SetActive(false);
            BuildButtons();
            if (panel != null) panel.SetActive(false);
        }

        void BuildButtons()
        {
            LevelCatalog catalog = LevelCatalog.Instance;
            if (catalog == null || catalog.levels == null || buttonTemplate == null)
            {
                Debug.LogError("Level select needs Resources/LevelCatalog and a button template.", this);
                return;
            }
            foreach (LevelData level in catalog.levels)
            {
                if (level == null) continue;
                LevelButton item = Instantiate(buttonTemplate, buttonTemplate.transform.parent);
                item.name = $"Level {level.levelNumber} Button";
                item.gameObject.SetActive(true);
                LevelData target = level;
                item.Button.onClick.AddListener(() => LevelSession.TryStart(target));
                buttons.Add(item);
                levels.Add(level);
            }
        }

        public void Open()
        {
            if (panel == null) return;
            Refresh();
            panel.SetActive(true);
            Button first = backButton;
            foreach (LevelButton item in buttons)
                if (item.Button.interactable) first = item.Button;
            if (first != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(first.gameObject);
        }

        public void Close()
        {
            if (panel == null || !panel.activeSelf) return;
            panel.SetActive(false);
            Closed?.Invoke();
        }

        public void Refresh()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                LevelData level = levels[i];
                buttons[i].Show(level, SaveManager.IsUnlocked(level.levelNumber), SaveManager.RecordFor(level.levelNumber));
            }
        }
    }
}
