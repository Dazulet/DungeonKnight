using System.Collections.Generic;
using UnityEngine;

namespace DungeonKnight
{
    public static class JuiceFx
    {
        static readonly List<GameObject> live = new List<GameObject>();

        public static bool Enabled => !SaveManager.Settings.effectsOff;

        public static void SetEnabled(bool enabled)
        {
            if (!enabled) ClearLive();
            SaveManager.SetEffects(enabled);
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position)
        {
            if (prefab == null || !Enabled) return null;
            live.RemoveAll(go => go == null);
            GameObject instance = Object.Instantiate(prefab, position, Quaternion.identity);
            live.Add(instance);
            return instance;
        }

        static void ClearLive()
        {
            foreach (GameObject go in live)
                if (go != null) Object.Destroy(go);
            live.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => live.Clear();
    }
}
