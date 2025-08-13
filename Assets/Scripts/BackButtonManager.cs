using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButtonManager : MonoBehaviour
{
    public void ExitWithoutSaving(){
        GameStatus.ExitWithoutSaving();
    }
    void Start()
    {
        gameObject.SetActive(Settings.currentDifficulty == Settings.Difficulty.Preevaluación || GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Start);
    }
}
