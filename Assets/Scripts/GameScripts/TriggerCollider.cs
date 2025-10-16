using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCollider : MonoBehaviour
{
    public GameStatus.GamePhase triggerPhase;
    
    /// <summary>
    /// Start next phase when the player's capsule collider enters the trigger.
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other){
        if ((GameStatus.currentPhase == triggerPhase) && (other.GetType() == typeof(CapsuleCollider))){
            GameStatus.SetNextPhase();
        }
    }
}
