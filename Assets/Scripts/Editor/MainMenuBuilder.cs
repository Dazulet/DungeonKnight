using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DungeonKnight.EditorTools
{
    public static class MainMenuBuilder
    {
        const float BgWidth = 320f;
        const float BgHeight = 180f;

        static readonly Vector2 MenuButtonSize = new Vector2(560f, 120f);
        static readonly Vector2 PanelButtonSize = new Vector2(420f, 120f);
        static readonly Vector2 LevelButtonSize = new Vector2(340f, 420f);

        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            UiFactory.Camera(Color.black, 5f);

            Canvas canvas = UiFactory.Canvas("Menu Canvas");
            BuildBackground(canvas.transform);

            RectTransform safe = UiFactory.SafeArea(canvas.transform);
            Text title = UiFactory.Text(safe, "Title", "DUNGEON KNIGHT", 150, UiFactory.TopCenter, new Vector2(0f, -150f),
                new Vector2(1600f, 160f), color: UiFactory.Gold, outline: 8f);
            title.rectTransform.pivot = UiFactory.Center;
            title.gameObject.AddComponent<UiFloat>();
            Text tagline = UiFactory.Text(safe, "Tagline", "CLEAR THE DUNGEON", 40, UiFactory.TopCenter, new Vector2(0f, -270f),
                new Vector2(1200f, 60f));
            tagline.rectTransform.pivot = UiFactory.Center;

            Button play = UiFactory.Button(safe, "Play Button", "PLAY", UiFactory.Center, new Vector2(0f, 150f), MenuButtonSize, 70);
            Button endless = UiFactory.Button(safe, "Endless Button", "ENDLESS", UiFactory.Center, new Vector2(0f, 10f), MenuButtonSize);
            Button settingsButton = UiFactory.Button(safe, "Settings Button", "SETTINGS", UiFactory.Center, new Vector2(0f, -130f), MenuButtonSize);
            Button quit = UiFactory.Button(safe, "Quit Button", "QUIT", UiFactory.Center, new Vector2(0f, -270f), MenuButtonSize);

            Text notice = UiFactory.Text(safe, "Save Notice", "SAVE FILE COULD NOT BE READ - STARTED WITH DEFAULTS", 36,
                UiFactory.BottomCenter, new Vector2(0f, 150f), new Vector2(1600f, 50f), color: new Color(1f, 0.55f, 0.45f));
            notice.rectTransform.pivot = UiFactory.Center;
            notice.gameObject.SetActive(false);

            UiFactory.Text(safe, "Controls Hint", "WASD MOVE - SPACE SHOOT - ESC PAUSE", 40, UiFactory.BottomCenter,
                new Vector2(0f, 90f), new Vector2(1600f, 50f), color: new Color(0.8f, 0.76f, 0.7f)).rectTransform.pivot = UiFactory.Center;
            UiFactory.Text(safe, "Footer", "NARXOZ UNIVERSITY - GAME DEVELOPMENT FUNDAMENTALS", 40, UiFactory.BottomCenter,
                new Vector2(0f, 36f), new Vector2(1800f, 50f), color: new Color(0.6f, 0.56f, 0.62f)).rectTransform.pivot = UiFactory.Center;

            LevelSelectPanel levelSelect = BuildLevelSelect(canvas.transform);
            SettingsPanel settings = BuildSettings(canvas.transform);

            var menu = canvas.gameObject.AddComponent<MainMenuController>();
            BuilderUtil.Set(menu, "playButton", play);
            BuilderUtil.Set(menu, "endlessButton", endless);
            BuilderUtil.Set(menu, "settingsButton", settingsButton);
            BuilderUtil.Set(menu, "quitButton", quit);
            BuilderUtil.Set(menu, "levelSelect", levelSelect);
            BuilderUtil.Set(menu, "settings", settings);
            BuilderUtil.Set(menu, "saveNotice", notice);

            UiFactory.EventSystem();

            BuilderUtil.EnsureFolder(BuilderUtil.SceneRoot);
            EditorSceneManager.SaveScene(scene, $"{BuilderUtil.SceneRoot}/{SceneNames.MainMenu}.unity");
        }

        static RectTransform Overlay(Transform parent, string name, Vector2 panelSize, string title, out GameObject overlay)
        {
            RectTransform root = UiFactory.Stretch(parent, name);
            UiFactory.Image(root, null, new Color(0f, 0f, 0f, 0.75f), raycast: true);
            RectTransform panel = UiFactory.Rect(root, "Panel", UiFactory.Center, Vector2.zero, panelSize);
            UiFactory.Image(panel, BuilderUtil.Sprite("UI/ui_panel"));
            UiFactory.Text(panel, "Title", title, 80, UiFactory.Center, new Vector2(0f, panelSize.y * 0.5f - 90f),
                new Vector2(panelSize.x - 60f, 110f), color: UiFactory.Gold, outline: 6f);
            overlay = root.gameObject;
            return panel;
        }

        static LevelSelectPanel BuildLevelSelect(Transform canvas)
        {
            RectTransform panel = Overlay(canvas, "Level Select Overlay", new Vector2(1240f, 820f), "SELECT LEVEL", out GameObject overlay);

            RectTransform list = UiFactory.Rect(panel, "Level List", UiFactory.Center, new Vector2(0f, 10f), new Vector2(1160f, LevelButtonSize.y));
            var layout = list.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 40f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Button template = UiFactory.Button(list, "Level Button Template", "", UiFactory.Center, Vector2.zero, LevelButtonSize);
            Object.DestroyImmediate(template.transform.Find("Label").gameObject);
            Text levelTitle = UiFactory.Text(template.transform, "Title", "LEVEL 1", 56, UiFactory.Center, new Vector2(0f, 140f),
                new Vector2(320f, 80f), color: UiFactory.Gold);
            Text levelDetail = UiFactory.Text(template.transform, "Detail", "LOCKED", 36, UiFactory.Center, new Vector2(0f, -40f),
                new Vector2(320f, 240f));
            levelDetail.lineSpacing = 1.25f;
            var item = template.gameObject.AddComponent<LevelButton>();
            BuilderUtil.Set(item, "button", template);
            BuilderUtil.Set(item, "title", levelTitle);
            BuilderUtil.Set(item, "detail", levelDetail);

            Button back = UiFactory.Button(panel, "Back Button", "BACK", UiFactory.Center, new Vector2(0f, -320f), PanelButtonSize);

            var levelSelect = overlay.AddComponent<LevelSelectPanel>();
            BuilderUtil.Set(levelSelect, "panel", overlay);
            BuilderUtil.Set(levelSelect, "buttonTemplate", item);
            BuilderUtil.Set(levelSelect, "backButton", back);
            return levelSelect;
        }

        static SettingsPanel BuildSettings(Transform canvas)
        {
            RectTransform panel = Overlay(canvas, "Settings Overlay", new Vector2(1000f, 1020f), "SETTINGS", out GameObject overlay);
            var wide = new Vector2(760f, 112f);
            Slider music = VolumeSlider(panel, "Music Volume", "MUSIC", 290f, out Text musicValue);
            Slider sfx = VolumeSlider(panel, "SFX Volume", "SFX", 180f, out Text sfxValue);
            Button shake = UiFactory.Button(panel, "Screen Shake Button", "SCREEN SHAKE: ON", UiFactory.Center, new Vector2(0f, 55f), wide, 52);
            Button aim = UiFactory.Button(panel, "Auto Aim Button", "AUTO AIM: ON", UiFactory.Center, new Vector2(0f, -70f), wide, 52);
            Button reset = UiFactory.Button(panel, "Reset Progress Button", "RESET PROGRESS", UiFactory.Center, new Vector2(0f, -195f), wide, 52);
            Text status = UiFactory.Text(panel, "Status", "", 40, UiFactory.Center, new Vector2(0f, -287f), new Vector2(900f, 50f),
                color: UiFactory.Gold);
            Button back = UiFactory.Button(panel, "Back Button", "BACK", UiFactory.Center, new Vector2(0f, -385f), PanelButtonSize);

            RectTransform confirm = Overlay(overlay.transform, "Reset Confirm", new Vector2(960f, 600f), "RESET PROGRESS?", out GameObject confirmOverlay);
            Text body = UiFactory.Text(confirm, "Message", "ALL LEVELS AND BEST SCORES\nWILL BE DELETED.\nSETTINGS ARE KEPT.", 40,
                UiFactory.Center, new Vector2(0f, 30f), new Vector2(900f, 200f));
            body.lineSpacing = 1.3f;
            Button yes = UiFactory.Button(confirm, "Confirm Reset Button", "RESET", UiFactory.Center, new Vector2(-210f, -190f),
                new Vector2(380f, 120f));
            Button no = UiFactory.Button(confirm, "Cancel Reset Button", "CANCEL", UiFactory.Center, new Vector2(210f, -190f),
                new Vector2(380f, 120f));

            var settings = overlay.AddComponent<SettingsPanel>();
            BuilderUtil.Set(settings, "panel", overlay);
            BuilderUtil.Set(settings, "musicSlider", music);
            BuilderUtil.Set(settings, "musicLabel", musicValue);
            BuilderUtil.Set(settings, "sfxSlider", sfx);
            BuilderUtil.Set(settings, "sfxLabel", sfxValue);
            BuilderUtil.Set(settings, "screenShakeButton", shake);
            BuilderUtil.Set(settings, "screenShakeLabel", shake.transform.Find("Label").GetComponent<Text>());
            BuilderUtil.Set(settings, "autoAimButton", aim);
            BuilderUtil.Set(settings, "autoAimLabel", aim.transform.Find("Label").GetComponent<Text>());
            BuilderUtil.Set(settings, "resetButton", reset);
            BuilderUtil.Set(settings, "backButton", back);
            BuilderUtil.Set(settings, "statusText", status);
            BuilderUtil.Set(settings, "confirmPanel", confirmOverlay);
            BuilderUtil.Set(settings, "confirmButton", yes);
            BuilderUtil.Set(settings, "cancelButton", no);
            return settings;
        }

        static Slider VolumeSlider(Transform parent, string name, string label, float y, out Text value)
        {
            RectTransform row = UiFactory.Rect(parent, name, UiFactory.Center, new Vector2(0f, y), new Vector2(760f, 96f));
            UiFactory.Text(row, "Label", label, 48, UiFactory.Center, new Vector2(-280f, 3f), new Vector2(200f, 80f), TextAnchor.MiddleLeft);
            value = UiFactory.Text(row, "Value", "80%", 46, UiFactory.Center, new Vector2(315f, 3f), new Vector2(130f, 80f),
                TextAnchor.MiddleRight, UiFactory.Gold);

            RectTransform root = UiFactory.Rect(row, "Slider", UiFactory.Center, new Vector2(40f, 0f), new Vector2(350f, 72f));
            RectTransform track = UiFactory.Area(root, "Background", new Vector2(0f, 0.32f), new Vector2(1f, 0.68f));
            Image trackImage = UiFactory.Image(track, null, new Color(0.13f, 0.09f, 0.13f, 1f), raycast: true);
            var trackEdge = track.gameObject.AddComponent<Outline>();
            trackEdge.effectColor = new Color(0.55f, 0.47f, 0.4f, 1f);
            trackEdge.effectDistance = new Vector2(3f, -3f);

            RectTransform fillArea = UiFactory.Area(root, "Fill Area", new Vector2(0f, 0.32f), new Vector2(1f, 0.68f));
            RectTransform fill = UiFactory.Stretch(fillArea, "Fill");
            UiFactory.Image(fill, null, UiFactory.Gold);

            RectTransform handleArea = UiFactory.Stretch(root, "Handle Slide Area");
            handleArea.offsetMin = new Vector2(32f, 0f);
            handleArea.offsetMax = new Vector2(-32f, 0f);
            RectTransform handle = UiFactory.Rect(handleArea, "Handle", UiFactory.Center, Vector2.zero, new Vector2(64f, 0f));
            handle.anchorMin = new Vector2(0f, 0f);
            handle.anchorMax = new Vector2(0f, 1f);
            handle.sizeDelta = new Vector2(64f, 0f);
            Image handleImage = UiFactory.Image(handle, BuilderUtil.Sprite("UI/ui_button"), raycast: true);

            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.value = GameSettings.DefaultMusicVolume;
            ColorBlock colors = slider.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.95f, 0.82f);
            colors.selectedColor = new Color(1f, 0.9f, 0.6f);
            colors.pressedColor = new Color(0.72f, 0.66f, 0.62f);
            colors.fadeDuration = 0.06f;
            slider.colors = colors;
            trackImage.raycastTarget = true;
            return slider;
        }

        static Vector2 BgPoint(float x, float yFromTop) => new Vector2(x / BgWidth, 1f - yFromTop / BgHeight);

        static Vector2 BgSize(float width, float height) => new Vector2(width / BgWidth, height / BgHeight);

        static void BuildBackground(Transform canvas)
        {
            RectTransform bg = UiFactory.Stretch(canvas, "Background");
            UiFactory.Image(bg, BuilderUtil.Sprite("UI/menu_bg"));
            var fitter = bg.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 16f / 9f;

            Sprite glow = BuilderUtil.Sprite("Environment/glow");
            Sprite[] torchFrames = BuilderUtil.Frames("Environment/torch", 2);
            foreach (float x in new[] { 86f, 234f })
            {
                RectTransform light = UiFactory.AreaAround(bg, "Torch Glow", BgPoint(x, 72f), BgSize(90f, 90f));
                UiFactory.Image(light, glow, new Color(1f, 0.6f, 0.25f, 0.45f));
                light.gameObject.AddComponent<TorchFlicker>();

                RectTransform torch = UiFactory.AreaAround(bg, "Torch", BgPoint(x, 70f), BgSize(16f, 16f));
                UiFactory.Image(torch, torchFrames[0]);
                BuilderUtil.AddFlipbook(torch.gameObject, torchFrames, 6f, randomizeStart: true, unscaledTime: true);
            }

            RectTransform knight = UiFactory.AreaAround(bg, "Knight", BgPoint(106f, 144f), BgSize(16f, 16f));
            UiFactory.Image(knight, BuilderUtil.Sprite("Characters/knight_idle_0"));
            BuilderUtil.AddFlipbook(knight.gameObject, BuilderUtil.Frames("Characters/knight_idle", 2), 3f, unscaledTime: true);

            RectTransform slime = UiFactory.AreaAround(bg, "Slime", BgPoint(214f, 146f), BgSize(16f, 16f));
            UiFactory.Image(slime, BuilderUtil.Sprite("Enemies/slime_0"));
            BuilderUtil.AddFlipbook(slime.gameObject, BuilderUtil.Frames("Enemies/slime", 2), 4f, unscaledTime: true);
        }
    }
}
