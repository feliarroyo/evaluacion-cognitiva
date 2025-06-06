using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycast : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, 5f)){
            if (hit.collider.gameObject.tag == "Drawer"){
                //Logging.DebugLog("hit");
                if (Input.GetMouseButtonDown(0)){
                    //hit.collider.gameObject.GetComponent<OpenDrawer>().DrawerIteraction();
                }
            }
        }else{
            //Logging.DebugLog("no hit");
        }
    }
}
