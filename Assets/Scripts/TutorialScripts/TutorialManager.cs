using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
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
    public GameObject roomIndicators; // arrows showing the way to the search room.
    public GameObject tutorialElements;
    public RectTransform handle;
    public List<HeldItem> items = new();
    public static float rotationX = 0.0f;
    public static float rotationY = 0.0f;
    const int DEFAULT_SPEED = 4;
    const int SLOW_SPEED = 3;
    const float LOOKDOWN_TIME = 1.4f;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        Debug.Log("La fase en este momento es : " + GameStatus.currentPhase);
        // missionText = mission.GetComponentInChildren<TextMeshProUGUI>();
        // In the tutorial, certain objects must spawn, and user control is disabled
        bool inTutorial = GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Start;
        tutorialElements.SetActive(inTutorial);
        PlayerMovement.allowPlayerMovement = !inTutorial;
        TouchController.allowCameraMovement = !inTutorial;
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
        player.GetComponent<PlayerMovement>().verticalInput = isForwardMovement ? 1f : -1f;
        handle.transform.localPosition = new Vector3(0, isForwardMovement ? 100 : -100, 0);
    }

    private IEnumerator MovePlayerToTarget(Vector3 targetPosition, float stopDistance = 0.05f)
{
    MovePlayerVertical();
    yield return new WaitUntil(() => 
        Vector3.Distance(player.transform.position, targetPosition) <= stopDistance);

    StopPlayerMovement();
}

    private void MovePlayerHorizontal(bool isRightMovement = true)
    {
        player.GetComponent<PlayerMovement>().horizontalInput = isRightMovement ? 1f : -1f;
        handle.transform.localPosition = new Vector3(isRightMovement ? 100 : -100, 0, 0);
    }

    private void StopPlayerMovement()
    {
        player.GetComponent<PlayerMovement>().verticalInput = 0;
        player.GetComponent<PlayerMovement>().horizontalInput = 0;
        handle.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void CameraMovement(float xAxis, float yAxis = 0)
    {
        player.GetComponentInChildren<CameraControl>().LockAxis.x = xAxis;
        player.GetComponentInChildren<CameraControl>().LockAxis.y = yAxis;
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

        player.GetComponent<PlayerMovement>().moveSpeed = SLOW_SPEED;
        yield return new WaitForSeconds(1f);
        MovePlayerVertical();
        yield return new WaitUntil(() => houseDoor.GetComponent<Interactable>().isInteractable);
        GameStatus.currentPhase = GameStatus.GamePhase.Tutorial_ReachApple;
        StopPlayerMovement();
        player.GetComponent<PlayerMovement>().moveSpeed = DEFAULT_SPEED;
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Explain how to open doors, and enters inside the house.
    /// </summary>
    private IEnumerator EnterHouse(PopUpManager popups)
    {
        yield return ShowPopups(popups, "Cuando la puerta tenga un borde blanco,\nse debe abrirla tocando sobre ella."); // #4
        yield return new WaitForSeconds(1f);
        houseDoor.ClickBehaviour(houseDoor.gameObject);
        yield return new WaitForSeconds(1.5f);
        yield return ShowPopups(popups, new string[] { // #5-#6
            "Cuando se atraviese la puerta de la casa,\nse encenderá la luz del hall.\nEn ese momento, se verán objetos en los estantes del hall,\ny se tendrá un tiempo límite para observarlos.",
            "Para que se encienda la luz del hall,\nse debe avanzar hacia los objetos en los estantes."
        });
        MovePlayerVertical();
        yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Memorizing);
        StopPlayerMovement();
    }
    
    /// <summary>
    /// Starts memorizing phase, and demonstrates player movement.
    /// </summary>
    private IEnumerator PlayerMovementDemonstration(PopUpManager popups)
    {
        const float MOVE_TIME = 0.75f;
        yield return ShowPopups(popups, new string[] {  // #7-#8
            "Al encenderse la luz del hall,\ncomenzará a correr el tiempo límite\npara observar los objetos,\nen el reloj que se muestra en la pantalla.",
            "Es posible moverse en todas las direcciones del hall\n(adelante, atrás, izquierda y derecha),\ndeslizando el control izquierdo hacia la dirección deseada."
        });
        yield return new WaitForSeconds(0.5f);
        player.GetComponent<PlayerMovement>().moveSpeed = SLOW_SPEED;
        MovePlayerHorizontal(true);
        yield return new WaitForSeconds(0.4f);
        StopPlayerMovement();
        yield return new WaitForSeconds(MOVE_TIME);
        MovePlayerHorizontal(false);
        yield return new WaitForSeconds(0.4f);
        StopPlayerMovement();
        yield return new WaitForSeconds(MOVE_TIME);
        MovePlayerVertical(false);
        yield return new WaitForSeconds(0.4f);
        StopPlayerMovement();
        yield return new WaitForSeconds(MOVE_TIME);
        MovePlayerVertical(true);
        yield return new WaitForSeconds(0.4f);
        StopPlayerMovement();
        yield return new WaitForSeconds(MOVE_TIME);
        player.GetComponent<PlayerMovement>().moveSpeed = DEFAULT_SPEED;
    }
    
    /// <summary>
    /// Shows how to move the camera in every direction.
    /// </summary>
    private IEnumerator CameraDemonstration(PopUpManager popups)
    {
        const float WAIT_SPEED = 0.5f;
        yield return ShowPopups(popups, "Es posible mirar hacia todas las direcciones del hall\n(arriba, abajo, izquierda y derecha)\ndeslizando sobre la pantalla en la dirección deseada.");  // #9
        CameraMovement(5f); // mirada hacia la derecha
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f);
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(-5f);  // vuelta
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f);  // vuelta
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f, -5f); // mirada hacia abajo
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f, 0f);
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f, 5f); // vuelta
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f, 0f);
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(-5f); // mirada hacia la izquierda
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f);
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(5f);  // vuelta
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f);  // vuelta
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f, 5f); // mirada hacia arriba
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f, 0f);
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f, -5f); // vuelta
        yield return new WaitForSeconds(WAIT_SPEED);
        CameraMovement(0f, 0f);
        yield return new WaitForSeconds(WAIT_SPEED);
    }

    /// <summary>
    /// Shows how to rotate and leave an item.
    /// </summary>
    private IEnumerator ItemRotationDemonstration(PopUpManager popups)
    {
        const float WAIT_SPEED = 1f;
        const int ITEMID_CAMERA = 0;
        yield return ShowPopups(popups, new string[] { // #10-#11
            "Se debe observar los objetos en el hall,\ny tratar de memorizarlos,\nantes de que se acabe el tiempo límite.",
            "Cuando un objeto tenga un borde blanco,\nes posible observarlo con más detalle,\ntocando sobre el objeto."
        });
        yield return new WaitForSeconds(1f);
        GetItem(ITEMID_CAMERA);
        yield return new WaitForSeconds(1f);
        yield return ShowPopups(popups, "Es posible girar un objeto en distintas direcciones\n(izquierda, derecha, arriba y abajo),\ndeslizando sobre el objeto en la dirección deseada."); // #12
        yield return new WaitForSeconds(WAIT_SPEED);
        rotationX = -0.75f;
        yield return new WaitForSeconds(WAIT_SPEED);
        rotationX = 0f;
        yield return new WaitForSeconds(WAIT_SPEED);
        rotationX = 0.75f;
        yield return new WaitForSeconds(WAIT_SPEED);
        rotationX = 0;
        yield return new WaitForSeconds(WAIT_SPEED);
        rotationY = -0.75f;
        yield return new WaitForSeconds(WAIT_SPEED);
        rotationY = 0f;
        yield return new WaitForSeconds(WAIT_SPEED);
        rotationY = 0.75f;
        yield return new WaitForSeconds(WAIT_SPEED);
        rotationY = 0f;
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
        yield return new WaitForSeconds(0.5f);
        yield return ShowPopups(popups, "Cuando el tiempo límite termine,\nla luz del hall se apagará,\ny se deberá avanzar hacia el pasillo de la casa."); // #14
        yield return new WaitForSeconds(1f);
        CameraMovement(5f);
        yield return new WaitForSeconds(1f);
        CameraMovement(0f);
        yield return new WaitForSeconds(1f);
        yield return MovePlayerToTarget(new(-48.0513573f,1.49810004f,-102.521416f));
        //MovePlayerVertical();
        //yield return new WaitForSeconds(0.7f);
        //StopPlayerMovement();
        CameraMovement(-5f);
        yield return new WaitForSeconds(1f);
        CameraMovement(0f);
        yield return ShowPopups(popups, "Se debe avanzar por el pasillo de la casa\nhacia la entrada de la sala de living."); // #15
        player.GetComponent<PlayerMovement>().moveSpeed = SLOW_SPEED;
        yield return new WaitForSeconds(0.6f);
        yield return MovePlayerToTarget(new(-47.9446869f,1.49810004f,-91.1999969f));
        //MovePlayerVertical();
        //yield return new WaitForSeconds(2f);
        //StopPlayerMovement();
        player.GetComponent<PlayerMovement>().moveSpeed = DEFAULT_SPEED;
        yield return new WaitForSeconds(2f);
    }

    /// <summary>
    /// Goes into the living room.
    /// </summary>
    private IEnumerator EnterLiving(PopUpManager popups)
    {
        yield return ShowPopups(popups, "Cuando se atraviese la entrada de la sala de living,\nla luz de la sala se encenderá.\nA partir de ese momento, se deberá recorrer la sala,\nobservar los objetos distribuídos en ella,\ny seleccionar a los objetos que estaban en el hall."); // #16
        yield return new WaitForSeconds(1f);
        MovePlayerVertical();
        yield return new WaitForSeconds(0.3f);
        StopPlayerMovement();
        yield return ShowPopups(popups, "Al encenderse la luz de la sala,\ncomenzará a correr el tiempo límite\npara recorrer la sala y seleccionar los objetos,\nen el reloj que se muestra en la pantalla."); // #17
        player.GetComponent<PlayerMovement>().moveSpeed = SLOW_SPEED;
        MovePlayerHorizontal(false);
        yield return new WaitForSeconds(0.2f);
        StopPlayerMovement();
    }

    /// <summary>
    /// Looks at the entrance furniture, and manipulates the item above it.
    /// </summary>
    private IEnumerator ItemInteractionDemonstration(PopUpManager popups)
    {
        const int ITEMID_ENTRANCEITEM = 2;
        yield return new WaitForSeconds(1f);
        MovePlayerVertical();
        yield return new WaitForSeconds(0.5f);
        StopPlayerMovement();
        CameraMovement(5f, -2.5f);
        yield return new WaitForSeconds(1.2f);
        CameraMovement(0f);
        yield return ShowPopups(popups, "Cuando un objeto tenga un borde blanco,\nes posible observarlo con más detalle,\ntocando sobre el objeto."); // #18
        yield return new WaitForSeconds(1f);
        GetItem(ITEMID_ENTRANCEITEM);
        yield return new WaitForSeconds(1f);
        yield return ShowPopups(popups, "Es posible girar un objeto que está siendo observado,\nen distintas direcciones (izquierda, derecha, arriba y abajo),\ndeslizando sobre el objeto en la dirección deseada."); // #19
        yield return new WaitForSeconds(1f);
        rotationX = -0.75f;
        yield return new WaitForSeconds(1f);
        rotationX = 0f;
        yield return new WaitForSeconds(0.5f);
        yield return ShowPopups(popups, "Para que el objeto que está siendo observado vuelva a su lugar,\nse debe tocar el botón rojo que se muestra en la pantalla."); // #20
        yield return new WaitForSeconds(1.5f);
        ReturnItem();
        yield return new WaitForSeconds(2f);
        GetItem(ITEMID_ENTRANCEITEM);
        yield return new WaitForSeconds(1f);
        yield return ShowPopups(popups, "Para seleccionar el objeto que está siendo observado,\nse debe tocar nuevamente sobre el objeto."); // #21
        yield return new WaitForSeconds(1f);
        StoreItem();
        yield return ShowPopups(popups, "Los objetos seleccionados se muestran\nen la parte superior derecha de la pantalla."); // #22
        yield return new WaitForSeconds(1f);
        CameraMovement(-5f, 2.5f); // volver a mirar al frente
        yield return new WaitForSeconds(1.2f);
        CameraMovement(0f);
    }

    /// <summary>
    /// Goes to the corner with the rack, and looks at the entire room from there.
    /// </summary>
    private IEnumerator LookFromCorner(PopUpManager popups)
    {
        player.GetComponent<PlayerMovement>().moveSpeed = SLOW_SPEED;
        MovePlayerVertical();
        yield return new WaitForSeconds(0.45f); // probar 0.4 -> 0.45
        StopPlayerMovement();
        // Mostrar vista de la sala
        CameraMovement(-5f);
        yield return new WaitForSeconds(2.4f);
        CameraMovement(0f);
        yield return new WaitForSeconds(1.5f);
        CameraMovement(5f);
        yield return new WaitForSeconds(1.2f);
        CameraMovement(0f);
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// Moves from the rack corner to the drawers in the opposite corner.
    /// </summary>
    private IEnumerator GoToOppositeCorner(PopUpManager popups){
        MovePlayerVertical();
        yield return new WaitForSeconds(0.8f); // por ahi un poco mas
        MovePlayerHorizontal(false);
        yield return new WaitForSeconds(0.2f);
    }

    /// <summary>
    /// Opens the first drawer, and manipulates the item within it.
    /// </summary>
    private IEnumerator DrawersDemonstration(PopUpManager popups){
        const int ITEMID_DRAWERITEM = 3;
        StopPlayerMovement();
        CameraMovement(0f, -2f);
        yield return new WaitForSeconds(LOOKDOWN_TIME);
        CameraMovement(0f);
        yield return ShowPopups(popups, "Cuando un cajón de algún mueble tenga un borde blanco,\nse podrá abrirlo tocando sobre él."); // #23
        yield return new WaitForSeconds(1f);
        houseDrawer.ClickBehaviour(houseDrawer.gameObject);
        yield return new WaitForSeconds(2f);
        GetItem(ITEMID_DRAWERITEM);
        yield return new WaitForSeconds(1f);
        ReturnItem();
        yield return new WaitForSeconds(1f);
        houseDrawer.ClickBehaviour(houseDrawer.gameObject);
        
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator GoThroughOpposite(PopUpManager popups){
        const int ITEMID_OPPITEM = 8;
        const int ITEMID_SHELFITEM = 9;
        
        // ir al centro
        MovePlayerHorizontal(false);
        yield return new WaitForSeconds(0.45f);
        StopPlayerMovement();
        yield return new WaitForSeconds(0.5f);
        // abrir y cerrar puertas
        yield return ShowPopups(popups, "Cuando una puerta de algún mueble tenga un borde blanco,\nse podrá abrirla tocando sobre ella."); // #24
        yield return new WaitForSeconds(1f);
        oppositeDoor1.ClickBehaviour(oppositeDoor1.gameObject);
        yield return new WaitForSeconds(1f);
        oppositeDoor2.ClickBehaviour(oppositeDoor2.gameObject);
        yield return new WaitForSeconds(1f);
        oppositeDoor3.ClickBehaviour(oppositeDoor3.gameObject);
        yield return new WaitForSeconds(1f);
        oppositeDoor1.ClickBehaviour(oppositeDoor1.gameObject);
        yield return new WaitForSeconds(1f);
        oppositeDoor2.ClickBehaviour(oppositeDoor2.gameObject);
        yield return new WaitForSeconds(1f);
        oppositeDoor3.ClickBehaviour(oppositeDoor3.gameObject);
        yield return new WaitForSeconds(1f);
        
        // mirar objeto de mueble
        GetItem(ITEMID_OPPITEM);
        yield return new WaitForSeconds(1f);
        rotationX = -0.75f;
        yield return new WaitForSeconds(1f);
        rotationX = 0f;
        yield return new WaitForSeconds(1.1f);
        ReturnItem();
        yield return new WaitForSeconds(1f);
        
        // ir hacia estanteria
        MovePlayerHorizontal(false);
        yield return new WaitForSeconds(0.38f); // probar 0.4 -> 0.38

        // mirar objeto de estanteria
        StopPlayerMovement();
        yield return new WaitForSeconds(1f);
        GetItem(ITEMID_SHELFITEM);
        yield return new WaitForSeconds(1f);
        rotationX = -0.75f;
        yield return new WaitForSeconds(1f);
        rotationX = 0f;
        yield return new WaitForSeconds(1.1f);
        ReturnItem();
        yield return new WaitForSeconds(1f);
        
        // subir la mirada
        CameraMovement(0f, 2f);
        yield return new WaitForSeconds(LOOKDOWN_TIME);
        CameraMovement(0f);
    }

    private IEnumerator GoThroughTV(PopUpManager popups){
        const int ITEMID_SHELFITEM = 4;
        const int ITEMID_SIDEITEM = 7;
        const int ITEMID_DOORITEM = 5;
        const float SIDELOOK_TIME = 0.3f;
        // Creo que sería bueno moverse hasta estar delante de la tv, y ahí mostrar como observar un objeto sobre la estanteria, y el objeto al lado de la tv.
        // Luego, se puede abrir las dos puertas que están debajo de la tv.

        // Girar hacia mueble de TV
        CameraMovement(-5f);
        yield return new WaitForSeconds(2.4f);
        CameraMovement(0f, 0f);
        yield return new WaitForSeconds(1f);
        // Moverse al centro de la TV
        MovePlayerVertical();
        yield return new WaitForSeconds(0.55f); // probar 0.5 -> 0.55
        StopPlayerMovement();
        // Mirar la TV
        CameraMovement(5f);
        yield return new WaitForSeconds(1.3f);
        CameraMovement(0f, 0f);
        yield return new WaitForSeconds(0.5f);
        // Mirar hacia la esquina, tomar item
        CameraMovement(5f);
        yield return new WaitForSeconds(SIDELOOK_TIME);
        CameraMovement(0f);
        GetItem(ITEMID_SHELFITEM);
        yield return new WaitForSeconds(1f);
        // rotar y llevarse item
        rotationX = -0.75f;
        yield return new WaitForSeconds(1f);
        rotationX = 0f;
        yield return new WaitForSeconds(0.5f);
        StoreItem();
        yield return new WaitForSeconds(1.2f);
        // devolver mirada al centro
        CameraMovement(-5f);
        yield return new WaitForSeconds(SIDELOOK_TIME);
        CameraMovement(0f, 0f);
        yield return new WaitForSeconds(0.5f);
        // bajar mirada, manipular objeto a la izquierda
        CameraMovement(-5f);
        yield return new WaitForSeconds(SIDELOOK_TIME);
        CameraMovement(0f, -2f);
        yield return new WaitForSeconds(LOOKDOWN_TIME);
        CameraMovement(0f,0f);
        yield return new WaitForSeconds(1f);
        GetItem(ITEMID_SIDEITEM);
        yield return new WaitForSeconds(1f);
        rotationY = 0.75f;
        yield return new WaitForSeconds(1f);
        rotationY = 0f;
        yield return new WaitForSeconds(1f);
        ReturnItem();
        yield return new WaitForSeconds(1f);
        // abrir puerta izquierda
        tvDoor2.ClickBehaviour(tvDoor2.gameObject);
        yield return new WaitForSeconds(2f);
        tvDoor2.ClickBehaviour(tvDoor2.gameObject);
        yield return new WaitForSeconds(2f);
        // mirar a la derecha, abrir puerta derecha y manipular su objeto
        CameraMovement(5f);
        yield return new WaitForSeconds(SIDELOOK_TIME *2);
        CameraMovement(0f);
        yield return new WaitForSeconds(1f);
        tvDoor1.ClickBehaviour(tvDoor1.gameObject);
        yield return new WaitForSeconds(2f);
        GetItem(ITEMID_DOORITEM);
        yield return new WaitForSeconds(1f);
        rotationX = 0.75f;
        yield return new WaitForSeconds(1f);
        rotationX = 0f;
        yield return new WaitForSeconds(1f);
        StoreItem();
        yield return new WaitForSeconds(1f);
        tvDoor1.ClickBehaviour(tvDoor1.gameObject);
        yield return new WaitForSeconds(1f);
        // levantar y centrar mirada
        CameraMovement(0f, 2f);
        yield return new WaitForSeconds(LOOKDOWN_TIME);
        CameraMovement(-5f);
        yield return new WaitForSeconds(SIDELOOK_TIME);
        CameraMovement(0f,0f);
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator LookAtCenter(PopUpManager popups){
        const int ITEMID_TABLEITEM = 6;
        // girar camara al centro
        CameraMovement(-5f);
        yield return new WaitForSeconds(2f);
        // bajar mirada
        CameraMovement(0f, -2f);
        yield return new WaitForSeconds(LOOKDOWN_TIME);
        CameraMovement(0f);
        yield return new WaitForSeconds(1f);
        // manipular item
        GetItem(ITEMID_TABLEITEM);
        yield return new WaitForSeconds(1.2f);
        ReturnItem();
        yield return new WaitForSeconds(1f);
        // subir mirada
        CameraMovement(0f, 2f);
        yield return new WaitForSeconds(LOOKDOWN_TIME);
        CameraMovement(0f);
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator FinalLook(PopUpManager popups){
        // mira a la izquierda
        CameraMovement(-5f);
        yield return new WaitForSeconds(1.5f);
        CameraMovement(0f);
        yield return new WaitForSeconds(1f);
        // mira a la derecha, a la pared de entrada
        CameraMovement(5f);
        yield return new WaitForSeconds(2f);
        CameraMovement(0f);
        yield return new WaitForSeconds(1.5f);
        yield return ShowPopups(popups, "Cuando el tiempo límite termine,\nla luz de la sala se apagará,\ny el recorrido se dará por finalizado."); // #25
        yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_SearchOver);
        DisableItemInteractions(items);
        yield return ShowPopups(popups, "También es posible finalizar el recorrido\nantes de que termine el tiempo límite.\nPara hacer esto, dirigirse hacia la entrada de la sala."); // #26
        yield return new WaitForSeconds(2f);
    }
    public IEnumerator TutorialSequence()
    {
        Interactable.allowAllInteractions = false;
        PopUpManager popups = PopUpManager.instance;
        DisablePlayerInput();
        yield return WalkToHouse(popups);
        yield return EnterHouse(popups);
        yield return PlayerMovementDemonstration(popups);
        yield return CameraDemonstration(popups);
        yield return ItemRotationDemonstration(popups);
        yield return GoThroughHallway(popups);
        yield return EnterLiving(popups);
        yield return ItemInteractionDemonstration(popups);
        yield return LookFromCorner(popups);
        yield return GoToOppositeCorner(popups);
        yield return DrawersDemonstration(popups);
        yield return GoThroughOpposite(popups);
        yield return GoThroughTV(popups);
        yield return LookAtCenter(popups);
        yield return FinalLook(popups);
        SceneLoader.LoadScene("MainMenu");
        yield return null;
    }

}
