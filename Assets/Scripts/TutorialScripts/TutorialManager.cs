using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class TutorialManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_1 = new(0.1f);
    private static WaitForSeconds _waitForSeconds0_5 = new(0.5f);
    private static WaitForSeconds _waitForSeconds0_7 = new(0.7f);
    private static WaitForSeconds _waitForSeconds1_4 = new(1.4f);
    private static WaitForSeconds _waitForSeconds1_5 = new(1.5f);
    private static WaitForSeconds _waitForSeconds1 = new(1f);
    private static WaitForSeconds _waitForSeconds2 = new(2f);
    private static WaitForSeconds _waitForSeconds2_5 = new(2.5f);
    private static WaitForSeconds _waitForSeconds3 = new(3f);
    private static WaitForSeconds _waitForSecondsWall = new(0.2f);
    public static TutorialManager instance;

    // Interactables used during the tutorial
    public OpenDoor houseDoor;
    public OpenDoor tvDoor1;
    public OpenDoor tvDoor2;
    public OpenDoor oppositeDoor1;
    public OpenDoor oppositeDoor2;
    public OpenDoor oppositeDoor3;
    public OpenDrawer houseDrawer;

    public GameObject player; // Reference to the player
    private CameraControl cam; // Reference to the camera, used to move it.
    public GameObject tutorialElements;
    public RectTransform handle; // Joystick handle manipulated during movements.
    public List<HeldItem> items = new();
    public static float rotationX = 0.0f;
    public static float rotationY = 0.0f;
    const int DEFAULT_SPEED = 2; // velocidad default
    const float SLOW_SPEED = 1.5f; // velocidad más lenta

    // Stop points used during the tutorial.
    private readonly List<Vector3> stopPoints = new()
    {
        new Vector3(-53.3699989f,1.52156401f,-120), // inicio
        new Vector3(-53.3699989f,1.52156401f,-108.668961f), // frente a puerta
        new Vector3(-53.3699989f,1.52156401f,-103.750031f), // en el hall
        new Vector3(-52.2084694f,1.52156401f,-103.750038f), // a la derecha
        new Vector3(-53.4415207f,1.52156401f,-103.750038f), // a la izquierda
        new Vector3(-53.4415283f,1.52156413f,-104.911568f), // para atras
        new Vector3(-53.4415283f,1.52156401f,-103.678513f), // para adelante
        new Vector3(-48.1700287f,1.52156448f,-103.821564f),
        new Vector3(-48.1700287f,1.52156472f,-91.45504f),
        new Vector3(-48.1700287f,1.52156472f,-89.9488831f),
        new Vector3(-48.3459892f,1.5215652f,-86.5954437f),
        new Vector3(-48.3459892f,1.5215652f,-84.0571442f),
        new Vector3(-54.6793365f,1.5215652f,-84.0571442f),
        new Vector3(-54.6793365f,1.5215652f,-86.3756256f),
        new Vector3(-54.6793365f,1.5215652f,-87.5372086f),
        new Vector3(-53.0574074f,1.52156532f,-87.0954361f),
        new Vector3(-53.0574112f,1.52156556f,-88.1166611f),
        new Vector3(-51.4666519f,1.52156544f,-88.1166611f)
    };

    // Start is called before the first frame update
    void Start()
    {
        // In the tutorial, certain objects must spawn, and user control is disabled
        bool inTutorial = GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Start;
        bool startingDialog = inTutorial || (GameStatus.currentPhase == GameStatus.GamePhase.Waiting && Settings.currentDifficulty == Settings.Difficulty.Preevaluación);
        if (!inTutorial)
        {
            Destroy(tutorialElements);
        }
        PlayerMovement.AllowMovement(!startingDialog);
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

    private void MovePlayerVertical(bool isForwardMovement = true)
    {
        player.GetComponent<PlayerMovement>().verticalInput = isForwardMovement ? 1f : -1f;
        handle.transform.localPosition = new Vector3(0, isForwardMovement ? 100 : -100, 0);
    }

    private IEnumerator MoveToTargetOverTime(Vector3 end, int tiltUp = 1, int tiltRight = 0, float speed = 3.25f)
    {
        Vector3 start = player.transform.position;
        float distance = Vector3.Distance(start, end);
        float progress = 0f;
        handle.transform.localPosition = new Vector3(tiltRight * 100, tiltUp * 100, 0);
        while (progress < 1f)
        {
            // Move progress depending on speed (normalized)
            progress += (speed / distance) * Time.deltaTime;

            // Ease in & out
            const float EASE_STRENGTH = 0.65f;
            float smooth = Mathf.SmoothStep(0f, 1f, progress);
            float eased = Mathf.Lerp(progress, smooth, EASE_STRENGTH);

            player.transform.position = Vector3.Lerp(start, end, eased);
            if (progress > 0.9f)
            {
                handle.transform.localPosition = new Vector3(0, 0, 0);
            }
            yield return null;
        }

        // Snap to final position
        player.transform.position = end;
    }

    private IEnumerator RotateHeldItem(float x, float y, float duration, float z = 0)
    {
        yield return StartCoroutine(HeldItem.currentlyHeldItem.RotateItemOverTime(x, y, z, duration));
    }

    private void StopPlayerMovement()
    {
        player.GetComponent<PlayerMovement>().verticalInput = 0;
        player.GetComponent<PlayerMovement>().horizontalInput = 0;
        handle.transform.localPosition = new Vector3(0, 0, 0);
    }

    private void GetItem(int index)
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

    private void DisableItemInteractions(List<HeldItem> items)
    {
        foreach (HeldItem hi in items)
        {
            hi.gameObject.GetComponent<Outline>().enabled = false;
            hi.gameObject.GetComponent<Interactable>().isInteractable = false;
            hi.gameObject.GetComponent<Interactable>().stoppedInteraction = true;
        }
    }

    private IEnumerator ShowPopups(PopUpManager popups, string msg)
    {
        Time.timeScale = 0;
        yield return popups.ShowPopups(msg);
        Time.timeScale = 1;
    }

    private IEnumerator ShowPopups(PopUpManager popups, string[] msg)
    {
        Time.timeScale = 0;
        yield return popups.ShowPopups(msg);
        Time.timeScale = 1;
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
        yield return _waitForSeconds0_1;
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
        yield return MoveToTargetOverTime(stopPoints[2], 1, 0, 2);
        yield return _waitForSeconds0_1;
    }

    /// <summary>
    /// Starts memorizing phase, and demonstrates player movement.
    /// </summary>
    private IEnumerator PlayerMovementDemonstration(PopUpManager popups)
    {
        const float STOP_TIME = 0.75f;
        yield return ShowPopups(popups, new string[] {  // #7-#8
            "Al encenderse la luz del hall,\ncomenzará a correr el tiempo límite\npara observar los objetos,\nen el reloj que se muestra en la pantalla.",
            "Es posible moverse en todas las direcciones del hall\n(adelante, atrás, izquierda y derecha),\ndeslizando el control izquierdo hacia la dirección deseada."
        });

        yield return _waitForSeconds0_5;
        player.GetComponent<PlayerMovement>().forceStop = false;
        yield return MoveToTargetOverTime(stopPoints[3], 0, 1, 2);
        yield return new WaitForSeconds(STOP_TIME);
        yield return MoveToTargetOverTime(stopPoints[4], 0, -1, 2);
        yield return new WaitForSeconds(STOP_TIME);
        yield return MoveToTargetOverTime(stopPoints[5], -1, 0, 2);
        yield return new WaitForSeconds(STOP_TIME);
        yield return MoveToTargetOverTime(stopPoints[6], 1, 0, 2);
        yield return new WaitForSeconds(STOP_TIME);
    }

    /// <summary>
    /// Shows how to move the camera in every direction.
    /// </summary>
    private IEnumerator CameraDemonstration(PopUpManager popups)
    {
        yield return ShowPopups(popups, "Es posible mirar hacia todas las direcciones del hall\n(arriba, abajo, izquierda y derecha)\ndeslizando sobre la pantalla en la dirección deseada.");  // #9
        int[] spinDegrees = { 30, 0, -30, 0 };
        foreach (int i in spinDegrees)
        {
            yield return cam.CameraMovementX(i);
            yield return _waitForSeconds0_5;
        }
        foreach (int i in spinDegrees)
        {
            yield return cam.CameraMovementY(-i);
            yield return _waitForSeconds0_5;
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
        yield return _waitForSeconds0_7;
        int[] spinDegrees = { 90, -90, -90, 90 };
        foreach (int i in spinDegrees)
        {
            yield return RotateHeldItem(0, i, WAIT_SPEED);
            yield return _waitForSeconds0_7;
        }
        foreach (int i in spinDegrees)
        {
            yield return RotateHeldItem(-i, 0, WAIT_SPEED);
            yield return _waitForSeconds0_7;
        }
        yield return ShowPopups(popups, "Para que el objeto vuelva a su lugar en el estante,\nse debe tocar el botón rojo que se muestra en la pantalla.");  // #13
        yield return _waitForSeconds0_5;
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
        yield return _waitForSeconds0_5;
        yield return ShowPopups(popups, "Cuando el tiempo límite termine,\nla luz del hall se apagará,\ny se deberá avanzar hacia el pasillo de la casa."); // #14
        yield return cam.CameraMovementX(90);
        yield return _waitForSeconds1;
        yield return MoveToTargetOverTime(stopPoints[7]);
        yield return _waitForSecondsWall;
        player.transform.position = new Vector3(-48.249752f, player.transform.position.y, player.transform.position.z);
        yield return cam.CameraMovementX(0);
        yield return _waitForSeconds1;
        yield return ShowPopups(popups, "Se debe avanzar por el pasillo de la casa\nhacia la entrada de la sala de living."); // #15
        yield return _waitForSeconds1;
        yield return MoveToTargetOverTime(stopPoints[8], 1, 0);
        yield return _waitForSeconds1;
    }

    /// <summary>
    /// Goes into the living room.
    /// </summary>
    private IEnumerator EnterLiving(PopUpManager popups)
    {
        yield return ShowPopups(popups, "Cuando se atraviese la entrada de la sala de living,\nla luz de la sala se encenderá.\nA partir de ese momento, se deberá recorrer la sala,\nobservar los objetos distribuídos en ella,\ny seleccionar a los objetos que estaban en el hall."); // #16
        yield return MoveToTargetOverTime(stopPoints[9], 1, 0, 2);
        yield return ShowPopups(popups, "Al encenderse la luz de la sala,\ncomenzará a correr el tiempo límite\npara recorrer la sala y seleccionar los objetos,\nen el reloj que se muestra en la pantalla."); // #17
    }

    /// <summary>
    /// Looks at the entrance furniture, and manipulates the item above it.
    /// </summary>
    private IEnumerator ItemInteractionDemonstration(PopUpManager popups)
    {
        const int ITEMID_ENTRANCEITEM = 2;
        yield return _waitForSeconds1;
        yield return MoveToTargetOverTime(stopPoints[10], 1, 0, 2);
        yield return _waitForSeconds1;
        yield return cam.CameraMovement(90f, 30f);
        yield return ShowPopups(popups, "Cuando un objeto tenga un borde blanco,\nes posible observarlo con más detalle,\ntocando sobre el objeto."); // #18
        yield return _waitForSeconds1;
        GetItem(ITEMID_ENTRANCEITEM);
        yield return _waitForSeconds1;
        yield return ShowPopups(popups, "Es posible girar un objeto que está siendo observado,\nen distintas direcciones (izquierda, derecha, arriba y abajo),\ndeslizando sobre el objeto en la dirección deseada."); // #19
        yield return _waitForSeconds1;
        yield return RotateHeldItem(0, 120, 1f);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(0, -120, 1f);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(90, 0, 1f);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(-90, 0, 1f);
        yield return _waitForSeconds0_5;
        yield return ShowPopups(popups, "Para que el objeto que está siendo observado vuelva a su lugar,\nse debe tocar el botón rojo que se muestra en la pantalla."); // #20
        yield return _waitForSeconds1;
        ReturnItem();
        yield return _waitForSeconds3;
        GetItem(ITEMID_ENTRANCEITEM);
        yield return _waitForSeconds1;
        yield return ShowPopups(popups, "Para seleccionar el objeto que está siendo observado,\nse debe tocar nuevamente sobre el objeto."); // #21
        yield return _waitForSeconds2;
        StoreItem();
        yield return ShowPopups(popups, "Los objetos seleccionados se muestran\nen la parte superior derecha de la pantalla."); // #22
        yield return _waitForSeconds1;
        yield return cam.CameraMovement(0, 0); // volver a mirar al frente
        yield return _waitForSeconds1;
    }

    /// <summary>
    /// Moves from the rack corner to the drawers in the opposite corner.
    /// </summary>
    private IEnumerator GoToOppositeCorner(PopUpManager popups)
    {
        yield return MoveToTargetOverTime(stopPoints[11], 1, 0, 2);
        yield return _waitForSecondsWall;
        player.GetComponent<PlayerMovement>().moveSpeed = SLOW_SPEED;
        yield return cam.CameraMovementX(-90); // mirar a pared opuesta
        Vector3 oppPosition = new(-54.5f, player.transform.position.y, player.transform.position.z); // -54.5 -> -55 -> -54.8 -> -54.7 -> -54.5
        yield return _waitForSeconds1;
        //yield return MovePlayerToTarget(oppPosition);
        yield return MoveToTargetOverTime(stopPoints[12], 1, 0, 2);
        yield return _waitForSecondsWall;
        player.GetComponent<PlayerMovement>().moveSpeed = DEFAULT_SPEED;
    }

    /// <summary>
    /// Opens the first drawer, and manipulates the item within it.
    /// </summary>
    private IEnumerator DrawersDemonstration(PopUpManager popups)
    {
        const int ITEMID_DRAWERITEM = 3;
        //StopPlayerMovement();
        yield return cam.CameraMovementY(45);
        yield return _waitForSeconds1_4;
        yield return ShowPopups(popups, "Cuando un cajón de algún mueble tenga un borde blanco,\nse podrá abrirlo tocando sobre él."); // #23
        yield return _waitForSeconds1;
        houseDrawer.ClickBehaviour(houseDrawer.gameObject);
        yield return _waitForSeconds2;
        GetItem(ITEMID_DRAWERITEM);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(0, 0, 1f, 70);
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
        // ir al centro
        yield return MoveToTargetOverTime(stopPoints[13], 0, -1);
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
        yield return _waitForSeconds2_5;
        GetItem(ITEMID_OPPITEM);    // mirar objeto de mueble
        yield return _waitForSeconds1;
        yield return RotateHeldItem(-180, 0, 1.5f);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(180, 0, 1.5f);
        yield return _waitForSeconds1;
        ReturnItem();
        yield return _waitForSeconds2;
        yield return MoveToTargetOverTime(stopPoints[14], 0, -1, 2);    // ir hacia estanteria
        yield return _waitForSeconds1;
        GetItem(ITEMID_SHELFITEM);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(0, 0, 1f, 100);
        yield return _waitForSeconds1;
        ReturnItem();
        yield return _waitForSeconds1;
        yield return cam.CameraMovementY(0f);   // subir la mirada
        yield return _waitForSecondsWall;
    }



    private IEnumerator LookAtCenterNew(PopUpManager popups)
    {
        const int ITEMID_TABLEITEM = 6;
        // GIRAR
        yield return cam.CameraMovementX(75);
        yield return _waitForSeconds1;
        // MOVERSE HASTA MESA
        yield return MoveToTargetOverTime(stopPoints[15], 1, 0, 2);
        yield return _waitForSecondsWall;
        // GIRAR 90 GRADOS, BAJAR MIRADA 60
        yield return cam.CameraMovement(90, 40);
        yield return _waitForSeconds1;
        // manipular item
        GetItem(ITEMID_TABLEITEM);
        yield return _waitForSeconds1_5;
        yield return RotateHeldItem(30, 70, 0.7f);
        yield return _waitForSeconds1;
        ReturnItem();
        yield return _waitForSeconds1;
        // subir mirada
        yield return cam.CameraMovementY(0);
        yield return _waitForSecondsWall; // _waitForSeconds1;
    }

    private IEnumerator GoThroughTVNew(PopUpManager popups)
    {
        const int ITEMID_SHELFITEM = 4;
        const int ITEMID_SIDEITEM = 7;
        const int ITEMID_DOORITEM = 5;
        yield return cam.CameraMovementX(180);  // Girar hacia mueble de TV
        yield return MoveToTargetOverTime(stopPoints[16], 1, 0, 2); // Moverse hacia la TV
        yield return _waitForSeconds1;
        // elegir objeto estantería
        GetItem(ITEMID_SHELFITEM);
        yield return _waitForSeconds1;
        // rotar y llevarse item
        yield return RotateHeldItem(0, 90, 1f);
        yield return _waitForSeconds1;
        StoreItem();
        yield return _waitForSeconds1;
        // bajar mirada
        yield return cam.CameraMovementY(30);
        yield return _waitForSeconds1;
        // abrir puerta derecha
        tvDoor1.ClickBehaviour(tvDoor1.gameObject);
        yield return new WaitUntil(() => tvDoor1.GetComponent<OpenDoor>().isMoving);
        yield return new WaitUntil(() => !tvDoor1.GetComponent<OpenDoor>().isMoving);
        // manipular objeto que hay dentro
        GetItem(ITEMID_DOORITEM);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(0, 0, 1f, 80);
        yield return _waitForSeconds1;
        // elegir objeto
        StoreItem();
        yield return _waitForSeconds1;
        tvDoor1.ClickBehaviour(tvDoor1.gameObject);
        yield return new WaitUntil(() => tvDoor1.GetComponent<OpenDoor>().isMoving);
        yield return new WaitUntil(() => !tvDoor1.GetComponent<OpenDoor>().isMoving);
        // moverse al centro
        yield return MoveToTargetOverTime(stopPoints[17], 0, -1, 2);
        yield return _waitForSeconds1;
        // ver objeto al lado TV
        yield return cam.CameraMovementX(150f);
        yield return _waitForSeconds1;
        // elegir objeto al lado TV
        GetItem(ITEMID_SIDEITEM);
        yield return _waitForSeconds1;
        yield return RotateHeldItem(0, -75, 1f);
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
        yield return cam.CameraMovement(180f, 0f);
        yield return _waitForSeconds1;
    }

    private IEnumerator FinalLook(PopUpManager popups)
    {
        // mira a la derecha, a la pared de entrada
        yield return cam.CameraMovementX(90);
        yield return _waitForSeconds1;
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
        PlayerMovement.AllowMovement(false);
        yield return WalkToHouse(popups);
        yield return EnterHouse(popups);
        yield return PlayerMovementDemonstration(popups);
        yield return CameraDemonstration(popups);
        yield return ItemRotationDemonstration(popups);
        yield return GoThroughHallway(popups);
        yield return EnterLiving(popups);
        yield return ItemInteractionDemonstration(popups);
        yield return GoToOppositeCorner(popups);
        yield return DrawersDemonstration(popups);
        yield return GoThroughOpposite(popups);
        yield return LookAtCenterNew(popups);
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
