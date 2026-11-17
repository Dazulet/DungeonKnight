using UnityEngine;
using UnityEngine.UI;

namespace DungeonKnight
{
    public class HudController : MonoBehaviour
    {
        [SerializeField] PlayerHealth player;
        [SerializeField] ScoreManager score;
        [SerializeField] Image[] hearts;
        [SerializeField] Sprite heartFull;
        [SerializeField] Sprite heartEmpty;
        [SerializeField] Text scoreText;
        [SerializeField] Text coinText;
        [SerializeField] LevelGoal goal;
        [SerializeField] Text goalText;
        [SerializeField] Text timerText;
        [SerializeField, Min(0)] int timerWarningSeconds = 15;
        [SerializeField] Color timerWarningColor = new Color(1f, 0.42f, 0.42f);
        [SerializeField, Min(1f)] float popScale = 1.25f;
        [SerializeField, Min(0.01f)] float popTime = 0.18f;

        float scorePop;
        float coinPop;
        int shownHearts = -1;
        int shownSeconds = -1;
        Color timerColor = Color.white;

        void Awake()
        {
            RefCheck.Valid(this, player, nameof(player));
            RefCheck.Valid(this, score, nameof(score));
            RefCheck.Valid(this, scoreText, nameof(scoreText));
            RefCheck.Valid(this, coinText, nameof(coinText));
            if (timerText != null) timerColor = timerText.color;
        }

        void OnEnable()
        {
            if (player != null) player.HeartsChanged += OnHeartsChanged;
            if (goal != null) goal.RemainingChanged += OnRemainingChanged;
            if (score != null)
            {
                score.ScoreChanged += OnScoreChanged;
                score.CoinsChanged += OnCoinsChanged;
            }
        }

        void OnDisable()
        {
            if (player != null) player.HeartsChanged -= OnHeartsChanged;
            if (goal != null) goal.RemainingChanged -= OnRemainingChanged;
            if (score != null)
            {
                score.ScoreChanged -= OnScoreChanged;
                score.CoinsChanged -= OnCoinsChanged;
            }
        }

        void Start()
        {
            if (player != null) ShowHearts(player.Hearts, player.MaxHearts);
            if (scoreText != null) scoreText.text = FormatScore(score != null ? score.Score : 0);
            if (coinText != null) coinText.text = FormatCoins(score != null ? score.Coins : 0);
            if (goal != null) OnRemainingChanged(goal.Remaining);
        }

        void OnRemainingChanged(int remaining)
        {
            if (goalText != null && goal != null) goalText.text = $"TRY {goal.Try}   ENEMIES {remaining}";
        }

        void OnHeartsChanged(int current, int max) => ShowHearts(current, max);

        void OnScoreChanged(int value)
        {
            if (scoreText != null) scoreText.text = FormatScore(value);
            scorePop = popTime;
        }

        void OnCoinsChanged(int value)
        {
            if (coinText != null) coinText.text = FormatCoins(value);
            coinPop = popTime;
        }

        static string FormatScore(int value) => value.ToString("D6");
        static string FormatCoins(int value) => "x" + value;

        void ShowHearts(int current, int max)
        {
            EnsureHeartSlots(max);
            if (hearts == null) return;
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] == null) continue;
                hearts[i].gameObject.SetActive(i < max);
                hearts[i].sprite = i < current ? heartFull : heartEmpty;
                if (shownHearts >= 0 && i == current && current < shownHearts)
                    hearts[i].rectTransform.localScale = Vector3.one * popScale;
            }
            shownHearts = current;
        }

        void EnsureHeartSlots(int count)
        {
            if (hearts == null || hearts.Length == 0 || hearts.Length >= count || hearts[0] == null) return;
            RectTransform first = hearts[0].rectTransform;
            Vector2 step = hearts.Length > 1 && hearts[1] != null
                ? hearts[1].rectTransform.anchoredPosition - first.anchoredPosition
                : new Vector2(first.sizeDelta.x, 0f);
            var slots = new Image[count];
            for (int i = 0; i < count; i++)
            {
                if (i < hearts.Length)
                {
                    slots[i] = hearts[i];
                    continue;
                }
                Image copy = Instantiate(hearts[0], first.parent);
                copy.name = $"Heart {i + 1}";
                copy.rectTransform.anchoredPosition = first.anchoredPosition + step * i;
                slots[i] = copy;
            }
            hearts = slots;
        }

        void Update()
        {
            UpdateTimer();
            float dt = Time.unscaledDeltaTime;
            scorePop = Animate(scoreText, scorePop, dt);
            coinPop = Animate(coinText, coinPop, dt);
            if (hearts == null) return;
            foreach (Image heart in hearts)
            {
                if (heart == null) continue;
                Transform t = heart.rectTransform;
                t.localScale = Vector3.MoveTowards(t.localScale, Vector3.one, dt * 2f);
            }
        }

        void UpdateTimer()
        {
            if (goal == null || timerText == null || goal.Level == null) return;
            int seconds = Mathf.CeilToInt(goal.TimeLeft);
            if (seconds == shownSeconds) return;
            shownSeconds = seconds;
            timerText.text = $"LEVEL {goal.Level.levelNumber}   TIME {seconds / 60}:{seconds % 60:00}";
            timerText.color = seconds <= timerWarningSeconds ? timerWarningColor : timerColor;
        }

        float Animate(Text text, float timeLeft, float dt)
        {
            if (text == null) return 0f;
            timeLeft = Mathf.Max(0f, timeLeft - dt);
            float s = Mathf.Lerp(1f, popScale, timeLeft / popTime);
            text.rectTransform.localScale = new Vector3(s, s, 1f);
            return timeLeft;
        }
    }
}
