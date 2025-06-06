using UnityEngine;

/// <summary>
/// This interface is used to implement clickable behaviour in interactable elements.
/// </summary>
[SerializeField]
public interface IElementBehaviour
{
    public void ClickBehaviour(GameObject go);
}