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
            Text tagline = UiFactory.Text(safe, "Tagline", "HOW LONG CAN YOU LAST?", 40, UiFactory.TopCenter, new Vector2(0f, -270f),
                new Vector2(1200f, 60f));
            tagline.rectTransform.pivot = UiFactory.Center;

            var buttonSize = new Vector2(560f, 140f);
            Button play = UiFactory.Button(safe, "Play Button", "PLAY", UiFactory.Center, new Vector2(0f, 40f), buttonSize, 70);
            Button quit = UiFactory.Button(safe, "Quit Button", "QUIT", UiFactory.Center, new Vector2(0f, -120f), buttonSize);

            UiFactory.Text(safe, "Controls Hint", "WASD MOVE - SPACE SHOOT - ESC PAUSE", 40, UiFactory.BottomCenter,
                new Vector2(0f, 90f), new Vector2(1600f, 50f), color: new Color(0.8f, 0.76f, 0.7f)).rectTransform.pivot = UiFactory.Center;
            UiFactory.Text(safe, "Footer", "NARXOZ UNIVERSITY - GAME DEVELOPMENT FUNDAMENTALS", 40, UiFactory.BottomCenter,
                new Vector2(0f, 36f), new Vector2(1800f, 50f), color: new Color(0.6f, 0.56f, 0.62f)).rectTransform.pivot = UiFactory.Center;

            var menu = canvas.gameObject.AddComponent<MainMenuController>();
            BuilderUtil.Set(menu, "playButton", play);
            BuilderUtil.Set(menu, "quitButton", quit);

            UiFactory.EventSystem();

            BuilderUtil.EnsureFolder(BuilderUtil.SceneRoot);
            EditorSceneManager.SaveScene(scene, $"{BuilderUtil.SceneRoot}/{SceneNames.MainMenu}.unity");
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
