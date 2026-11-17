using UnityEngine;
using UnityEngine.UI;

namespace DungeonKnight
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Text title;
        [SerializeField] Text detail;
        [SerializeField] Color lockedTextColor = new Color(0.55f, 0.5f, 0.55f);

        Color unlockedTextColor;

        public Button Button => button;

        void Awake()
        {
            RefCheck.Valid(this, button, nameof(button));
            RefCheck.Valid(this, title, nameof(title));
            RefCheck.Valid(this, detail, nameof(detail));
            if (title != null) unlockedTextColor = title.color;
        }

        public void Show(LevelData level, bool unlocked, LevelRecord record)
        {
            if (button != null) button.interactable = unlocked;
            if (title != null)
            {
                title.text = level.displayName;
                title.color = unlocked ? unlockedTextColor : lockedTextColor;
            }
            if (detail == null) return;
            detail.color = unlocked ? unlockedTextColor : lockedTextColor;
            if (!unlocked)
            {
                detail.text = "LOCKED";
                return;
            }
            int limit = Mathf.RoundToInt(level.timeLimit);
            string best = record != null ? $"BEST {record.bestScore}" : "NOT CLEARED";
            detail.text = $"{level.EnemyCount} ENEMIES\nTIME {limit / 60}:{limit % 60:00}\n{best}";
        }
    }
}
