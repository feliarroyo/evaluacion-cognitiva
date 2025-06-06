using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupScene : MonoBehaviour
{
    public Settings.Difficulty difficulty;
    public GameStatus.GamePhase startingPhase;
    public void SetConfig()
    {
        Settings.currentDifficulty = difficulty;
        Settings.startingPhase = startingPhase;
        GameStatus.currentPhase = startingPhase; // sacar luego
    }
}
