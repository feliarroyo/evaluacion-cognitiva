using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

/// <summary>
/// This behaviour is used for the opening of doors. It rotates the interactable gameObject on click.
/// </summary>
public class OpenDoor : MonoBehaviour, IElementBehaviour
{
    public float speed = 2;
    public float openAngle = 90.0f;
    public bool canBeClosed = true;
    public OcclusionPortal portal = null;
    public bool isOpen = false;
    private Quaternion currentRotation;
    private Quaternion openRotation;
    const float threshold = 0.01f; // Tolerance for stopping condition
    private bool isMoving = false;
    public static List<OpenDoor> allDoors = new List<OpenDoor>();
    public char rotationAxis = 'y';
    public Quaternion testRotation;
    public string id;

    void Start()
    {
        currentRotation = Quaternion.Euler(gameObject.transform.eulerAngles);
        switch (rotationAxis)
        {
            case 'x':
                openRotation = Quaternion.Euler(currentRotation.x + openAngle, currentRotation.y, currentRotation.z);
                break;
            case 'y':
                openRotation = Quaternion.Euler(currentRotation.x, currentRotation.y + openAngle, currentRotation.z);
                break;
            case 'z':
                openRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentRotation.z + openAngle);
                break;
            default:
                openRotation = Quaternion.Euler(currentRotation.x, currentRotation.y + openAngle, currentRotation.z);
                break;

        }

        allDoors.Add(this);
    }

    void OnDestroy()
    {
        allDoors.Remove(this);
    }

    public static void EnableInteractions(bool enable)
    {
        foreach (var door in allDoors)
        {
            door.GetComponent<Interactable>().EnableInteraction(enable);
        }
    }

    void Update()
    {
        switch (rotationAxis)
        {
            case 'x':
                openRotation = Quaternion.Euler(currentRotation.x + openAngle, currentRotation.y, currentRotation.z);
                break;
            case 'y':
                openRotation = Quaternion.Euler(currentRotation.x, currentRotation.y + openAngle, currentRotation.z);
                break;
            case 'z':
                openRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentRotation.z + openAngle);
                break;
            default:
                openRotation = Quaternion.Euler(currentRotation.x, currentRotation.y + openAngle, currentRotation.z);
                break;

        }

        Quaternion targetRotation = isOpen ? openRotation : currentRotation;
        float currentAngle = Quaternion.Angle(transform.rotation, targetRotation);
        if (currentAngle > threshold)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
            isMoving = currentAngle > threshold + 10;
        }
        else
        {
            transform.rotation = targetRotation; // Ensure it snaps to the exact target
            isMoving = false;
        }
    }

    public bool IsDoorMoving()
    {
        return isMoving;
    }

    public void ClickBehaviour(GameObject go)
    {
        if (!canBeClosed && isOpen)
        {
            return;
        }
        isOpen = !isOpen;
        if (isOpen){
            Logging.Log(Logging.EventType.ElementOpen, new[] {(Object) id});
        }
        else
            Logging.Log(Logging.EventType.ElementClose, new[] {(Object) id});
        if (portal != null)
        {
            portal.open = isOpen;
        }
        if (!canBeClosed && isOpen)
        {
            GetComponent<Interactable>().EnableInteraction(false);
            GetComponent<Interactable>().stoppedInteraction = true;
        }
    }
}
