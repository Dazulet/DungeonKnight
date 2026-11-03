using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonKnight
{

    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        public static void ReloadCurrent()
        {
            Load(SceneManager.GetActiveScene().name);
        }
    }
}
