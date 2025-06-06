using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour
{
    public FixedTouchField _FixedTouchField;
    public CameraControl _cameraControl;
    public static bool allowCameraMovement = true;
    void Start()
    {
        
    }

    
    void Update()
    {
        if (allowCameraMovement){
            _cameraControl.LockAxis = _FixedTouchField.TouchDist;
        }
    }
}
