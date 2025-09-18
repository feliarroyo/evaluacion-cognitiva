using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class TutorialManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds1_5 = new WaitForSeconds(1.5f);
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    private static WaitForSeconds _waitForSeconds2 = new WaitForSeconds(2f);
    private static WaitForSeconds _waitForSecondsWall = new WaitForSeconds(0.2f);
    public static TutorialManager instance;
    public GameObject mission;
    // private TextMeshProUGUI missionText;
    public OpenDoor houseDoor;
    public OpenDoor tvDoor1;
    public OpenDoor tvDoor2;
    public OpenDoor oppositeDoor1;
    public OpenDoor oppositeDoor2;
    public OpenDoor oppositeDoor3;
    public OpenDrawer houseDrawer;
    public GameObject player;
    public CameraControl cam;
    public GameObject roomIndicators; // arrows showing the way to the search room.
    public GameObject tutorialElements;
    public RectTransform handle;
    public List<HeldItem> items = new();
    public static float rotationX = 0.0f;
    public static float rotationY = 0.0f;
    const int DEFAULT_SPEED = 2; // velocidad default
    const float SLOW_SPEED = 1.5f; // velocidad más lenta
    const float LOOKDOWN_TIME = 1.4f;
    const float CAMERA_ROT_SPEED = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        // if (instance == null)
        // {
        //     instance = this;
        // }
        Debug.Log("La fase en este momento es : " + GameStatus.currentPhase);
        // missionText = mission.GetComponentInChildren<TextMeshProUGUI>();
        // In the tutorial, certain objects must spawn, and user control is disabled
        bool inTutorial = GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Start;
        tutorialElements.SetActive(inTutorial);
        PlayerMovement.allowPlayerMovement = !inTutorial;
        TouchController.allowCameraMovement = !inTutorial;
        cam = player.GetComponentInChildren<CameraControl>();
        if (inTutorial)
        {
            cam.sensitivity = 15;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void StartMission()
    {
        //missionText.text = text;
        //mission.SetActive(true);
        Time.timeScale = 1;
    }

    private void StopMission()
    {
        Time.timeScale = 0;
        mission.SetActive(false);
    }

    private void DisablePlayerInput()
    {
        TouchController.allowCameraMovement = false;
        PlayerMovement.allowPlayerMovement = false;
    }
    private void MovePlayerVertical(bool isForwardMovement = true)
    {
        //player.GetComponent<PlayerMovement>().forceStop = false;
        player.GetComponent<PlayerMovement>().verticalInput = isForwardMovement ? 1f : -1f;
        handle.transform.localPosition = new Vector3(0, isForwardMovement ? 100 : -100, 0);
    }

    private IEnumerator RotateHeldItem(float x, float y, float duration)
    {
        yield return StartCoroutine(HeldItem.currentlyHeldItem.RotateItemOverTime(x, y, duration));
    }

    private Quaternion LookRotation(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - player.transform.position).normalized;
        direction.y = 0f; // keep only horizontal rotation
        if (direction != Vector3.zero)
            return Quaternion.LookRotation(direction);
        else
            return new();
    }

    // private IEnumerator LookPlayerToTarget(Vector3 targetPosition, bool turnRight = true, float stopAngle = 1f)
    // {
    //     cam.LockAxis.x = turnRight ? 5f : -5f;
    //     Debug.Log("ROTATION: " + player.transform.rotation + "TARGET: " + LookRotation(targetPosition));
    //     Quaternion target = LookRotation(targetPosition);
    //     yield return new WaitUntil(() => Quaternion.Angle(player.transform.rotation, target) < stopAngle);
    //     cam.LockAxis.x = 0f;
    // }

    private IEnumerator MovePlayerToTarget(Vector3 targetPosition, float stopDistance = 0.15f)
    {
        MovePlayerVertical();
        Debug.Log("DISTANCE: " + Vector3.Distance(player.transform.position, targetPosition));
        yield return new WaitUntil(() =>
            Vector3.Distance(player.transform.position, targetPosition) <= stopDistance);

        StopPlayerMovement();
    }
    private IEnumerator MovePlayerToTargetX(float targetPositionX, float stopDistance = 0.15f)
    {
        MovePlayerVertical();
        Debug.Log("DISTANCE: " + (player.transform.position.x - targetPositionX));
        yield return new WaitUntil(() =>
            Mathf.Abs(player.transform.position.x - targetPositionX) <= stopDistance);

        StopPlayerMovement();
    }

    private IEnumerator MovePlayerToTargetHorizontal(Vector3 targetPosition, float stopDistance = 0.15f)
    {
        MovePlayerHorizontal(false);
        Debug.Log("ENTER");
        yield return new WaitUntil(() =>
            Vector3.Distance(player.transform.position, targetPosition) <= stopDistance);
        Debug.Log("DISTANCE: " + Vector3.Distance(player.transform.position, targetPosition));
        StopPlayerMovement();
    }

    private void MovePlayerHorizontal(bool isRightMovement = true)
    {
        //player.GetComponent<PlayerMovement>().forceStop = false;
        player.GetComponent<PlayerMovement>().horizontalInput = isRightMovement ? 1f : -1f;
        handle.transform.localPosition = new Vector3(isRightMovement ? 100 : -100, 0, 0);
    }

    private void StopPlayerMovement()
    {
        player.GetComponent<PlayerMovement>().verticalInput = 0;
        player.GetComponent<PlayerMovement>().horizontalInput = 0;
        handle.transform.localPosition = new Vector3(0, 0, 0);
        //player.GetComponent<PlayerMovement>().forceStop = true;
    }

    public void CameraMovementByStrength(float xAxis, float yAxis = 0)
     {
         player.GetComponentInChildren<CameraControl>().LockAxis.x = xAxis;
         player.GetComponentInChildren<CameraControl>().LockAxis.y = yAxis;
     }

    public IEnumerator CameraMovement(float targetX, float targetY, float speed = 3)
    {
        yield return cam.RotateXY(targetX,targetY,speed);
    }

    public IEnumerator CameraMovementX(float targetX, float speed = 3)
    {
        yield return cam.RotateY(targetX, speed);
    }

    public IEnumerator CameraMovementY(float targetY, float speed = 3)
    {
        yield return cam.RotateX(targetY, speed);
    }


    public void GetItem(int index)
    {
        items[index].GetComponentInChildren<HeldItem>().ClickBehaviour(items[index].gameObject);
    }

    private void StoreItem()
    {
        HeldItem.StoreItem();
        Interactable.allowAllInteractions = false;
    }

    private void ReturnItem()
    {
        HeldItem.ReturnItem();
        Interactable.allowAllInteractions = false;
    }

    public void DisableItemInteractions(List<HeldItem> items)
    {
        foreach (HeldItem hi in items)
        {
            DisableInteractionOnItem(hi);
        }
    }

    private void DisableInteractionOnItem(HeldItem it)
    {
        it.gameObject.GetComponent<Outline>().enabled = false;
        it.gameObject.GetComponent<Interactable>().isInteractable = false;
        it.gameObject.GetComponent<Interactable>().stoppedInteraction = true;
    }

    private IEnumerator ShowPopups(PopUpManager popups, string msg)
    {
        StopMission();
        yield return popups.ShowPopups(msg);
        StartMission();
    }

    private IEnumerator ShowPopups(PopUpManager popups, string[] msg)
    {
        StopMission();
        yield return popups.ShowPopups(msg);
        StartMission();
    }

    /// <summary>
    /// Walks from the starting point to the house, explaining how to move forward along the way.
    /// </summary>
    private IEnumerator WalkToHouse(PopUpManager popups)
    {
        yield return ShowPopups(popups, new string[] { // #1-#3
            "En este tutorial, se describen\nel ambiente en el que se desarrolla el juego,\ny las tareas a ser realizadas en este ambiente.",
            "Al comienzo del juego,\nse verá la fachada de una casa.",
            "Se debe avanzar hacia la puerta de la casa,\ndeslizando el control izquierdo hacia arriba."
        });
        player.GetComponent<PlayerMovement>().moveSpeed = DEFAULT_SPEED;
        yield return _waitForSeconds1;
        MovePlayerVertical();
        yield return new WaitUntil(() => houseDoor.GetComponent<Interactable>().isInteractable);
        yield return new WaitForSeconds(0.1f);
        GameStatus.currentPhase = GameStatus.GamePhase.Tutorial_ReachApple;
        StopPlayerMovement();
        yield return _waitForSeconds1;
    }

    /// <summary>
    /// Explain how to open doors, and enters inside the house.
    /// </summary>
    private IEnumerator EnterHouse(PopUpManager popups)
    {
        yield return ShowPopups(popups, "Cuando la puerta tenga un borde blanco,\nse debe abrirla tocando sobre ella."); // #4
        yield return _waitForSeconds1;
        houseDoor.ClickBehaviour(houseDoor.gameObject);
        yield return new WaitUntil(() => houseDoor.GetComponent<OpenDoor>().isMoving);
        yield return new WaitUntil(() => !houseDoor.GetComponent<OpenDoor>().isMoving);
        yield return _waitForSeconds1;
        yield return ShowPopups(popups, new string[] { // #5-#6
            "Cuando se atraviese la puerta de la casa,\nse encenderá la luz del hall.\nEn ese momento, se verán objetos en los estantes del hall,\ny se tendrá un tiempo límite para observarlos.",
            "Para que se encienda la luz del hall,\nse debe avanzar hacia los objetos en los estantes."
        });
        yield return _waitForSeconds1;
        MovePlayerVertical();
        yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Memorizing);
        yield return new WaitForSeconds(0.4f);
        StopPlayerMovement();
        player.GetComponent<PlayerMovement>().forceStop = true;
        yield return new WaitForSeconds(0.1f);
        // player.GetComponent<PlayerMovement>().moveSpeed = DEFAULT_SPEED;
    }

    /// <summary>
    /// Starts memorizing phase, and demonstrates player movement.
    /// </summary>
    private IEnumerator PlayerMovementDemonstration(PopUpManager popups)
    {
        const float MOVE_TIME = 0.4f;
        const float STOP_TIME = 0.75f;
        yield return ShowPopups(popups, new string[] {  // #7-#8
            "Al encenderse la luz del hall,\ncomenzará a correr el tiempo límite\npara observar los objetos,\nen el reloj que se muestra en la pantalla.",
            "Es posible moverse en todas las direcciones del hall\n(adelante, atrás, izquierda y derecha),\ndeslizando el control izquierdo hacia la dirección deseada."
        });

        yield return new WaitForSeconds(0.5f);
        player.GetComponent<PlayerMovement>().forceStop = false;
        // player.GetComponent<PlayerMovement>().moveSpeed = SLOW_SPEED;
        MovePlayerHorizontal(true);
        yield return new WaitForSeconds(MOVE_TIME);
        StopPlayerMovement();
        yield return new WaitForSeconds(STOP_TIME);
        MovePlayerHorizontal(false);
        yield return new WaitForSeconds(MOVE_TIME);
        StopPlayerMovement();
        yield return new WaitForSeconds(STOP_TIME);
        MovePlayerVertical(false);
        yield return new WaitForSeconds(MOVE_TIME);
        StopPlayerMovement();
        yield return new WaitForSeconds(STOP_TIME);
        MovePlayerVertical(true);
        yield return new WaitForSeconds(MOVE_TIME);
        StopPlayerMovement();
        yield return new WaitForSeconds(STOP_TIME);
    }

    /// <summary>
    /// Shows how to move the camera in every direction.
    /// </summary>
    private IEnumerator CameraDemonstration(PopUpManager popups)
    {
        const float WAIT_SPEED = 0.5f;
        yield return ShowPopups(popups, "Es posible mirar hacia todas las direcciones del hall\n(arriba, abajo, izquierda y derecha)\ndeslizando sobre la pantalla en la dirección deseada.");  // #9
        int[] spinDegrees = {30, 0, -30, 0};
        foreach (int i in spinDegrees)
        {
            yield return CameraMovementX(i);
            yield return new WaitForSeconds(WAIT_SPEED);
        }
        foreach (int i in spinDegrees)
        {
            yield return CameraMovementY(-i);
            yield return new WaitForSeconds(WAIT_SPEED);
        }
    }

    /// <summary>
    /// Shows how to rotate and leave an item.
    /// </summary>
    private IEnumerator ItemRotationDemonstration(PopUpManager popups)
    {
        const float WAIT_SPEED = 0.7f;
        const int ITEMID_CAMERA = 0;
        yield return ShowPopups(popups, new string[] { // #10-#11
            "Se debe observar los objetos en el hall,\ny tratar de memorizarlos,\nantes de que se acabe el tiempo límite.",
            "Cuando un objeto tenga un borde blanco,\nes posible observarlo con más detalle,\ntocando sobre el objeto."
        });
        yield return _waitForSeconds1;
        GetItem(ITEMID_CAMERA);
        yield return _waitForSeconds1;
        yield return ShowPopups(popups, "Es posible girar un objeto en distintas direcciones\n(izquierda, derecha, arriba y abajo),\ndeslizando sobre el objeto en la dirección deseada."); // #12
        yield return new WaitForSeconds(WAIT_SPEED); 
        yield return RotateHeldItem(-90, 0, WAIT_SPEED);
        yield return new WaitForSeconds(WAIT_SPEED); 
        yield return RotateHeldItem(90, 0, WAIT_SPEED);
        yield return new WaitForSeconds(WAIT_SPEED);
        yield return RotateHeldItem(90, 0, WAIT_SPEED);
        yield return new WaitForSeconds(WAIT_SPEED);
        yield return RotateHeldItem(-90, 0, WAIT_SPEED);
        yield return new WaitForSeconds(WAIT_SPEED); 
        yield return RotateHeldItem(0, 90, WAIT_SPEED);
        yield return new WaitForSeconds(WAIT_SPEED);
        yield return RotateHeldItem(0, -90, WAIT_SPEED);
        yield return new WaitForSeconds(WAIT_SPEED);
        yield return RotateHeldItem(0, -90, WAIT_SPEED);
        yield return new WaitForSeconds(WAIT_SPEED);
        yield return RotateHeldItem(0, 90, WAIT_SPEED);
        yield return new WaitForSeconds(WAIT_SPEED);
        yield return ShowPopups(popups, "Para que el objeto vuelva a su lugar en el estante,\nse debe tocar el botón rojo que se muestra en la pantalla.");  // #13
        yield return new WaitForSeconds(0.5f);
        ReturnItem();
    }

    /// <summary>
    /// Once the memorizing phase ends, goes through the hallway and into the living.
    /// </summary>
    private IEnumerator GoThroughHallway(PopUpManager popups)
    {
        yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_BeforeSearch);
        DisableItemInteractions(new List<HeldItem>() { items[0], items[1] });
        SwapMaterial.SetMaterials(false);
        yield return new WaitForSeconds(0.5f);
        yield return ShowPopups(popups, "Cuando el tiempo límite termine,\nla luz del hall se apagará,\ny se deberá avanzar hacia el pasillo de la casa."); // #14
        yield return CameraMovementX(90);
        Vector3 passageStart = new(-48.63f, player.transform.position.y, player.transform.position.z); // -48.45 -> -48.65 -> -48.55 -> -48.62 (no choca)
        yield return _waitForSeconds1;
        yield return MovePlayerToTarget(passageStart);
        yield return _waitForSecondsWall; // new WaitForSeconds(0.5f);
        player.transform.position = new Vector3(-48.249752f, player.transform.position.y, player.transform.position.z);
        yield return CameraMovementX(0);
        yield return _waitForSeconds1;
        //yield return LookPlayerToTarget(passageEnd, false);
        //CameraMovementX(-5f);
        //yield return _waitForSeconds1;
        //CameraMovementX(0f);
        yield return ShowPopups(popups, "Se debe avanzar por el pasillo de la casa\nhacia la entrada de la sala de living."); // #15
        // player.GetComponent<PlayerMovement>().moveSpeed = SLOW_SPEED;
        // yield return new WaitForSeconds(0.6f);
        yield return _waitForSeconds1;
        Vector3 passageEnd = new(player.transform.position.x, player.transform.position.y, -92f);
        yield return MovePlayerToTarget(passageEnd);
        // player.GetComponent<PlayerMovement>().moveSpeed = DEFAULT_SPEED;
        yield return _waitForSeconds1;
    }

    /// <summary>
    /// Goes into the living room.
    /// </summary>
    private IEnumerator EnterLiving(PopUpManager popups)
    {
        yield return ShowPopups(popups, "Cuando se atraviese la entrada de la sala de living,\nla luz de la sala se encenderá.\nA partir de ese momento, se deberá recorrer la sala,\nobservar los objetos distribuídos en ella,\ny seleccionar a los objetos que estaban en el hall."); // #16
        // MovePlayerVertical();
        // yield return new WaitForSeconds(0.3f);
        Vector3 livingEntrance = new(player.transform.position.x, player.transform.position.y, -89.5f);
        MovePlayerVertical();
        yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Search);
        yield return new WaitForSeconds(0.2f);
        player.GetComponent<PlayerMovement>().forceStop = true;
        yield return new WaitForSeconds(0.1f);
        player.GetComponent<PlayerMovement>().forceStop = false;
        yield return ShowPopups(popups, "Al encenderse la luz de la sala,\ncomenzará a correr el tiempo límite\npara recorrer la sala y seleccionar los objetos,\nen el reloj que se muestra en la pantalla."); // #17
        //yield return new WaitForSeconds(0.2f);
        //StopPlayerMovement();
    }

    /// <summary>
    /// Looks at the entrance furniture, and manipulates the item above it.
    /// </summary>
    private IEnumerator ItemInteractionDemonstration(PopUpManager popups)
    {
        Vector3 centerPosition = new(player.transform.position.x, player.transform.position.y, -86.81f); // -86.2 -> -86.4 -> -86.8
        const int ITEMID_ENTRANCEITEM = 2;
        yield return _waitForSeconds1;
        yield return MovePlayerToTarget(centerPosition);
        yield return _waitForSeconds1;
        //Vector3 livingSidestep = new(-49.2f, player.transform.position.y, player.transform.position.z); // un pasito para atrás para que se vea todo el mueble
        //yield return MovePlayerToTargetHorizontal(livingSidestep, 0.5f);
        // yield return _waitForSeconds1;
        yield return CameraMovement(90f, 30f);
        yield return ShowPopups(popups, "Cuando un objeto tenga un borde blanco,\nes posible observarlo con más detalle,\ntocando sobre el objeto."); // #18
        yield return _waitForSeconds1;
        GetItem(ITEMID_ENTRANCEITEM);
        yield return _waitForSeconds1;
        yield return ShowPopups(popups, "Es posible girar un objeto que está siendo observado,\nen distintas direcciones (izquierda, derecha, arriba y abajo),\ndeslizando sobre el objeto en la dirección deseada."); // #19
        yield return _waitForSeconds1;
        yield return RotateHeldItem(-90, 0, 1f);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(90, 0, 1f);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(0, -90, 1f);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(0, 90, 1f);
        yield return new WaitForSeconds(0.5f);
        yield return ShowPopups(popups, "Para que el objeto que está siendo observado vuelva a su lugar,\nse debe tocar el botón rojo que se muestra en la pantalla."); // #20
        yield return _waitForSeconds1;
        ReturnItem();
        yield return new WaitForSeconds(3f);
        GetItem(ITEMID_ENTRANCEITEM);
        yield return _waitForSeconds1;
        yield return ShowPopups(popups, "Para seleccionar el objeto que está siendo observado,\nse debe tocar nuevamente sobre el objeto."); // #21
        yield return _waitForSeconds2;
        StoreItem();
        yield return ShowPopups(popups, "Los objetos seleccionados se muestran\nen la parte superior derecha de la pantalla."); // #22
        yield return _waitForSeconds1;
        yield return CameraMovement(0, 0); // volver a mirar al frente
        yield return _waitForSeconds1;
    }

    // /// <summary>
    // /// Goes to the corner with the rack, and looks at the entire room from there.
    // /// </summary>
    // private IEnumerator LookFromCorner(PopUpManager popups)
    // {
    //     Vector3 cornerPosition = new(player.transform.position.x, player.transform.position.y, -83);
    //     yield return CameraMovementX(45, 3); // mirar al perchero
    //     yield return _waitForSeconds1;
    //     yield return CameraMovementX(-120, 3); // Mostrar vista de la sala
    //     yield return _waitForSeconds1;
    //     yield return CameraMovementX(0, 3); // mirar al frente
    //     yield return _waitForSeconds1;
    // }

    /// <summary>
    /// Moves from the rack corner to the drawers in the opposite corner.
    /// </summary>
    private IEnumerator GoToOppositeCorner(PopUpManager popups)
    {
        // yield return CameraMovementX(45, 3); // mirar al perchero
        // yield return _waitForSeconds1;
        // yield return CameraMovementX(0, 3); // mirar al frente
        // yield return _waitForSeconds1;
        Vector3 rackCorner = new(player.transform.position.x, player.transform.position.y, -84.4f); // -83.7 -> -83.9 -> -84.4
        yield return MovePlayerToTarget(rackCorner);
        yield return _waitForSecondsWall;
        player.GetComponent<PlayerMovement>().moveSpeed = SLOW_SPEED;
        yield return CameraMovementX(-90); // mirar a pared opuesta
        Vector3 oppPosition = new(-54.5f, player.transform.position.y, player.transform.position.z); // -54.5 -> -55 -> -54.8 -> -54.7 -> -54.5
        yield return _waitForSeconds1;
        yield return MovePlayerToTarget(oppPosition);
        yield return _waitForSecondsWall;
        player.GetComponent<PlayerMovement>().moveSpeed = DEFAULT_SPEED;
        //Vector3 oppSidestep = new(player.transform.position.x, player.transform.position.y, -85.1f); // -85 -> -85.1
        //yield return MovePlayerToTargetHorizontal(oppSidestep, 1f);
        // MovePlayerHorizontal(false);
        // yield return new WaitForSeconds(0.2f);
    }

    /// <summary>
    /// Opens the first drawer, and manipulates the item within it.
    /// </summary>
    private IEnumerator DrawersDemonstration(PopUpManager popups)
    {
        const int ITEMID_DRAWERITEM = 3;
        StopPlayerMovement();
        yield return CameraMovementY(45);
        yield return new WaitForSeconds(LOOKDOWN_TIME);
        yield return ShowPopups(popups, "Cuando un cajón de algún mueble tenga un borde blanco,\nse podrá abrirlo tocando sobre él."); // #23
        yield return _waitForSeconds1;
        houseDrawer.ClickBehaviour(houseDrawer.gameObject);
        yield return _waitForSeconds2;
        GetItem(ITEMID_DRAWERITEM);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(30, 0, 1f);
        yield return _waitForSeconds1;
        ReturnItem();
        yield return _waitForSeconds1;
        houseDrawer.ClickBehaviour(houseDrawer.gameObject);

        yield return _waitForSeconds2;
    }

    private IEnumerator GoThroughOpposite(PopUpManager popups)
    {
        const int ITEMID_OPPITEM = 8;
        const int ITEMID_SHELFITEM = 9;
        Vector3 centerPosition = new(player.transform.position.x, player.transform.position.y, -86.85f); // -86.2 -> -86.6 -> -86.85
        // ir al centro
        yield return MovePlayerToTargetHorizontal(centerPosition, 1f);
        // MovePlayerHorizontal(false);
        // yield return new WaitForSeconds(0.45f);
        yield return _waitForSeconds1;
        // abrir y cerrar puertas
        yield return ShowPopups(popups, "Cuando una puerta de algún mueble tenga un borde blanco,\nse podrá abrirla tocando sobre ella."); // #24
        yield return _waitForSeconds1;
        oppositeDoor1.ClickBehaviour(oppositeDoor1.gameObject);
        yield return _waitForSeconds1;
        oppositeDoor2.ClickBehaviour(oppositeDoor2.gameObject);
        yield return _waitForSeconds1;
        oppositeDoor3.ClickBehaviour(oppositeDoor3.gameObject);
        yield return _waitForSeconds2;
        oppositeDoor1.ClickBehaviour(oppositeDoor1.gameObject);
        yield return _waitForSeconds1;
        oppositeDoor2.ClickBehaviour(oppositeDoor2.gameObject);
        yield return _waitForSeconds1;
        oppositeDoor3.ClickBehaviour(oppositeDoor3.gameObject);
        yield return new WaitForSeconds(2.5f);

        // mirar objeto de mueble
        GetItem(ITEMID_OPPITEM);
        yield return _waitForSeconds1;
        rotationY = -0.75f;
        yield return _waitForSeconds1;
        rotationY = 0f;
        yield return _waitForSeconds1;
        rotationY = 0.75f;
        yield return _waitForSeconds1;
        rotationY = 0f;
        yield return _waitForSeconds1;
        ReturnItem();
        yield return _waitForSeconds2;

        // ir hacia estanteria
        Vector3 oppCorner = new(player.transform.position.x, player.transform.position.y, -88.1f); // -88 -> -88.3 -> -88.1
        yield return MovePlayerToTargetHorizontal(oppCorner, 1f);
        // MovePlayerHorizontal(false);
        // yield return new WaitForSeconds(0.38f); // probar 0.4 -> 0.38
        // StopPlayerMovement();
        // mirar objeto de estanteria
        
        yield return _waitForSeconds1;
        GetItem(ITEMID_SHELFITEM);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(75, 0, 1f);
        ReturnItem();
        yield return _waitForSeconds1;

        // subir la mirada
        yield return CameraMovementY(0f);
        yield return _waitForSecondsWall; // new WaitForSeconds(LOOKDOWN_TIME);
    }



    private IEnumerator LookAtCenterNew(PopUpManager popups)
    {
        const int ITEMID_TABLEITEM = 6;
        // GIRAR 50 GRADOS
        yield return CameraMovementX(75);
        yield return _waitForSeconds1;
        // MOVERSE HASTA Vector3(-53.3479996,1.42966449,-87.0749969)
        
        float tablePos = -53f; // -53.89 -> -53.59 -> -53.69 -> -53.83
        yield return MovePlayerToTargetX(tablePos);
        yield return _waitForSecondsWall;
        // GIRAR 90 GRADOS, BAJAR MIRADA 60
        yield return CameraMovement(90, 40);
        yield return _waitForSeconds1;
        // manipular item
        GetItem(ITEMID_TABLEITEM);
        yield return _waitForSeconds1_5;
        yield return RotateHeldItem(70, 0, 0.7f);
        yield return _waitForSeconds1;
        ReturnItem();
        yield return _waitForSeconds1;
        // subir mirada
        yield return CameraMovementY(0);
        yield return _waitForSecondsWall; // _waitForSeconds1;
    }

        private IEnumerator GoThroughTVNew(PopUpManager popups)
    {
        const int ITEMID_SHELFITEM = 4;
        const int ITEMID_SIDEITEM = 7;
        const int ITEMID_DOORITEM = 5;
        // Creo que sería bueno moverse hasta estar delante de la tv, y ahí mostrar como observar un objeto sobre la estanteria, y el objeto al lado de la tv.
        // Luego, se puede abrir las dos puertas que están debajo de la tv.

        // Girar hacia mueble de TV
        yield return CameraMovementX(180);
        // Moverse hacia la TV
        Vector3 tvSide = new(player.transform.position.x, player.transform.position.y, -87.9f); // -88.48 -> -87.9
        yield return MovePlayerToTarget(tvSide);
        yield return _waitForSeconds1;
        // elegir objeto estantería
        GetItem(ITEMID_SHELFITEM);
        yield return _waitForSeconds1;
        // rotar y llevarse item
        yield return RotateHeldItem(90, 0, 1f);
        yield return _waitForSeconds1;
        StoreItem();
        yield return _waitForSeconds1;
        // bajar mirada
        yield return CameraMovementY(30);
        yield return _waitForSeconds1;
        // abrir puerta derecha
        tvDoor1.ClickBehaviour(tvDoor1.gameObject);
        yield return new WaitUntil(() => tvDoor1.GetComponent<OpenDoor>().isMoving);
        yield return new WaitUntil(() => !tvDoor1.GetComponent<OpenDoor>().isMoving);
        // manipular objeto que hay dentro
        GetItem(ITEMID_DOORITEM);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(80, 0, 1f);
        yield return _waitForSeconds1;
        // elegir objeto
        StoreItem();
        yield return _waitForSeconds1;
        tvDoor1.ClickBehaviour(tvDoor1.gameObject);
        yield return new WaitUntil(() => tvDoor1.GetComponent<OpenDoor>().isMoving);
        yield return new WaitUntil(() => !tvDoor1.GetComponent<OpenDoor>().isMoving);
        // moverse al centro
        Vector3 tvCenter = new(-51.58f -0.19f, player.transform.position.y, player.transform.position.z); // -0.15 -> -0.17 -> -0.19
        yield return MovePlayerToTargetHorizontal(tvCenter);
        yield return _waitForSeconds1;
        // ver objeto al lado TV
        yield return CameraMovementX(150f);
        yield return _waitForSeconds1;
        // elegir objeto al lado TV
        GetItem(ITEMID_SIDEITEM);
        yield return RotateHeldItem(75, 0, 1f);
        yield return _waitForSeconds1;
        // devolver objeto al lado TV
        ReturnItem();
        yield return _waitForSeconds1;
        
        // abrir puerta izquierda
        tvDoor2.ClickBehaviour(tvDoor2.gameObject);
        yield return new WaitUntil(() => tvDoor2.GetComponent<OpenDoor>().isMoving);
        yield return new WaitUntil(() => !tvDoor2.GetComponent<OpenDoor>().isMoving);
        yield return _waitForSeconds1;
        tvDoor2.ClickBehaviour(tvDoor2.gameObject);
        yield return new WaitUntil(() => tvDoor2.GetComponent<OpenDoor>().isMoving);
        yield return new WaitUntil(() => !tvDoor2.GetComponent<OpenDoor>().isMoving);
        yield return _waitForSeconds1;

        // levantar y centrar mirada
        yield return CameraMovement(180f, 0f, 3f);
        yield return _waitForSeconds1;
    }

    private IEnumerator FinalLook(PopUpManager popups)
    {
        // mira a la izquierda
        //yield return CameraMovementX(90);
        //yield return _waitForSeconds1;
        //yield return CameraMovementX(0);
        //yield return _waitForSeconds1;
        //yield return CameraMovementX(-90);
        //yield return _waitForSeconds1;
        // mira a la derecha, a la pared de entrada
        yield return CameraMovementX(90);
        yield return _waitForSeconds1;
        // yield return CameraMovementX(0f);
        // yield return _waitForSeconds1_5;
        yield return ShowPopups(popups, "Cuando el tiempo límite termine,\nla luz de la sala se apagará,\ny el recorrido se dará por finalizado."); // #25
        yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_SearchOver);
        DisableItemInteractions(items);
        yield return ShowPopups(popups, "También es posible finalizar el recorrido\nantes de que termine el tiempo límite.\nPara hacer esto, dirigirse hacia la entrada de la sala."); // #26
        yield return _waitForSeconds1_5;
    }
    
    public IEnumerator TutorialSequence()
    {
        Interactable.allowAllInteractions = false;
        PopUpManager popups = PopUpManager.instance;
        DisablePlayerInput();
        Debug.Log("Caminando hacia la casa...");
        yield return WalkToHouse(popups);
        Debug.Log("Entrando en la casa...");
        yield return EnterHouse(popups);
        Debug.Log("Mostrando movimiento del jugador...");
        yield return PlayerMovementDemonstration(popups);
        Debug.Log("Mostrando movimiento de la cámara...");
        yield return CameraDemonstration(popups);
        Debug.Log("Mostrando rotación de objetos...");
        yield return ItemRotationDemonstration(popups);
        Debug.Log("Pasando por el pasillo...");
        yield return GoThroughHallway(popups);
        Debug.Log("Entrando al living...");
        yield return EnterLiving(popups);
        Debug.Log("Mostrando interacción con objetos...");
        yield return ItemInteractionDemonstration(popups);
        //yield return LookFromCorner(popups);
        Debug.Log("Yendo hacia la pared opuesta...");
        yield return GoToOppositeCorner(popups);
        Debug.Log("Mostrando interacción con cajones...");
        yield return DrawersDemonstration(popups);
        Debug.Log("Yendo por el mueble opuesto...");
        yield return GoThroughOpposite(popups);
        Debug.Log("Yendo a la mesa del centro...");
        yield return LookAtCenterNew(popups);
        Debug.Log("Yendo al mueble de la TV...");
        yield return GoThroughTVNew(popups);
        yield return FinalLook(popups);
        SceneLoader.LoadScene("MainMenu");
        yield return null;
    }
    public void OnDestroy()
    {
        instance = null;
    }
}
