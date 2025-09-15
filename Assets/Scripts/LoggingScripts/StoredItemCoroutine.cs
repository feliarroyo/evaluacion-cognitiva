using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine;
using Object = System.Object;
using System.IO;
using System.Linq;
using TMPro;
using System.Collections;
using System.Runtime.CompilerServices;

public class StoredItemCoroutine : MonoBehaviour
{
    public string itemName;
    public static StoredItemCoroutine instance;

    public StoredItemCoroutine(string itemName)
    {
        this.itemName = itemName;
    }

    void Awake()
    {
        instance = this;
    }

    public void CheckNoItem(string grabbedItem)
    {
        StartCoroutine(SetNoItem(grabbedItem));
    }

    /// <summary>
    /// After a period of time upon choosing an item, clean the heldItem slot if nothing else is grabbed.
    /// </summary>
    /// <param name="grabbedItem">Name of the item to check.</param>
    private IEnumerator SetNoItem(string grabbedItem)
    {
        yield return new WaitForSeconds(0.5f);
        if (Logging.currentLog.heldItem.Equals(grabbedItem))
        {
            Logging.Log(Logging.EventType.NoItem, null);
        }
    }
}