using UnityEngine;

namespace DungeonKnight
{
    public static class TryCounter
    {
        static int level = -1;

        public static int Current { get; private set; }

        public static int BeginAttempt(int levelNumber)
        {
            if (levelNumber != level)
            {
                level = levelNumber;
                Current = 0;
            }
            return ++Current;
        }

        public static void Reset() => Current = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLaunch()
        {
            Current = 0;
            level = -1;
        }
    }
}
