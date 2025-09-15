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
    public static List<Interactable> interactablesInScene = new();
    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<HeldItem>() == null)
        {
            Debug.Log("NUEVO FURNITURE EN SCENE: " + name);
            interactablesInScene.Add(this);
        }
        GetComponent<Outline>().enabled = false;
        behaviour = GetComponent<IElementBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        interactablesInScene.Remove(this);
    }

    /// <summary>
    /// Toggles whether an interactable can be interacted with.
    /// </summary>
    public void EnableInteraction(bool enable)
    {
        if (!stoppedInteraction)
        {
            SetOutline(enable);
            isInteractable = enable;
        }
    }

    public bool CheckVisibility()
    {
        return IsActuallyVisible(GetComponentInChildren<Renderer>(), Camera.main);
    }

    bool IsActuallyVisible(Renderer rend, Camera cam)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(rend.bounds.center);

        if (viewportPos.z < 0 || viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            Debug.Log(name + " IS ACTUALLY VISIBLE: (VIEWPOINT) " + false);
            return false;
        }
        Vector3 dir = (rend.bounds.center - cam.transform.position).normalized;

        if (Physics.Raycast(cam.transform.position, dir, out RaycastHit hit))
        {
            bool visible = hit.collider == GetComponent<Collider>();
            Debug.Log(name + " IS ACTUALLY VISIBLE (RAYCAST): " + visible);
            return visible;
        }
        Debug.Log(name + " IS ACTUALLY VISIBLE: " + false);
        return false;
    }

    /// <summary>
    /// This function defines whether the interactable element should be surrounded by an outline or not.
    /// </summary>
    /// 
    public virtual void SetOutline(bool showBorder)
    {
        GetComponent<Outline>().enabled = showBorder;
    }

    protected bool OverVirtualJoystick()
    {
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
            //Debug.Log("Clickeé en " + hitGameObject.name);
            return hitGameObject.name.Equals("RaycastPreventionCircle");
        }
        return false;
    }
    public void OnMouseDown()
    {
        if (OverVirtualJoystick())
        {
            return;
        }

        if (isInteractable && allowAllInteractions)
            behaviour.ClickBehaviour(gameObject);
    }
    public string GetID()
    {
        if (GetComponent<OpenDoor>() != null)
            return GetComponent<OpenDoor>().id;
        if (GetComponent<OpenDrawer>() != null)
            return GetComponent<OpenDrawer>().id;
        return name;
    }
}