using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultUIManager : MonoBehaviour
{
    public Transform resultKeyContent;
    public Transform resultFoundContent;
    public GameObject resultPrefab;
    public TextMeshProUGUI memorizeTimeText, searchTimeText;
    public List<HeldItem> itemsList;
    private readonly Color neutralColor = new(0.847f, 0.831f, 0.654f);
    private readonly Color correctColor = new(0.6f, 0.8f, 0.4f);
    private readonly Color wrongColor = new(1f, 0.7f, 0.7f);

    public void ShowResult(ResultData result)
    {

        foreach (Transform child in resultKeyContent) Destroy(child.gameObject);
        foreach (Transform child in resultFoundContent) Destroy(child.gameObject);

        memorizeTimeText.text = "Tiempo en el hall: " + result.memorizeTime;
        searchTimeText.text = "Tiempo en el living: " + result.searchTime;

        foreach (var keyItem in result.keyImageName)
        {
            GameObject newResult = Instantiate(resultPrefab, resultKeyContent);
            Image itemImage = newResult.transform.Find("BGItem/Item").GetComponent<Image>();
            Image itemBG = newResult.transform.Find("BGItem").GetComponent<Image>();
            itemBG.color = neutralColor;
            //  Sprite loadedSprite = Resources.Load<Sprite>(keyItem);
            //  itemImage.sprite = loadedSprite;
            HeldItem heldItem = GetHeldItemByName(keyItem);
            if (heldItem != null)
            {
                itemImage.sprite = heldItem.uiIconNoBG;
            }
        }

        foreach (var foundItem in result.foundImageName)
        {
            GameObject newResult = Instantiate(resultPrefab, resultFoundContent);
            Image itemImage = newResult.transform.Find("BGItem/Item").GetComponent<Image>();
            Image itemBG = newResult.transform.Find("BGItem").GetComponent<Image>();
            if (result.keyImageName.Contains(foundItem))
                itemBG.color = correctColor;
            else
                itemBG.color = wrongColor;
            // Sprite loadedSprite = Resources.Load<Sprite>(foundItem);
            // itemImage.sprite = loadedSprite;
            HeldItem heldItem = GetHeldItemByName(foundItem);
            if (heldItem != null)
            {
                itemImage.sprite = heldItem.uiIconNoBG;
            }
        }
    }

    
    public void ShowResult(List<HeldItem> keyItems, List<HeldItem> foundItems)
    {
        // Clean positions
        foreach (Transform child in resultKeyContent) Destroy(child.gameObject);
        foreach (Transform child in resultFoundContent) Destroy(child.gameObject);


        foreach (HeldItem keyItem in keyItems)
        {
            InstantiateItemIcon(keyItem, neutralColor, resultKeyContent);
        }

        foreach (HeldItem foundItem in foundItems)
        {
            InstantiateItemIcon(foundItem, foundItem.isKeyItem ? correctColor : wrongColor, resultFoundContent);
        }
    }

    private void InstantiateItemIcon(HeldItem hi, Color bgColor, Transform target)
    {
        GameObject newResult = Instantiate(resultPrefab, target);
        Image itemImage = newResult.transform.Find("BGItem/Item").GetComponent<Image>();
        Image itemBG = newResult.transform.Find("BGItem").GetComponent<Image>();
        itemBG.color = bgColor;
        itemImage.sprite = hi.uiIconNoBG;
    }

    private HeldItem GetHeldItemByName(string itemName)
    {
        return itemsList.Find(item => item.itemName == itemName);
    }
}
