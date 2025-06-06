using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDetection : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SphereCollider>();
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
        if (other.gameObject.CompareTag("Interactable")) {
            other.gameObject.GetComponent<Interactable>().EnableInteraction(true);
            Debug.Log("TRIGGER ENTERING... " + gameObject);
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
        if (other.gameObject.CompareTag("Interactable")) {
            other.gameObject.GetComponent<Interactable>().EnableInteraction(false);
        }
    }
}
