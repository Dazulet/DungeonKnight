using UnityEngine;

namespace DungeonKnight
{
    public static class PhysicsCompat
    {
        public static void SetVelocity(this Rigidbody2D body, Vector2 velocity)
        {
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = velocity;
#else
            body.velocity = velocity;
#endif
        }
    }
}
