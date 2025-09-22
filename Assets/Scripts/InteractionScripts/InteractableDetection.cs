using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDetection : MonoBehaviour
{
    public static List<Interactable> enableWhenEntering;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SphereCollider>();
        enableWhenEntering = new();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// When entering the trigger, the element is surrounded by an outline and can be interacted with.
    /// </summary>
    public void OnTriggerEnter(Collider other){
        if (other is CapsuleCollider) // do not change triggers with CapsuleCollider
            return;
        if (other.gameObject.CompareTag("Tutorial")) {
            TutorialItem.playerIsClose = true;
            return;
        }
        if (other.gameObject.CompareTag("Interactable"))
        {
            // to avoid showing silhouettes before entering
            if (GameStatus.currentPhase != GameStatus.GamePhase.Tutorial_BeforeSearch && GameStatus.currentPhase != GameStatus.GamePhase.BeforeSearch)
            {
                other.gameObject.GetComponent<Interactable>().EnableInteraction(true);
                if (other.GetComponent<HeldItem>() != null)
                {
                    HeldItem hi = other.GetComponent<HeldItem>();
                    // if (Logging.itemsSeen.Contains(hi))
                    // {
                    //     if (Logging.currentLog.seenItems[hi.itemName].isInteractable == false)
                    //     {
                    //         Logging.Log(Logging.EventType.SeenObjectInteractivityChange, new[] {
                    //             hi.itemName
                    //         });
                    //     }
                    // }
                }
            }
            else enableWhenEntering.Add(other.gameObject.GetComponent<Interactable>());
            //Debug.Log("TRIGGER ENTERING... " + gameObject);
        }
    }

    /// <summary>
    /// When exiting the trigger, the element can no longer be interacted with.
    /// </summary>
    public void OnTriggerExit(Collider other){
        if (other is CapsuleCollider) // do not change triggers with CapsuleCollider
            return;
        if (other.gameObject.CompareTag("Tutorial")) {
            TutorialItem.playerIsClose = false;
            return;
        }
        if (other.gameObject.CompareTag("Interactable"))
        {
            if (GameStatus.currentPhase != GameStatus.GamePhase.Tutorial_BeforeSearch && GameStatus.currentPhase != GameStatus.GamePhase.BeforeSearch)
            {
                other.gameObject.GetComponent<Interactable>().EnableInteraction(false);
            }
            else
            {
                enableWhenEntering.Remove(other.gameObject.GetComponent<Interactable>());
            }
            
        }
    }
}
