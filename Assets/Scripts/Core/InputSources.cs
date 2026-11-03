using UnityEngine;

namespace DungeonKnight
{
    public interface IMoveSource
    {
        Vector2 Move { get; }
    }

    public interface IShootSource
    {
        bool IsShooting { get; }
    }
}
