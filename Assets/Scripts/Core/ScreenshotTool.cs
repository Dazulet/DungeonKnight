using System;
using System.IO;
using UnityEngine;

namespace DungeonKnight
{
  
    public class ScreenshotTool : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void CreateInstance()
        {
            if (FindAnyObjectByType<ScreenshotTool>() != null) return;
            var go = new GameObject(nameof(ScreenshotTool));
            DontDestroyOnLoad(go);
            go.AddComponent<ScreenshotTool>();
        }

        void Update()
        {
            if (KeyboardInput.ScreenshotPressed) Capture();
        }

        public static string Capture()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Submission", "Screenshots", "_raw"));
            Directory.CreateDirectory(folder);
            string file = Path.Combine(folder, $"shot_{DateTime.Now:yyyyMMdd_HHmmss}_{Screen.width}x{Screen.height}.png");
            ScreenCapture.CaptureScreenshot(file);
            Debug.Log($"Screenshot saved to {file}");
            return file;
        }
#endif
    }
}
