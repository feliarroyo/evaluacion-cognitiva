using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class ClickItemTester : MonoBehaviour
{
    public ItemTester tester;
    public bool previous;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnMouseDown()
    {
        if (previous) {
            tester.DisplayPreviousItem();
        }
        else {
            tester.DisplayNextItem();
        }
    }
}
