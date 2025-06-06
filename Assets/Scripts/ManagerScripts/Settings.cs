using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    [SerializeField]
    public enum Difficulty
    {
        Preevaluación,
        Bajo,
        Alto
    }

    public static GameStatus.GamePhase startingPhase = GameStatus.GamePhase.Waiting;
    public static Difficulty currentDifficulty = Difficulty.Bajo;
    // Start is called before the first frame update


}