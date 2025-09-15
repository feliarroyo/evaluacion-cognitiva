using System.Collections;
using UnityEngine;
using Object = System.Object;

/// <summary>
/// This script controls the camera movement.
/// </summary>
public class CameraControl : MonoBehaviour
{
    private float XMove;
    private float YMove;
    private float XRotation;
    private Vector2 lastMovement = new(0, 0);
    [SerializeField] private Transform PlayerBody;
    public Vector2 LockAxis;
    public float sensitivity = 40f;
    private bool isMoving = false;
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
        
        if (isMoving || XMove != 0 || YMove != 0)
        {
            LogMovement();
        }
    }

    private void LogMovement()
    {
        Vector2 currentCamera = new(PlayerBody.rotation.y, -transform.localRotation.x);
        Vector2 currentMovement = new(XMove, YMove);
        if (!isMoving)
        {

            Logging.Log(Logging.EventType.PlayerRotationStart, new[] { currentCamera, (Object)currentMovement });
            lastMovement = currentMovement;
            isMoving = true;
        }
        else
        {
            if (LockAxis.x == 0 && LockAxis.y == 0)
            {
                
                StartCoroutine(CheckIfStill(currentCamera));
            }
            else if (Vector2.Distance(currentMovement, lastMovement) > 30f)
            {
                Logging.Log(Logging.EventType.PlayerRotationChange, new[] { (Object)currentCamera, currentMovement });
                lastMovement = currentMovement;
            }
        }

    }

    private IEnumerator CheckIfStill(Vector2 currentCamera)
    {
        float timer = 0f;
        while (isMoving)
        {
            if (LockAxis.x != 0 || LockAxis.y != 0)
            {
                yield break;
            }
            else
            {
                timer += Time.deltaTime;
                // If we've been still for long enough, stop
                if (timer >= 0.6f)
                {
                    Logging.Log(Logging.EventType.PlayerRotationEnd, new[] { (Object)currentCamera });
                    isMoving = false;
                    yield break; // exit coroutine
                }
            }
        }
    }

    public void SetSensitivity(float value)
    {
        sensitivity = value;
    }

    public float GetXRotation()
    {
        return XRotation;
    }

    public Vector2 GetRotationVector()
    {
        return new(XRotation, PlayerBody.rotation.y);
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