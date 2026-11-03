using UnityEngine;

namespace DungeonKnight
{
   
    public static class GameLayers
    {
        public const int Player = 8;
        public const int Enemy = 9;
        public const int Projectile = 10;
        public const int Pickup = 11;
        public const int Obstacle = 12;

        static readonly int[] All = { Player, Enemy, Projectile, Pickup, Obstacle };

        public static int Mask(params int[] layers)
        {
            int mask = 0;
            foreach (int layer in layers) mask |= 1 << layer;
            return mask;
        }

        static bool Collides(int a, int b)
        {
            return Pair(a, b, Player, Obstacle)
                || Pair(a, b, Enemy, Obstacle)
                || Pair(a, b, Enemy, Enemy)
                || Pair(a, b, Projectile, Obstacle)
                || Pair(a, b, Projectile, Enemy)
                || Pair(a, b, Pickup, Player);
        }

        static bool Pair(int a, int b, int x, int y) => (a == x && b == y) || (a == y && b == x);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ApplyCollisionMatrix()
        {
            foreach (int a in All)
            foreach (int b in All)
                Physics2D.IgnoreLayerCollision(a, b, !Collides(a, b));
        }
    }
}
