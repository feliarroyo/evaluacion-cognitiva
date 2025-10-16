using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuUIController : MonoBehaviour
{
    public enum SESSION_STATUS
    {
        FirstTime,
        TutorialOver,
        PracticeOver,
        Level1Over,
        Level2Over
    }
    public static SESSION_STATUS currentStatus;
    public SESSION_STATUS unlockStatus = SESSION_STATUS.FirstTime;
    private static List<MenuUIController> instances = new();

    // Start is called before the first frame update
    void Start()
    {
        instances.Add(this);
    }

    void OnDestroy()
    {
        instances.Remove(this);
    }

    void Awake()
    {
        currentStatus = (SESSION_STATUS)PlayerPrefs.GetInt("sessionStatus", (int)SESSION_STATUS.FirstTime);
        SetInteractivity();
    }

    void OnEnable()
    {
        SetInteractivity();
    }

    public void SetInteractivity()
    {
        GetComponent<Button>().interactable = unlockStatus <= currentStatus;
    }

    public void UpdateCurrentStatus()
    {
        if (currentStatus == unlockStatus)
        {
            currentStatus++;
            PlayerPrefs.SetInt("sessionStatus", (int)currentStatus);
        }
    }

    public static void ResetCurrentStatus()
    {
        PlayerPrefs.SetInt("sessionStatus", 0);
        currentStatus = SESSION_STATUS.FirstTime;
        UpdateInteractivity();
    }

    private static void UpdateInteractivity()
    {
        foreach (MenuUIController lv in instances)
        {
            lv.SetInteractivity();
        }
    }
}
