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

    public void ShowResult(ResultData result)
    {

        foreach (Transform child in resultKeyContent) Destroy(child.gameObject);
        foreach (Transform child in resultFoundContent) Destroy(child.gameObject);

        memorizeTimeText.text = "Tiempo en el hall: " + result.memorizeTime;
        searchTimeText.text = "Tiempo en el living: " + result.searchTime;

        foreach (var keyItem in result.keyImageName)
        {
            GameObject newResult = Instantiate(resultPrefab,resultKeyContent);
            UnityEngine.UI.Image itemImage = newResult.transform.Find("BGItem/Item").GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image itemBG = newResult.transform.Find("BGItem").GetComponent<UnityEngine.UI.Image>();
            itemBG.color = new Color(0.847f, 0.831f, 0.654f);
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
            UnityEngine.UI.Image itemImage = newResult.transform.Find("BGItem/Item").GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image itemBG = newResult.transform.Find("BGItem").GetComponent<UnityEngine.UI.Image>();
            if (result.keyImageName.Contains(foundItem))
                itemBG.color = new Color(0.6f, 0.8f, 0.4f);
            else
                itemBG.color = new Color(1f, 0.7f, 0.7f);
            // Sprite loadedSprite = Resources.Load<Sprite>(foundItem);
            // itemImage.sprite = loadedSprite;
            HeldItem heldItem = GetHeldItemByName(foundItem);
            if (heldItem != null)
            {
                itemImage.sprite = heldItem.uiIconNoBG;
            }
        }
    }

    private HeldItem GetHeldItemByName(string itemName)
    {
        return itemsList.Find(item => item.itemName == itemName);
    }
}
