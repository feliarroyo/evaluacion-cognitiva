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

    // Start is called before the first frame update
    void Start()
    {
        // LockAndHideCursor(true)

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
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
        Gizmos.DrawWireCube(orientation.position, new(0.74f,2f,0.74f));
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
            if (joystick.Horizontal == 0 && joystick.Vertical == 0) { // If joystick not being used, used AWSD keys
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
        if (!isMoving && moveDirection != Vector3.zero){
            Logging.Log(Logging.EventType.PlayerMovementStart, new[] {(Object) gameObject.transform.position});
            isMoving = true;
        }
        if (isMoving && moveDirection == Vector3.zero){
            Logging.Log(Logging.EventType.PlayerMovementEnd, new[] {(Object) gameObject.transform.position});
            isMoving = false;
        }
        
        rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);
    }

    /// <summary>
    /// Limit maximum speed to a certain value.
    /// </summary>
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}
