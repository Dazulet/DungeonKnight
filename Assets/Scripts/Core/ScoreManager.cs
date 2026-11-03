using System;
using UnityEngine;

namespace DungeonKnight
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField, Min(0)] int pointsPerCoin = 10;

        public int Score { get; private set; }
        public int Coins { get; private set; }

        public event Action<int> ScoreChanged;
        public event Action<int> CoinsChanged;

        public void AddScore(int amount)
        {
            if (amount <= 0) return;
            Score += amount;
            ScoreChanged?.Invoke(Score);
        }

        public void AddCoin()
        {
            Coins++;
            CoinsChanged?.Invoke(Coins);
            AddScore(pointsPerCoin);
        }
    }
}
