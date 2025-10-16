using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script is used to instantiate item icons on the top-right of the screen.
/// </summary>
public class InventoryDisplay : MonoBehaviour
{
    private static GameObject instance;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null){
            instance = gameObject;
        }
    }

    public static void AddItemToInventory(Sprite img){
        GameObject newGO = new GameObject();
        Image newIMG = newGO.AddComponent<Image>();
        newIMG.sprite = img;
        Instantiate(newGO, instance.transform);
    }

}
