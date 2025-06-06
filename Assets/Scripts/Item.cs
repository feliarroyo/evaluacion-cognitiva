using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Object = System.Object;

public class Item : MonoBehaviour
{
    public Vector3 originalPosition;
    public string itemName;
    public bool 
        isKeyItem, // key items are those that the player needs to find
        isEnvironmentItem, // environment items are those found during the activity
        isBeingMoved = false, // whether it is travelling to the player or not
        isBeingHeld = false,  // whether the player is currently holding them or not
        isBeingReturned = false // whether it is returning to its original position or not
    ;
    
    public static GameObject currentlyHeldItem = null;
    private bool rotate = false;
    private float rotX, rotY;
    
    // Update is called once per frame
    void Update()
    {
        if (isBeingMoved){
            if (Vector3.Distance(transform.position, Camera.main.transform.TransformPoint(Vector3.forward * 1)) < 0.001f){
                isBeingMoved = false;
                isBeingHeld = true;
            }
        }
        if (isBeingHeld){
            // Allow rotation controls
            if (Input.GetMouseButton(0))
            {
                rotate = true;
                Logging.DebugLog("Rotation active");
            }
            else 
            {
                Logging.DebugLog("Stop rotation");
                rotate = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (rotate){
            gameObject.GetComponent<Rigidbody>().AddTorque(rotY, -rotX, 0);
            }
    }

    void OnMouseDown(){
        currentlyHeldItem = gameObject;
        Logging.Log(Logging.EventType.ItemGrab, new [] {currentlyHeldItem.GetComponent<HeldItem>().itemName});
        ItemInteraction.EnableButton(true);
        Logging.DebugLog(gameObject.name + " clicked");
        originalPosition = transform.position;
        isBeingMoved = true;
    }

}
