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
        [SerializeField, Min(1f)] float popScale = 1.25f;
        [SerializeField, Min(0.01f)] float popTime = 0.18f;

        float scorePop;
        float coinPop;
        int shownHearts = -1;

        void Awake()
        {
            RefCheck.Valid(this, player, nameof(player));
            RefCheck.Valid(this, score, nameof(score));
            RefCheck.Valid(this, scoreText, nameof(scoreText));
            RefCheck.Valid(this, coinText, nameof(coinText));
        }

        void OnEnable()
        {
            if (player != null) player.HeartsChanged += OnHeartsChanged;
            if (score != null)
            {
                score.ScoreChanged += OnScoreChanged;
                score.CoinsChanged += OnCoinsChanged;
            }
        }

        void OnDisable()
        {
            if (player != null) player.HeartsChanged -= OnHeartsChanged;
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

        void Update()
        {
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
