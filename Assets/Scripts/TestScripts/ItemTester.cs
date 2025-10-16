using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTester : MonoBehaviour
{
    public List<GameObject> allItemsList = new();
    public ItemSpawn spawn;
    public int currentIndex = 0;
    private GameObject currentItem;
    private int maxCount;
    // Start is called before the first frame update
    void Start()
    {
        currentItem = PlaceItemInSpawnpoint(allItemsList[currentIndex], spawn);
        maxCount = allItemsList.Count;
    }

    public void DisplayNextItem()
    {
        ChangeDisplayedItem(1);
    }

    public void DisplayPreviousItem()
    {
        ChangeDisplayedItem(-1);
    }

    private void ChangeDisplayedItem(int changeValue)
    {
        currentIndex += changeValue;

        if (currentIndex >= maxCount)
        {
            currentIndex -= maxCount;
        }
        if (currentIndex < 0)
        {
            currentIndex += maxCount;
        }
        if (HeldItem.currentlyHeldItem != null)
        {
            HeldItem.ReturnItem();
        }
        if (currentItem != null)
        {
            Destroy(currentItem);
        }
        currentItem = PlaceItemInSpawnpoint(allItemsList[currentIndex], spawn);
    }


    public GameObject PlaceItemInSpawnpoint(GameObject item, ItemSpawn spawnPoint)
    {
        HeldItem heldItem = item.GetComponent<HeldItem>();
        heldItem.isEnvironmentItem = false;
        GameObject go = Instantiate(item, spawn.transform);
        return go;
    }
    
}
