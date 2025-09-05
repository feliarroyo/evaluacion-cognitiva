using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyActiveOnTutorial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GameStatus.currentPhase != GameStatus.GamePhase.Tutorial_Start)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
