using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ItemInteraction : MonoBehaviour
{
    private static Button button;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
    }

    /// <summary>
    /// (De)Activates the item interaction canvas.
    /// </summary>
    /// <param name="setActive">True to show canvas, False to hide.</param>
    public static void EnableButton(bool setActive){
        // Only environment items should be able to be stored
        button.interactable = setActive;
    }

    public void ReturnItem(){
        if (GameStatus.currentPhase < GameStatus.GamePhase.Waiting){
            return;
        }
        PlayerMovement.AllowMovement(true);
        HeldItem.ReturnItem();
        button.interactable = false;
    }
}
