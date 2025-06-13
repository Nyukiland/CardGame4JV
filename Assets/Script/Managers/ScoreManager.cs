using System;
using UnityEngine;

namespace CardGame.Managers
{
    public static class ScoreManager
    {
        public delegate void IntegerDelegate(int value);
        public static event IntegerDelegate ScoringEvent;
        
        public static int PlayerScore { get; private set; }

        public static void AddFinishedZone(int tokensAmount)
        {
            PlayerScore += tokensAmount;
            ScoringEvent?.Invoke(PlayerScore);
        }

        public static void AddWhiteFlag(int tilesAmount)
        {
            PlayerScore += tilesAmount * 2;
            ScoringEvent?.Invoke(PlayerScore);
        }
    }
}