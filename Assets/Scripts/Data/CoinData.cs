using UnityEngine;

namespace DungeonKnight
{
    [CreateAssetMenu(menuName = "Dungeon Knight/Coin Data", fileName = "CoinData")]
    public class CoinData : ScriptableObject
    {
        [Min(0)] public int points = 10;
        [Min(0f)] public float magnetRange = 2.2f;
        [Min(0f)] public float magnetSpeed = 9f;
        [Min(1f)] public float lifetime = 25f;
    }
}
