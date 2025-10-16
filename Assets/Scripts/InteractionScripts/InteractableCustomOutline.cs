using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

/// <summary>
/// This class adds the option to define the outline component in the inspector.
/// </summary>
public class InteractableCustomOutline : Interactable
{
    public Outline outlineComponent;

    void Start()
    {
        CheckInteractableInScene();
        outlineComponent.enabled = false;
        behaviour = GetComponent<IElementBehaviour>();
    }

    /// <summary>
    /// This function defines whether the interactable element should be surrounded by an outline or not.
    /// </summary>
    /// 
    public override void SetOutline(bool showBorder){
        outlineComponent.enabled = showBorder;
    }
}