using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalLimitController : MonoBehaviour
{

    private bool isInside;

    void Start()
    {
        isInside = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(!isInside){
                isInside = true;
            }
            else if ((isInside) && (Timer.IsTimeOver() != true)){
                isInside = false;
            }
        }
    }
}
