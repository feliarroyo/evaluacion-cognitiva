using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

/// <summary>
/// This behaviour is used for the opening and closing of drawers. 
/// It moves the interactable gameObject on click on the corresponding direction.
/// </summary>
public class OpenDrawer : MonoBehaviour, IElementBehaviour
{
    public float speed;
    public Vector3 openPosition;
    public Vector3 closePosition;
    public float openingSize;

    private bool isOpen = false;
    private bool opening = false;
    private bool closing = false;

    public static List<OpenDrawer> allDrawers = new List<OpenDrawer>();

    // Item Spawn movement
    public List<ItemSpawn> spawnsContained;
    private Dictionary<ItemSpawn, Vector3> itemOffsets; // Stores original item positions relative to drawer

    public string id;

    // Start is called before the first frame update
    void Start()
    {
        closePosition = transform.localPosition;
        openPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z - openingSize);

        itemOffsets = new Dictionary<ItemSpawn, Vector3>();
        foreach (ItemSpawn item in spawnsContained)
        {
            itemOffsets[item] = item.transform.position - transform.position;
        }

        allDrawers.Add(this);
    }

    void OnDestroy(){
        allDrawers.Remove(this);
    }

    public static void EnableInteractions(bool enable){
        foreach (var drawer in allDrawers){
            drawer.GetComponent<Interactable>().EnableInteraction(enable);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (opening){
            MoveDrawer(openPosition);
        }
        if (Vector3.Distance(transform.localPosition, openPosition) < 0.0001f){
            opening = false;
            isOpen = true;
        }
        if (closing){
            MoveDrawer(closePosition);
        }
        if (Vector3.Distance(transform.localPosition, closePosition) < 0.0001f){
            closing = false;
            isOpen = false;
        }
    }

    private void MoveDrawer(Vector3 positionTowards){
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, positionTowards, Time.deltaTime * speed);
        foreach (ItemSpawn item in spawnsContained) {
            item.transform.position = transform.position + itemOffsets[item]; // Maintain original offset
            if (item.GetComponentInChildren<HeldItem>() != null) {
                item.GetComponentInChildren<HeldItem>().originalPosition = item.transform.position;
            }
        }
    }

    public void ClickBehaviour(GameObject go)
    {
        if (!isOpen){
            opening = true;
            Logging.Log(Logging.EventType.ElementOpen, new[] {(Object) id});
        }else{
            closing = true;
            Logging.Log(Logging.EventType.ElementClose, new[] {(Object) id});
        }
    }
}
