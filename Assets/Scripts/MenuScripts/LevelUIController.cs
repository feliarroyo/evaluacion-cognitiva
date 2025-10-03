using System.Collections.Generic;
using UnityEngine.UI;

[UnityEngine.RequireComponent(typeof(Button))]
public class LevelUIController : MenuUIController
{
    public SESSION_STATUS lockStatus;

    public new void SetInteractivity()
    {
        GetComponent<Button>().interactable = unlockStatus <= currentStatus && currentStatus != lockStatus;
    }

    void OnEnable()
    {
        SetInteractivity();
    }

}
