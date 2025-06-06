using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Purchasing;
using Object = System.Object;

public class TutorialItem : HeldItem
{
    public static bool playerIsClose;
    public static bool playerIsLookingBack;
    
    protected override void Start(){
        base.Start();
    }
}