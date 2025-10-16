using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseDistributionManager : MonoBehaviour
{
    public GameObject level1_distribution;
    public GameObject level2_distribution;
    private Settings.Difficulty highDifficulty = Settings.Difficulty.Alto;

    void Start()
    {
        if (Settings.currentDifficulty == highDifficulty){
            level2_distribution.SetActive(true);
            level1_distribution.SetActive(false);
        }else{
            level2_distribution.SetActive(false);
            level1_distribution.SetActive(true);
        }
    }
}
