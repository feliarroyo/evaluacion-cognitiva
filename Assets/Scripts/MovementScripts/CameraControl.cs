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

        transform.localRotation = Quaternion.Euler(XRotation, 0, 0);
        PlayerBody.Rotate(Vector3.up * XMove);
    }

    public void SetSensitivity(float value)
    {
        sensitivity = value;
    }

    public float GetXRotation()
    {
        return XRotation;
    }

    // --- ROTATION COROUTINES USING SENSITIVITY ---

    public IEnumerator RotateX(float targetX, float speed = 5f, float tolerance = 0.5f)
    {
        while (Mathf.Abs(XRotation - targetX) > tolerance)
        {
            XRotation = Mathf.MoveTowards(XRotation, targetX, speed * sensitivity * Time.deltaTime);
            yield return null;
        }
        XRotation = targetX;
    }

    public IEnumerator RotateY(float targetY, float speed = 5f, float tolerance = 0.5f)
    {
        while (Mathf.Abs(Mathf.DeltaAngle(PlayerBody.eulerAngles.y, targetY)) > tolerance)
        {
            float newY = Mathf.MoveTowardsAngle(PlayerBody.eulerAngles.y, targetY, speed * sensitivity * Time.deltaTime);
            PlayerBody.rotation = Quaternion.Euler(0f, newY, 0f);
            yield return null;
        }
        PlayerBody.rotation = Quaternion.Euler(0f, targetY, 0f);
    }

    public IEnumerator RotateXY(float targetY, float targetX, float speed = 5f, float tolerance = 0.5f)
    {
        while (Mathf.Abs(XRotation - targetX) > tolerance ||
               Mathf.Abs(Mathf.DeltaAngle(PlayerBody.eulerAngles.y, targetY)) > tolerance)
        {
            XRotation = Mathf.MoveTowards(XRotation, targetX, speed * sensitivity * Time.deltaTime);

            float newY = Mathf.MoveTowardsAngle(PlayerBody.eulerAngles.y, targetY, speed * sensitivity * Time.deltaTime);
            PlayerBody.rotation = Quaternion.Euler(0f, newY, 0f);

            yield return null;
        }

        XRotation = targetX;
        PlayerBody.rotation = Quaternion.Euler(0f, targetY, 0f);
    }
}