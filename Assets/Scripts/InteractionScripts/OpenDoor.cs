using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

[RequireComponent(typeof(Collider))]

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
    public bool isMoving = false;
    public static List<OpenDoor> allDoors = new();
    public char rotationAxis = 'y';
    public Quaternion testRotation;
    public string id;
    private Collider col;
    private Interactable interactable;

    void Start()
    {
        col = GetComponent<Collider>();
        interactable = GetComponent<Interactable>();
        currentRotation = Quaternion.Euler(gameObject.transform.localEulerAngles);
        openRotation = rotationAxis switch
        {
            'x' => Quaternion.Euler(currentRotation.x + openAngle, currentRotation.y, currentRotation.z),
            'y' => Quaternion.Euler(currentRotation.x, currentRotation.y + openAngle, currentRotation.z),
            'z' => Quaternion.Euler(currentRotation.x, currentRotation.y, currentRotation.z + openAngle),
            _ => Quaternion.Euler(currentRotation.x, currentRotation.y + openAngle, currentRotation.z),
        };
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
        openRotation = rotationAxis switch
        {
            'x' => Quaternion.Euler(currentRotation.x + openAngle, currentRotation.y, currentRotation.z),
            'y' => Quaternion.Euler(currentRotation.x, currentRotation.y + openAngle, currentRotation.z),
            'z' => Quaternion.Euler(currentRotation.x, currentRotation.y, currentRotation.z + openAngle),
            _ => Quaternion.Euler(currentRotation.x, currentRotation.y + openAngle, currentRotation.z),
        };
        
        Quaternion targetRotation = isOpen ? openRotation : currentRotation;
        float currentAngle = Quaternion.Angle(transform.rotation, targetRotation);
        if (currentAngle > threshold)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * speed);
            isMoving = currentAngle > threshold + 10;
        }
        else
        {
            transform.localRotation = targetRotation; // Ensure it snaps to the exact target
            isMoving = false;
            if (!isOpen){
                col.enabled = true;
            }
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
            if (GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Search)
                col.enabled = false;
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
            interactable.EnableInteraction(false);
            interactable.stoppedInteraction = true;
        }
    }
}
