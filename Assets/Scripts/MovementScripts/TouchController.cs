using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

public class TouchController : MonoBehaviour
{
    public FixedTouchField _FixedTouchField;
    public CameraControl _cameraControl;
    public static bool allowCameraMovement = true;
    private bool isRotating = false;
    void Start()
    {
        
    }

    
    void Update()
    {
        if (allowCameraMovement){
            _cameraControl.LockAxis = _FixedTouchField.TouchDist;
            if (!isRotating && _FixedTouchField.Pressed) {
                Logging.Log(Logging.EventType.PlayerRotationStart, new[] { (Object) gameObject.transform.rotation });
                isRotating = true;
            }
            else if (isRotating && !_FixedTouchField.Pressed) {
                Logging.Log(Logging.EventType.PlayerRotationEnd, new[] { (Object) gameObject.transform.rotation });
                isRotating = false;
            }
        }
    }
}
