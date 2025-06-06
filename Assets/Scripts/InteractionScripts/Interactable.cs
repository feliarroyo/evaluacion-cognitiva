using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

/// <summary>
/// This class represents the behavior of interactive elements within the environment, such as items or doors.
/// </summary>
public class Interactable : MonoBehaviour
{
    public IElementBehaviour behaviour;
    public static bool allowAllInteractions = true; // used to disable all interactions in certain cases.
    public bool isInteractable = false;
    public bool stoppedInteraction = false; // used to stop interaction completely
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Outline>().enabled = false;
        behaviour = GetComponent<IElementBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Toggles whether an interactable can be interacted with.
    /// </summary>
    public void EnableInteraction(bool enable){
        if (!stoppedInteraction){
            SetOutline(enable);
            isInteractable = enable;
        }
    }


    /// <summary>
    /// This function defines whether the interactable element should be surrounded by an outline or not.
    /// </summary>
    /// 
    public virtual void SetOutline(bool showBorder){
        GetComponent<Outline>().enabled = showBorder;
    }

    protected bool OverVirtualJoystick(){
        PointerEventData pointerEventData = new(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // Create a list to store the raycast results
        List<RaycastResult> raycastResults = new();

        // Perform the raycast to check for any UI elements under the pointer
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        // If the raycast hits any UI elements, the list will contain RaycastResults
        if (raycastResults.Count > 0)
        {
            // Get the first hit UI element
            GameObject hitGameObject = raycastResults[0].gameObject;
            Debug.Log("Clickeé en " + hitGameObject.name);
            return hitGameObject.name.Equals("RaycastPreventionCircle");
        }
        return false;
    }
    public void OnMouseDown(){
        if (OverVirtualJoystick()){
            return;
        }

        if (isInteractable && allowAllInteractions)
            behaviour.ClickBehaviour(gameObject);
    }
}