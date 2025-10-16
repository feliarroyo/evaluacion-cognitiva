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
    private float lastLogTime;
    [SerializeField] private Transform PlayerBody;
    public Vector2 LockAxis;
    public float sensitivity = 10f;
    private bool isMoving = false;
    private bool checkingStillness = false;

    void Start()
    {
        lastLogTime = Time.time;
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
        Vector2 currentCamera = new(Mathf.DeltaAngle(0, PlayerBody.rotation.eulerAngles.y), -Mathf.DeltaAngle(0, transform.rotation.eulerAngles.x));
        Vector2 rawMovement = new(XMove, YMove);
        Vector2 currentMovement = rawMovement.normalized;
        if (!isMoving)
        {

            Logging.Log(Logging.EventType.PlayerRotationStart, new[] { currentCamera, (Object)currentMovement });
            lastMovement = currentMovement;
            isMoving = true;
        }
        else
        {
            float angle = Vector2.Angle(lastMovement, currentMovement);
            if (!checkingStillness && LockAxis.x == 0 && LockAxis.y == 0)
            {
                checkingStillness = true;
                StartCoroutine(CheckIfStill(currentCamera));
            }
            else if (angle > 30f && Time.time - lastLogTime > 0.2f) // Only noticeable angles and within a reasonable time frame are considered changes
            {
                Logging.Log(Logging.EventType.PlayerRotationChange, new[] { (Object)currentCamera, currentMovement });
                lastMovement = currentMovement;
                lastLogTime = Time.time;
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
                checkingStillness = false;
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
                    checkingStillness = false;
                    yield break; // exit coroutine
                }
            }
        }
        checkingStillness = false;
    }

    // --- ROTATION COROUTINES USING SENSITIVITY ---

    public IEnumerator CameraMovementY(float targetX, float speed = 3f, float tolerance = 0.5f)
    {
        while (Mathf.Abs(XRotation - targetX) > tolerance)
        {
            XRotation = Mathf.MoveTowards(XRotation, targetX, speed * sensitivity * Time.deltaTime);
            yield return null;
        }
        XRotation = targetX;
    }

    public IEnumerator CameraMovementX(float targetY, float speed = 3f, float tolerance = 0.5f)
    {
        while (Mathf.Abs(Mathf.DeltaAngle(PlayerBody.eulerAngles.y, targetY)) > tolerance)
        {
            float newY = Mathf.MoveTowardsAngle(PlayerBody.eulerAngles.y, targetY, speed * sensitivity * Time.deltaTime);
            PlayerBody.rotation = Quaternion.Euler(0f, newY, 0f);
            yield return null;
        }
        PlayerBody.rotation = Quaternion.Euler(0f, targetY, 0f);
    }

    public IEnumerator CameraMovement(float targetY, float targetX, float speed = 3f, float tolerance = 0.5f)
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