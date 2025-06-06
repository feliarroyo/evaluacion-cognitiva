using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This script controls the camera movement.
/// </summary>
public class CameraControl : MonoBehaviour
{
    private float XMove;
    private float YMove;
    private float XRotation;
    [SerializeField] private Transform PlayerBody;
    public Vector2 LockAxis;
    public float sensitivity = 40f;
    void Start()
    {
        
    }

    
    void Update()
    {
        XMove = LockAxis.x * sensitivity * Time.deltaTime;
        YMove = LockAxis.y * sensitivity * Time.deltaTime;
        XRotation -= YMove;
        XRotation = Mathf.Clamp(XRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(XRotation,0,0);
        PlayerBody.Rotate(Vector3.up * XMove);
    }

    public void SetSensitivity(float value){
        sensitivity = value;
    }
}