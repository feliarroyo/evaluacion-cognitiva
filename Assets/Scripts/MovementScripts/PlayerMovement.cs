using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System;
using Object = System.Object;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public Transform orientation;
    public float horizontalInput;
    public float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;
    public static bool allowPlayerMovement = true;
    public Joystick joystick;
    private bool isMoving = false;
    public bool forceStop = false;
    public static Vector3 currentPosition;
    Vector3 lastDirection;
    private readonly Vector3 gizmo = new(0.74f, 2f, 0.74f);

    // Start is called before the first frame update
    void Start()
    {
        // LockAndHideCursor(true)
        currentPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        currentPosition = transform.position;
        if (allowPlayerMovement)
        {
            MyInput();
            SpeedControl();
        }
    }

    void OnDrawGizmos()
    {
        // Choose different colors for big and small spawn points
        Gizmos.color = Color.yellow;

        // Draw a sphere to indicate spawn points
        Gizmos.DrawWireCube(orientation.position, gizmo);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    /// <summary>
    /// Obtains input from the virtual joystick.
    /// </summary>
    private void MyInput()
    {
        horizontalInput = joystick.Horizontal;
        verticalInput = joystick.Vertical;
        // Windows Editor Controls
#if UNITY_EDITOR_WIN
        if (joystick.Horizontal == 0 && joystick.Vertical == 0)
        { // If joystick not being used, used AWSD keys
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
        }
#endif
    }

    /// <summary>
    /// Executes movement according to the values previously obtained.
    /// </summary>
    private void MovePlayer()
    {
        if (forceStop)
        {
            rb.velocity = Vector3.zero;
            return;
        }
        moveDirection = verticalInput * orientation.forward + horizontalInput * orientation.right;
        LogMovement();
        rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);
    }

    private void LogMovement()
    {
        if (!isMoving && moveDirection != Vector3.zero)
        { // Empieza a moverse
            Logging.Log(Logging.EventType.PlayerMovementStart, new[] { (Object)gameObject.transform.position, moveDirection });
            lastDirection = moveDirection.normalized;
            isMoving = true;
        }
        if (isMoving)
        {
            if (moveDirection == Vector3.zero) // Deja de moverse
            {
                Logging.Log(Logging.EventType.PlayerMovementEnd, new[] { (Object)gameObject.transform.position });
                isMoving = false;
            }
            else if (Vector3.Distance(moveDirection.normalized, lastDirection) > 0.25f)
            {
                Logging.Log(Logging.EventType.PlayerMovementChange, new[] { (Object)gameObject.transform.position, moveDirection.normalized });
                lastDirection = moveDirection.normalized;
            }
        }
    }
    /// <summary>
    /// Limit maximum speed to a certain value.
    /// </summary>
    private void SpeedControl()
    {
        float flatVelX = rb.velocity.x;
        float flatVelZ = rb.velocity.z;
        float flatMag = Mathf.Sqrt(flatVelX * flatVelX + flatVelZ * flatVelZ);

        if (flatMag > moveSpeed)
        {
            float normX = flatVelX / flatMag;
            float normZ = flatVelZ / flatMag;
            float limitedX = normX * moveSpeed;
            float limitedZ = normZ * moveSpeed;
            rb.velocity = new Vector3(limitedX, rb.velocity.y, limitedZ);
        }
    }

    public static void AllowMovement(bool allow)
    {
        allowPlayerMovement = allow;
        TouchController.allowCameraMovement = allow;
    }
}
