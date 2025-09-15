using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

/// <summary>
/// This behaviour is used for the opening of doors. It rotates the interactable gameObject on click.
/// </summary>
public class TriggerDoor : OpenDoor, IElementBehaviour
{
    public GameStatus.GamePhase triggerPhase;
    public new void ClickBehaviour(GameObject go)
    {
        base.ClickBehaviour(go);
        if (GameStatus.currentPhase == triggerPhase)
        {
            GameStatus.SetNextPhase();
        }
    }
}
