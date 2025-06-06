using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This script disables interaction for a short period after loading a scene. It's used to avoid accidental inputs during scene transitions.
/// </summary>
public class DisableUIForTime : MonoBehaviour
{
    public float ignoreTime = 0.5f; // Time to ignore input
    private EventSystem eventSystem;

    void Start()
    {
        eventSystem = EventSystem.current;
        StartCoroutine(EnableUIAfterDelay());
    }

    IEnumerator EnableUIAfterDelay()
    {
        if (eventSystem != null)
            eventSystem.enabled = false; // Disable UI input

        yield return new WaitForSeconds(ignoreTime);

        if (eventSystem != null)
            eventSystem.enabled = true; // Re-enable UI input
    }
}
