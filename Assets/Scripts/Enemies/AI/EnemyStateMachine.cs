using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonKnight
{
    public class EnemyStateMachine<TState> where TState : struct, Enum
    {
        int lastChangeFrame = -1;

        public EnemyStateMachine(TState initial)
        {
            Current = initial;
            EnteredAt = Time.time;
        }

        public TState Current { get; private set; }
        public float EnteredAt { get; private set; }
        public float TimeInState => Time.time - EnteredAt;

        public void Restart() => EnteredAt = Time.time;

        public bool TryChange(TState next)
        {
            if (EqualityComparer<TState>.Default.Equals(next, Current)) return false;
            if (lastChangeFrame == Time.frameCount) return false;
            Current = next;
            EnteredAt = Time.time;
            lastChangeFrame = Time.frameCount;
            return true;
        }
    }
}
