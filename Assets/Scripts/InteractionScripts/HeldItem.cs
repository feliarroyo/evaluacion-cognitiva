using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HeldItem : MonoBehaviour, IElementBehaviour, IEquatable
{
    public string itemName; // Used to identify item in results.
    public bool isBeingHeld = false; // an item being currently held shouldn't be interactable.
    const float ROTATION_STRENGTH = 1f;
    const float ITEM_TRAVEL_SPEED = 7.5f; // how fast does the item move from one point to another.
    const float ROTATION_RETURN_STRENGTH = 4f; // how fast does the item return to its original rotation on return.
    public Vector3 originalPosition; // used to return items to their original position after being grabbed.
    public Quaternion originalRotation; // used to return items to their original rotation after being grabbed.
    public bool isMoving; // whether it is travelling to the player or not
    private bool isHeld = false;
    public bool isReturning = false; // whether it is returning to its original position or not
    public bool isKeyItem; // key items are those that the player needs to find
    public bool isEnvironmentItem; // environment items are those found during the activity
    public bool isLargeItem; // large items cannot spawn in small spawn points.
    public static HeldItem currentlyHeldItem = null;
    private const float tapTime = 0.3f; // time within a tap is considered as a click for storing items.
    private float heldTime = 0.0f; // time holding a press, used to determine if there's tapping or holding.
    public Sprite uiIcon;
    public Sprite uiIconNoBG;
    private LayerMask groundLayer;
    private LayerMask itemLayer;
    public List<SpawnTypeEntry> validSpawnTypes = new();
    public Camera mainCamera;
    // Define if the item to instantiate should be placed back
    public bool pushBackItem;
    public static List<HeldItem> itemsInScene = new();
    public Interactable interactable;


    protected virtual void Start()
    {
        mainCamera = Camera.main;
        interactable = GetComponent<Interactable>();
        // Initialize layer to be detected
        groundLayer = LayerMask.GetMask("Furniture", "Drawer");
        itemLayer = LayerMask.GetMask("Items");
        //if (validSpawnTypes[(int) ItemSpawning.SpawnType.wall].isValid && isEnvironmentItem){
        //  transform.Rotate(90f, 0f, -90f); // place over wall
        //  SnapObjectBack(transform.position + Vector3.back * 0.05f);

        // Shoot raycast to position item over surface, unless it's a rack item
        if (pushBackItem)
        {
            SnapObjectBack(transform.position + Vector3.back * 0.05f);
        }
        if (!validSpawnTypes[(int)ItemSpawning.SpawnType.rack].isValid)
        {
            SnapObjectDown(transform.position + Vector3.up * 0.05f);
        }
        // Caso de prueba libros, después generalizar de algún modo
        if (itemName == "Libro The Art of Netting")
        {
            SnapObjectLeft(transform.position + Vector3.left * 0.05f);
        }
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        itemsInScene.Add(this);
    }

    public bool Equals(HeldItem other)
    {
        return itemName.Equals(other.itemName);
    }

    public bool CheckVisibility()
    {
        return IsActuallyVisible(GetComponentInChildren<Renderer>(), Camera.main);
    }

    bool IsActuallyVisible(Renderer rend, Camera cam)
    {

        Vector3 viewportPos = cam.WorldToViewportPoint(rend.bounds.center);

        if (viewportPos.z < 0 || viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            return false;
        }
        Vector3 dir = (rend.bounds.center - cam.transform.position).normalized;

        if (Physics.Raycast(cam.transform.position, dir, out RaycastHit hit))
        {
            bool visible = hit.collider == GetComponent<Collider>();
            return visible;
        }
        return false;
    }

    private void OnDestroy()
    {
        itemsInScene.Remove(this);
    }

    public void SnapObjectLeft(Vector3 coord)
    {
        try
        {
            if (Physics.Raycast(coord, Vector3.back, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                float objectWidth = GetComponent<Collider>().bounds.extents.z;
                transform.position = new Vector3(coord.x, coord.y, hit.point.z + objectWidth);
                // transform.position = new Vector3(hit.point.x + objectWidth, coord.y, coord.z);
                originalPosition = transform.position;
                originalRotation = transform.rotation;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Raycast crash prevented: " + e);
        }
    }



    /// <summary>
    /// Snaps object to the ground on the position passed as parameter.
    /// </summary>
    /// <param name="position">Position where to place the object.</param>
    public void SnapObjectDown(Vector3 coord)
    {
        try
        {
            if (Physics.Raycast(coord, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                float objectHeight = GetComponent<Collider>().bounds.extents.y;
                transform.position = new Vector3(coord.x, hit.point.y + objectHeight, coord.z);
                transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                originalPosition = transform.position;
                originalRotation = transform.rotation;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Raycast crash prevented: " + e);
        }

    }

    public void SnapObjectBack(Vector3 coord)
    {
        try
        {
            if (Physics.Raycast(coord, Vector3.forward, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                float objectWidth = GetComponent<Collider>().bounds.extents.z;
                transform.position = new Vector3(coord.x, coord.y, hit.point.z - (objectWidth / 2));
                originalPosition = transform.position;
                originalRotation = transform.rotation;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Raycast crash prevented: " + e);
        }
    }


    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            OnTouchDown();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnTouchUp();
        }
#elif UNITY_ANDROID
            if (Input.touchCount > 0) {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) {
                    OnTouchDown();
                }
                else if (touch.phase == TouchPhase.Ended) {
                    OnTouchUp();
                }
            }
#endif
        if (isHeld)
        { // Allow rotation controls
            if (GameStatus.currentPhase < GameStatus.GamePhase.Waiting) //TUTORIAL
            {
                float rotX = TutorialManager.rotationX * ROTATION_STRENGTH;
                float rotY = TutorialManager.rotationY * ROTATION_STRENGTH;
                Quaternion horizontalRotation = Quaternion.AngleAxis(-rotX, mainCamera.transform.up);
                Quaternion verticalRotation = Quaternion.AngleAxis(rotY, mainCamera.transform.right);
                gameObject.transform.rotation = horizontalRotation * verticalRotation * transform.rotation;
            }
            else if (Input.GetMouseButton(0))
            {
                float rotX = GetRotationInputX() * ROTATION_STRENGTH;
                float rotY = GetRotationInputY() * ROTATION_STRENGTH;
                Quaternion horizontalRotation = Quaternion.AngleAxis(-rotX, mainCamera.transform.up);
                Quaternion verticalRotation = Quaternion.AngleAxis(rotY, mainCamera.transform.right);
                gameObject.transform.rotation = horizontalRotation * verticalRotation * transform.rotation;
            }
        }
    }

    private float GetRotationInputX()
    {
        if (Input.touchCount > 0) // Touch input (Android & Simulator)
        {
            return Input.GetTouch(0).deltaPosition.x;
        }
        else // Mouse input (Unity Editor)
        {
            return Input.GetAxis("Mouse X");
        }
    }

    private float GetRotationInputY()
    {
        if (Input.touchCount > 0) // Touch input (Android & Simulator)
        {
            return Input.GetTouch(0).deltaPosition.y;
        }
        else // Mouse input (Unity Editor)
        {
            return Input.GetAxis("Mouse Y");
        }
    }

    /// <summary>
    /// If the interaction is allowed, get the item to the center of the screen
    /// and show the item canvas.
    /// </summary>
    /// <param name="go">GameObject of the held item.</param>
    public void ClickBehaviour(GameObject go)
    {
        if (!isHeld)
        { // Grabbing item
            currentlyHeldItem = this;
            isMoving = true;
            GetComponent<Outline>().OutlineWidth = 0;
            PlayerMovement.allowPlayerMovement = false; // disable player movement while holding an item.
            TouchController.allowCameraMovement = false; // disable camera movement while holding an item.
            Interactable.allowAllInteractions = false; // disable all interactions while the item is active.
            ItemInteraction.EnableButton(true); // Allow item to be left.
            // Log item
            Logging.Log(Logging.EventType.ItemGrab, new[] { gameObject.GetComponent<HeldItem>().itemName });
            // Move to the center of the screen.
            StartCoroutine(BringItemToPlayer());
            isHeld = true;
        }
    }

    /// <summary>
    /// Brings the item to the player.
    /// </summary>
    public IEnumerator BringItemToPlayer()
    {
        GetComponent<Collider>().enabled = false; // disable collision to avoid issues with items in the way
        while (Vector3.Distance(transform.position, mainCamera.transform.TransformPoint(Vector3.forward * 1)) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, mainCamera.transform.TransformPoint(Vector3.forward), ITEM_TRAVEL_SPEED * Time.deltaTime);
            yield return null;
        }
        isMoving = false;
    }

    /// <summary>
    /// Returns item to its original position and rotation.
    /// </summary>
    /// <returns></returns>
    public IEnumerator ReturnItemToSpawn()
    {
        GetComponent<Collider>().enabled = false;
        while (Vector3.Distance(transform.position, originalPosition) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, ITEM_TRAVEL_SPEED * Time.deltaTime);
            yield return null;
        }
        while (Quaternion.Angle(transform.rotation, originalRotation) > 1f)
        {
            float totalRotation = Quaternion.Angle(transform.rotation, originalRotation);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, originalRotation, ROTATION_RETURN_STRENGTH * totalRotation * Time.deltaTime);
            yield return null;
        }
        isReturning = false;
        GetComponent<Collider>().enabled = true;
        GetComponent<Outline>().OutlineWidth = 8;
        if (Logging.currentLog.heldItem == gameObject.GetComponent<HeldItem>().itemName)
        {
            Logging.Log(Logging.EventType.NoItem, null);
        }
    }

    /// <summary>
    /// Tracks press time to identify if it's a touch or a hold.
    /// </summary>
    public void OnTouchDown()
    {
        heldTime = Time.time;
    }

    /// <summary>
    /// If the item if being held and the time pressing it was short enough, store the item.
    /// </summary>
    public void OnTouchUp()
    {
        // Debug.Log("Held TIME: " + heldTime + " Current Time: " + Time.time);
        if ((Time.time - heldTime < tapTime) && isHeld && !isMoving && AllowToStoreItems())
        {
            Debug.Log("Enters if STORE");
            PlayerMovement.allowPlayerMovement = true;
            TouchController.allowCameraMovement = true;
            StoreItem();
            ItemInteraction.EnableButton(false);
        }
        ;
    }

    /// <summary>
    /// Returns the held item to its original position, and allow interaction with other elements once again.
    /// </summary>
    public static void ReturnItem()
    {
        Logging.Log(Logging.EventType.ItemReturn, new[] { currentlyHeldItem.itemName });
        ItemInteraction.EnableButton(false);
        currentlyHeldItem.isMoving = false;
        currentlyHeldItem.isHeld = false;
        currentlyHeldItem.StartCoroutine(currentlyHeldItem.ReturnItemToSpawn());
        currentlyHeldItem.isReturning = true;
        currentlyHeldItem = null;
        if (GameStatus.currentPhase > GameStatus.GamePhase.Waiting)
        {
            Interactable.allowAllInteractions = true;
        }
    }
    /// <summary>
    /// An item can be stored only if time is not over and if it was placed on the testing environment
    /// </summary>
    /// <returns>True if the item can be stored.</returns>
    public static bool AllowToStoreItems()
    {
        try
        {
            Debug.Log("Environment Item: " + currentlyHeldItem.GetComponent<HeldItem>().isEnvironmentItem);
            Debug.Log("Is timer over: " + Timer.IsTimeOver());
            Debug.Log("Current difficulty: " + Settings.currentDifficulty);

            return currentlyHeldItem.GetComponent<HeldItem>().isEnvironmentItem && (!Timer.IsTimeOver() || Settings.currentDifficulty == Settings.Difficulty.Preevaluación);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Stores the currently held item.
    /// </summary>
    public static bool StoreItem()
    {
        if (currentlyHeldItem != null)
        {
            Logging.Log(Logging.EventType.ItemStore);
            GameStatus.SaveItem(currentlyHeldItem.GetComponent<HeldItem>());
            // Destroys the gameObject in the scene.
            StoredItemCoroutine.instance.CheckNoItem(currentlyHeldItem.itemName);
            currentlyHeldItem.gameObject.SetActive(false);
            currentlyHeldItem = null;
            if (GameStatus.currentPhase > GameStatus.GamePhase.Waiting)
            {
                Interactable.allowAllInteractions = true;
            }
            return true;

        }
        return false;
    }



    /// <summary>
    /// Checks whether a determined spawn type is recommended for this item.
    /// </summary>
    /// <param name="spawnType">Type of spawn to check.</param>
    /// <returns>Whether the spawn type is recommended for this kind of item.</returns>
    public bool CanUseSpawnType(ItemSpawning.SpawnType spawnType)
    {
        return validSpawnTypes[(int)spawnType].isValid;
    }

    public IEnumerator RotateItemOverTime(float rotateXDegrees, float rotateYDegrees, float rotateZDegrees = 0, float duration = 1f)
    {
        if (currentlyHeldItem == null || duration <= 0f) yield break;

        Quaternion startRotation = currentlyHeldItem.transform.localRotation;

        // Target rotation based on requested X (pitch) and Y (yaw)
        Quaternion targetRotation = Quaternion.Euler(
            currentlyHeldItem.transform.localEulerAngles.x + rotateXDegrees,
            currentlyHeldItem.transform.localEulerAngles.y + rotateYDegrees,
            currentlyHeldItem.transform.localEulerAngles.z + rotateZDegrees // keep Z as is
        );

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            currentlyHeldItem.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        // Ensure exact final rotation
        currentlyHeldItem.transform.rotation = targetRotation;
    }
}

internal interface IEquatable
{
}

[Serializable]
public class SpawnTypeEntry
{
    public ItemSpawning.SpawnType spawnType;
    public bool isValid;
}