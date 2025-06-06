using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    /// <summary>
    /// Stops or resumes time according to the boolean passed as parameter.
    /// </summary>
    /// <param name="pause">Whether to pause or unpause the game.</param>
    public void PauseGame(bool pause){
        Time.timeScale = pause? 0 : 1;
    }
}
