using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // If using TextMeshPro


public class PopUpManager : MonoBehaviour
{
    public static PopUpManager instance;
    public GameObject popupPrefab; // Assign the prefab in the Inspector
    private GameObject currentPopup;
    private bool closeMessage;

    void Awake(){
        if (instance == null){
            instance = this;
        }
    }
    
    /// <summary>
    /// Creates a new window if none are active, with the message passed as parameter.
    /// </summary>
    /// <param name="message"></param>
    private void CreateWindow(string message){
        if (currentPopup == null) { // if no pop-up is active, create a pop-up
            currentPopup = Instantiate(popupPrefab, transform);
        }

        // Find the message text and update it
        TextMeshProUGUI messageText = currentPopup.GetComponent<PopUpPanel>().message;
        messageText.text = message;
        messageText.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageText.rectTransform);
    }
    
    public IEnumerator ShowPopups(string[] messages){
        foreach (string message in messages){
            yield return ShowPopup(message);
        }
        CloseWindow();
    }

    public IEnumerator ShowPopups(string message){
        yield return ShowPopup(message);
        CloseWindow();
    }
    
    /// <summary>
    /// Instantiate a Pop-Up if there isn't any present, and shows the message passed as parameter.
    /// </summary>
    /// <param name="message">Text message to show in the pop-up window.</param>
    public IEnumerator ShowPopup(string message)
    {
        if (popupPrefab != null)
        {
            CreateWindow(message);
            yield return new WaitUntil(() => closeMessage); // Wait until window is closed;
            closeMessage = false;
        }
        else
        {
            Debug.LogError("Popup Prefab not assigned!");
        }
    }

    public void ContinueButton(){
        closeMessage = true;
    }

    /// <summary>
    /// Destroys the current pop-up.
    /// </summary>
    private void CloseWindow()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup);
            currentPopup = null;
        }
    }
}
