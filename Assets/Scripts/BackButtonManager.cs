using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButtonManager : MonoBehaviour
{

    void Start()
    {
        gameObject.SetActive(Settings.currentDifficulty == Settings.Difficulty.Preevaluación || GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Start);
    }
}
