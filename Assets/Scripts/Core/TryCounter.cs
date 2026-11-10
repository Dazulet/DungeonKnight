using UnityEngine;

namespace DungeonKnight
{
    public static class TryCounter
    {
        public static int Current { get; private set; }

        public static int BeginAttempt() => ++Current;

        public static void Reset() => Current = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLaunch() => Current = 0;
    }
}
