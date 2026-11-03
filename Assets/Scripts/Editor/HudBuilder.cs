using UnityEngine;
using UnityEngine.UI;

namespace DungeonKnight.EditorTools
{
    public static class HudBuilder
    {
        public readonly struct Result
        {
            public readonly MobileJoystick Joystick;
            public readonly MobileShootButton ShootButton;

            public Result(MobileJoystick joystick, MobileShootButton shootButton)
            {
                Joystick = joystick;
                ShootButton = shootButton;
            }
        }

        static readonly Vector2 MenuButtonSize = new Vector2(560f, 140f);

        public static Result Build(GameManager game, ScoreManager score, PlayerHealth player)
        {
            Canvas canvas = UiFactory.Canvas("HUD Canvas");
            RectTransform safe = UiFactory.SafeArea(canvas.transform);

            Sprite heartFull = BuilderUtil.Sprite("UI/heart_full");
            Sprite heartEmpty = BuilderUtil.Sprite("UI/heart_empty");
            var hearts = new Image[3];
            for (int i = 0; i < hearts.Length; i++)
            {
                RectTransform heart = UiFactory.Rect(safe, $"Heart {i + 1}", UiFactory.TopLeft,
                    new Vector2(40f + i * 108f, -40f), new Vector2(96f, 96f));
                hearts[i] = UiFactory.Image(heart, heartFull);
            }

            RectTransform scorePanel = UiFactory.Rect(safe, "Score Panel", UiFactory.TopCenter, new Vector2(0f, -30f), new Vector2(480f, 120f));
            UiFactory.Image(scorePanel, BuilderUtil.Sprite("UI/ui_panel"));
            var left = new Vector2(0f, 0.5f);
            Text scoreText = UiFactory.Text(scorePanel, "Score", "000000", 60, left, new Vector2(28f, 0f),
                new Vector2(240f, 80f), TextAnchor.MiddleLeft);
            UiFactory.Image(UiFactory.Rect(scorePanel, "Coin Icon", left, new Vector2(280f, 0f), new Vector2(56f, 56f)),
                BuilderUtil.Sprite("UI/coin_icon"));
            Text coinText = UiFactory.Text(scorePanel, "Coins", "x0", 50, left, new Vector2(344f, 0f),
                new Vector2(110f, 80f), TextAnchor.MiddleLeft, UiFactory.Gold);

            Button pauseButton = UiFactory.IconButton(safe, "Pause Button", BuilderUtil.Sprite("UI/pause_icon"),
                UiFactory.TopRight, new Vector2(-40f, -40f), new Vector2(140f, 140f), new Vector2(64f, 64f));

            RectTransform joystickZone = UiFactory.Rect(safe, "Joystick", UiFactory.BottomLeft, Vector2.zero, new Vector2(520f, 520f));
            UiFactory.TouchZone(joystickZone);
            RectTransform stickBase = UiFactory.Rect(joystickZone, "Base", UiFactory.BottomLeft, new Vector2(250f, 250f),
                new Vector2(360f, 360f), UiFactory.Center);
            Image baseImage = UiFactory.Image(stickBase, BuilderUtil.Sprite("UI/joystick_base"));
            RectTransform knob = UiFactory.Rect(stickBase, "Knob", UiFactory.Center, Vector2.zero, new Vector2(150f, 150f));
            UiFactory.Image(knob, BuilderUtil.Sprite("UI/joystick_knob"));
            var joystick = joystickZone.gameObject.AddComponent<MobileJoystick>();
            BuilderUtil.Set(joystick, "stickBase", stickBase);
            BuilderUtil.Set(joystick, "knob", knob);
            BuilderUtil.Set(joystick, "baseGraphic", baseImage);

            RectTransform shootZone = UiFactory.Rect(safe, "Shoot Button", UiFactory.BottomRight, new Vector2(-50f, 50f), new Vector2(300f, 300f));
            UiFactory.TouchZone(shootZone);
            RectTransform shootVisual = UiFactory.Rect(shootZone, "Visual", UiFactory.Center, Vector2.zero, new Vector2(260f, 260f));
            UiFactory.Image(shootVisual, BuilderUtil.Sprite("UI/shoot_base"));
            UiFactory.Image(UiFactory.Rect(shootVisual, "Icon", UiFactory.Center, Vector2.zero, new Vector2(128f, 128f)),
                BuilderUtil.Sprite("UI/shoot_icon"));
            var shoot = shootZone.gameObject.AddComponent<MobileShootButton>();
            BuilderUtil.Set(shoot, "visual", shootVisual);

            GameObject pauseOverlay = BuildPauseOverlay(canvas.transform, out Button resume, out Button restart, out Button pauseMenu);
            GameObject gameOverOverlay = BuildGameOverOverlay(canvas.transform, out Text result, out Button retry, out Button gameOverMenu);

            var hud = canvas.gameObject.AddComponent<HudController>();
            BuilderUtil.Set(hud, "player", player);
            BuilderUtil.Set(hud, "score", score);
            BuilderUtil.Set(hud, "hearts", hearts);
            BuilderUtil.Set(hud, "heartFull", heartFull);
            BuilderUtil.Set(hud, "heartEmpty", heartEmpty);
            BuilderUtil.Set(hud, "scoreText", scoreText);
            BuilderUtil.Set(hud, "coinText", coinText);

            var pause = canvas.gameObject.AddComponent<PauseManager>();
            BuilderUtil.Set(pause, "game", game);
            BuilderUtil.Set(pause, "pausePanel", pauseOverlay);
            BuilderUtil.Set(pause, "pauseButton", pauseButton);
            BuilderUtil.Set(pause, "resumeButton", resume);
            BuilderUtil.Set(pause, "restartButton", restart);
            BuilderUtil.Set(pause, "menuButton", pauseMenu);
            BuilderUtil.Set(pause, "gameplayControls", new[] { joystickZone.gameObject, shootZone.gameObject, pauseButton.gameObject });

            var gameOver = canvas.gameObject.AddComponent<GameOverPanel>();
            BuilderUtil.Set(gameOver, "game", game);
            BuilderUtil.Set(gameOver, "score", score);
            BuilderUtil.Set(gameOver, "panel", gameOverOverlay);
            BuilderUtil.Set(gameOver, "resultText", result);
            BuilderUtil.Set(gameOver, "retryButton", retry);
            BuilderUtil.Set(gameOver, "menuButton", gameOverMenu);

            return new Result(joystick, shoot);
        }

        static RectTransform Overlay(Transform canvas, string name, Color dim, string title, Color titleColor)
        {
            RectTransform overlay = UiFactory.Stretch(canvas, name);
            UiFactory.Image(overlay, null, dim, raycast: true); 
            RectTransform panel = UiFactory.Rect(overlay, "Panel", UiFactory.Center, Vector2.zero, new Vector2(800f, 760f));
            UiFactory.Image(panel, BuilderUtil.Sprite("UI/ui_panel"));
            UiFactory.Text(panel, "Title", title, 90, UiFactory.Center, new Vector2(0f, 270f), new Vector2(760f, 120f),
                color: titleColor, outline: 6f);
            return panel;
        }

        static GameObject BuildPauseOverlay(Transform canvas, out Button resume, out Button restart, out Button menu)
        {
            RectTransform panel = Overlay(canvas, "Pause Overlay", new Color(0f, 0f, 0f, 0.7f), "PAUSED", UiFactory.Gold);
            resume = UiFactory.Button(panel, "Resume Button", "RESUME", UiFactory.Center, new Vector2(0f, 90f), MenuButtonSize);
            restart = UiFactory.Button(panel, "Restart Button", "RESTART", UiFactory.Center, new Vector2(0f, -80f), MenuButtonSize);
            menu = UiFactory.Button(panel, "Main Menu Button", "MAIN MENU", UiFactory.Center, new Vector2(0f, -250f), MenuButtonSize);
            GameObject overlay = panel.parent.gameObject;
            overlay.SetActive(false);
            return overlay;
        }

        static GameObject BuildGameOverOverlay(Transform canvas, out Text result, out Button retry, out Button menu)
        {
            RectTransform panel = Overlay(canvas, "Game Over Overlay", new Color(0.16f, 0.02f, 0.05f, 0.8f), "GAME OVER",
                new Color(1f, 0.42f, 0.42f));
            result = UiFactory.Text(panel, "Result", "SCORE 0\nCOINS 0", 60, UiFactory.Center, new Vector2(0f, 110f),
                new Vector2(700f, 180f));
            result.lineSpacing = 1.3f;
            retry = UiFactory.Button(panel, "Retry Button", "TRY AGAIN", UiFactory.Center, new Vector2(0f, -90f), MenuButtonSize);
            menu = UiFactory.Button(panel, "Main Menu Button", "MAIN MENU", UiFactory.Center, new Vector2(0f, -260f), MenuButtonSize);
            GameObject overlay = panel.parent.gameObject;
            overlay.SetActive(false);
            return overlay;
        }
    }
}
