using UnityEngine;

public class TouchController : MonoBehaviour
{
    public FixedTouchField _FixedTouchField;
    public CameraControl _cameraControl;
    public static bool allowCameraMovement = true;
    private bool isRotating = false;

    void Update()
    {
        if (allowCameraMovement)
        {
            _cameraControl.LockAxis = _FixedTouchField.TouchDist;
            if (!isRotating && _FixedTouchField.Pressed)
            {
                isRotating = true;
            }
            else if (isRotating && !_FixedTouchField.Pressed)
            {
                isRotating = false;
            }
        }
        else
        {
            _cameraControl.LockAxis = Vector2.zero;
        }
    }
}