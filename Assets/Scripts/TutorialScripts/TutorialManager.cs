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
    private TextMeshProUGUI missionText;
    public OpenDoor houseDoor;
    public OpenDoor tvDoor;
    public GameObject player;
    public GameObject roomIndicators; // arrows showing the way to the search room.
    public GameObject tutorialElements;
    public RectTransform handle;
    public List<HeldItem> items = new();
    public static float rotationX = 0.0f;
    public static float rotationY = 0.0f;
    
    // Start is called before the first frame update
    void Start(){
        if (instance == null){
            instance = this;
        }
        Debug.Log("La fase en este momento es : " + GameStatus.currentPhase);
        missionText = mission.GetComponentInChildren<TextMeshProUGUI>();
        // In the tutorial, certain objects must spawn, and user control is disabled
        bool inTutorial = GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Start;
        tutorialElements.SetActive(inTutorial);
        PlayerMovement.allowPlayerMovement = !inTutorial;
        TouchController.allowCameraMovement = !inTutorial;
    }

    private void StartMission(){
        //missionText.text = text;
        //mission.SetActive(true);
        Time.timeScale = 1;
    }

    private void StopMission(){
        Time.timeScale = 0;
        mission.SetActive(false);
    }

    private void DisablePlayerInput(){
        TouchController.allowCameraMovement = false;
        PlayerMovement.allowPlayerMovement = false;
    }
    private void MovePlayerForward(){
        player.GetComponent<PlayerMovement>().verticalInput = 0.001f;
        handle.transform.localPosition = new Vector3(0,100,0);
    }
    private void StopPlayerMovement() {
        player.GetComponent<PlayerMovement>().verticalInput = 0;
        handle.transform.localPosition = new Vector3(0,0,0);
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

    public void ReturnItem()
    {
        HeldItem.ReturnItem();
        Interactable.allowAllInteractions = false;
    }

    public void DisableItemInteractions(){
        DisableInteractionOnItem(items[0]);
        DisableInteractionOnItem(items[1]);
    }

    private void DisableInteractionOnItem(HeldItem it){
        it.gameObject.GetComponent<Outline>().enabled = false;
        it.gameObject.GetComponent<Interactable>().isInteractable = false;
        it.gameObject.GetComponent<Interactable>().stoppedInteraction = true;
    }

    public IEnumerator TutorialSequence()
    {
        Interactable.allowAllInteractions = false;
        PopUpManager popups = PopUpManager.instance;
        DisablePlayerInput();
        StopMission();
        
        yield return popups.ShowPopups(new string[] {
            "Incliná el joystick hacia adelante para dirigirte hacia la puerta de la casa."
        });
        StartMission();
        yield return new WaitForSeconds(1f);
        MovePlayerForward();
        yield return new WaitUntil(() => houseDoor.GetComponent<Interactable>().isInteractable);
        GameStatus.currentPhase = GameStatus.GamePhase.Tutorial_ReachApple;  
        StopPlayerMovement();
        yield return new WaitForSeconds(0.5f);
        StopMission();

        yield return popups.ShowPopups(new string[] {
            "La puerta tiene un borde blanco. Eso significa que puedes interactuar con ella tocándola."
        });
        StartMission();
        yield return new WaitForSeconds(1f);
        houseDoor.ClickBehaviour(houseDoor.gameObject);
        yield return new WaitForSeconds(1.5f);
        MovePlayerForward();
        yield return new WaitForSeconds(0.75f);
        StopPlayerMovement();
        yield return new WaitForSeconds(1.5f);
        StopMission();
        yield return popups.ShowPopups("Observá los objetos con atención, y tratá de memorizarlos. Podés presionarlos para verlos más de cerca.");
        StartMission();
        yield return new WaitForSeconds(1f);
        GetItem(0);
        yield return new WaitForSeconds(1f);
        StopMission();
        yield return popups.ShowPopups("Deslizá en la pantalla para rotar el objeto.");
        StartMission();
        yield return new WaitForSeconds(1f);
        rotationY = -0.75f;
        yield return new WaitForSeconds(1f);
        rotationY = 0f;
        yield return new WaitForSeconds(0.5f);
        StopMission();
        yield return popups.ShowPopups("Presiona el botón rojo para dejar el objeto donde estaba.");
        StartMission();
        yield return new WaitForSeconds(0.5f);
        ReturnItem();
        yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_BeforeSearch);
        DisableItemInteractions();
        yield return new WaitForSeconds(0.5f);
        StopMission();
        yield return popups.ShowPopups("Puedes deslizar en la pantalla para mover la cámara.");
        StartMission();
        yield return new WaitForSeconds(1f);
        CameraMovement(5f);
        yield return new WaitForSeconds(1f);
        CameraMovement(0f);
        StopMission();
        yield return popups.ShowPopups("Dirigite hacia la derecha por el pasillo para continuar.");
        StartMission();
        yield return new WaitForSeconds(1f);
        MovePlayerForward();
        yield return new WaitForSeconds(0.7f);
        StopPlayerMovement();
        CameraMovement(-5f);
        yield return new WaitForSeconds(1f);
        CameraMovement(0f);
        MovePlayerForward();
        yield return new WaitForSeconds(1.4f);
        StopPlayerMovement();
        StopMission();
        yield return popups.ShowPopups("Observá los objetos del living con atención, y tratá de reconocer aquellos que estaban en el hall.");
        StartMission();
        MovePlayerForward();
        yield return new WaitForSeconds(0.2f);
        StopPlayerMovement();
        yield return new WaitForSeconds(1f);
        CameraMovement(-2.5f, -1.5f); // mirar a la mesita
        yield return new WaitForSeconds(1.5f);
        CameraMovement(0f);
        MovePlayerForward();
        yield return new WaitForSeconds(0.5f);
        StopPlayerMovement();
        yield return new WaitForSeconds(1f);
        GetItem(2);
        yield return new WaitForSeconds(1f);
        StopMission();
        yield return popups.ShowPopups("Mientras observás un objeto, puedes presionar el botón rojo para dejarlo.");
        StartMission();
        yield return new WaitForSeconds(0.5f);
        ReturnItem();
        yield return new WaitForSeconds(0.5f);
        CameraMovement(-2.5f);
        yield return new WaitForSeconds(3f);
        CameraMovement(0f);
        yield return new WaitForSeconds(0.5f);
        StopMission();
        yield return popups.ShowPopups("Las puertas y cajones pueden abrirse y cerrarse cuando estás cerca.");
        StartMission();
        yield return new WaitForSeconds(1f);
        tvDoor.ClickBehaviour(tvDoor.gameObject);
        yield return new WaitForSeconds(1f);
        GetItem(3);
        yield return new WaitForSeconds(1f);
        StopMission();
        yield return popups.ShowPopups("Mientras observás un objeto, puedes presionarlo de nuevo para elegirlo, si creés que estaba en el hall.");
        StartMission();
        yield return new WaitForSeconds(1f);
        rotationX = 0.75f;
        yield return new WaitForSeconds(1f);
        rotationX = 0f;
        yield return new WaitForSeconds(1f);
        HeldItem.StoreItem();
        Interactable.allowAllInteractions = false;
        yield return new WaitForSeconds(1f);
        yield return popups.ShowPopups("Los objetos recogidos se muestran en la pantalla, arriba a la derecha.");
        StartMission();
        yield return new WaitForSeconds(1f);
        tvDoor.ClickBehaviour(tvDoor.gameObject);
        yield return new WaitForSeconds(1f);
        CameraMovement(-5f, 1.5f);
        yield return new WaitForSeconds(1.1f);
        CameraMovement(0f, 1.5f);
        yield return new WaitForSeconds(0.4f);
        StopMission();
        yield return popups.ShowPopups("El juego termina cuando se acaba el tiempo o cuando se sale del living.");
        StartMission();
        CameraMovement(0f);
        yield return new WaitForSeconds(1f);
        MovePlayerForward();
        yield return new WaitForSeconds(0.4f);
        StopPlayerMovement();
        CameraMovement(5f);
        yield return new WaitForSeconds(1f);
        CameraMovement(0f);
        MovePlayerForward();
        yield return null;







        // //StartMission("Mové la cámara para mirar detrás tuyo.");
        // yield return new WaitUntil(() => player.transform.eulerAngles.y > 170 && player.transform.eulerAngles.y < 190); // AllowMovement -> Trigger darse vuelta
        // // Camera control - Part 2
        
        // //StartMission("Mirá la manzana.");
        // yield return new WaitUntil(() => player.transform.eulerAngles.y < 10 || player.transform.eulerAngles.y > 350); // AllowMovement -> Trigger darse vuelta
        // // Grab item
        // StopMission();
        // yield return popups.ShowPopups(new string[] {
        //     "Para interactuar con un elemento, acercate a él hasta que tenga un borde blanco, y presionalo."
        // });
        // StartMission("Acercate y tomá la manzana.");
        // PlayerMovement.allowPlayerMovement = true;
        // yield return new WaitUntil(() => HeldItem.currentlyHeldItem != null && !HeldItem.currentlyHeldItem.isMoving); // AllowMovement -> Trigger Tomar Manzana
        // // Drop Item
        // StopMission();
        // yield return popups.ShowPopups(new string[] {
        //     "Los objetos podés rotarlos deslizando en la pantalla.",
        //     "Para dejar un objeto que estás sosteniendo, presioná el botón rojo."
        // });
        // StartMission("Dejá la manzana.");
        // HeldItem tutorialItem = HeldItem.currentlyHeldItem;
        // yield return new WaitUntil(() => HeldItem.currentlyHeldItem == null && !tutorialItem.isReturning); // AllowMovement -> Trigger Dejar Manzana
        // // Open Door
        // StopMission();
        // yield return popups.ShowPopups(new string[] {
        //     "Las puertas y los cajones de la casa se pueden abrir tocándolos."
        // });
        // StartMission("Abrí la puerta y entrá a la casa.");
        // yield return new WaitUntil(() => houseDoor.isOpen && !houseDoor.IsDoorMoving()); // AllowMovement -> Trigger Abrir Puerta
        // yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Memorizing);
        // yield return new WaitForSeconds(1.5f); // espera para las luces
        // // Memorizing
        // StopMission();
        // yield return popups.ShowPopups(new string[] {
        //     "Hay objetos en el mueble que debes memorizar, puedes interactuar con ellos.",
        //     "Tendrás un tiempo limitado para hacerlo, que se puede ver en el reloj."
        // });
        // StartMission("Observá los objetos.");
        // yield return new WaitUntil(() => Timer.IsTimeOver()); // AllowMovement -> Trigger Se Acaba el Tiempo
        // yield return new WaitForSeconds(1.5f); // espera para las luces
        // // Memorizing Over
        // StopMission();
        // yield return popups.ShowPopups(new string[] {
        //     "Dirigite siguiendo las flechas del piso hasta la habitación que hay más adelante."
        // });
        // StartMission("Entrá en la habitación que hay adelante.");
        // roomIndicators.SetActive(true);
        // yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Search); // AllowMovement -> Trigger Entra En Habitación
        // yield return new WaitForSeconds(1.5f); // espera para las luces
        // // Search
        // StopMission();
        // yield return popups.ShowPopups(new string[] {
        //     "En ésta habitación, estarán los objetos que viste en el hall.",
        //     "Cuando encuentres alguno podrás elegirlo tocándolo dos veces.",
        //     "Buscá la manzana que se encuentra en la habitación y elegila."
        // });
        // StartMission("Encontrá la manzana y elegila.");
        // roomIndicators.SetActive(false);
        // yield return new WaitUntil(() => GameStatus.ContainsItem("Manzana")); // AllowMovement -> Trigger Guardar Manzana
        // GameStatus.SetNextPhase();
        // // Search Over / End Tutorial
        // StopMission();
        // yield return popups.ShowPopups(new string[] {
        //     "Cuando creas que ya encontraste todos los objetos del Hall, salí de la habitación.",
        //     "¡Has completado el tutorial! Podés seguir practicando en la sección Práctica, sin límites de tiempo.",
        //     "Y cuando estés listo, podés jugar los diferentes niveles de dificultad desde el menú principal, en la sección Juego."
        // });
        // StartMission("Salí de la habitación para terminar el tutorial.");
    }

    // public IEnumerator TutorialSequence()
    // {
    //     // Introduction / Player movement
    //     StopMission();
    //     TouchController.allowCameraMovement = false;
    //     PopUpManager popups = PopUpManager.instance;
    //     yield return popups.ShowPopups(new string[] {
    //         "Para moverte, incliná el joystick que está a tu izquierda hacia la dirección deseada."
    //     });
    //     StartMission("Dirigite hacia la casa que está adelante.");
    //     yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_ReachApple); // Trigger Acercarse a Manzana        
    //     // Camera control - Part 1
    //     StopMission();
    //     yield return popups.ShowPopups(new string[] {
    //         "Para mover la cámara, deslizá en la pantalla hacia la dirección deseada."
    //     });
    //     TouchController.allowCameraMovement = true;
    //     PlayerMovement.allowPlayerMovement = false;
    //     StartMission("Mové la cámara para mirar detrás tuyo.");
    //     yield return new WaitUntil(() => player.transform.eulerAngles.y > 170 && player.transform.eulerAngles.y < 190); // AllowMovement -> Trigger darse vuelta
    //     // Camera control - Part 2
    //     StopMission();
    //     yield return popups.ShowPopups("Ahora, volvé a mirar hacía la casa.");
    //     StartMission("Mirá la manzana.");
    //     yield return new WaitUntil(() => player.transform.eulerAngles.y < 10 || player.transform.eulerAngles.y > 350); // AllowMovement -> Trigger darse vuelta
    //     // Grab item
    //     StopMission();
    //     yield return popups.ShowPopups(new string[] {
    //         "Para interactuar con un elemento, acercate a él hasta que tenga un borde blanco, y presionalo."
    //     });
    //     StartMission("Acercate y tomá la manzana.");
    //     PlayerMovement.allowPlayerMovement = true;
    //     yield return new WaitUntil(() => HeldItem.currentlyHeldItem != null && !HeldItem.currentlyHeldItem.isMoving); // AllowMovement -> Trigger Tomar Manzana
    //     // Drop Item
    //     StopMission();
    //     yield return popups.ShowPopups(new string[] {
    //         "Los objetos podés rotarlos deslizando en la pantalla.",
    //         "Para dejar un objeto que estás sosteniendo, presioná el botón rojo."
    //     });
    //     StartMission("Dejá la manzana.");
    //     HeldItem tutorialItem = HeldItem.currentlyHeldItem;
    //     yield return new WaitUntil(() => HeldItem.currentlyHeldItem == null && !tutorialItem.isReturning); // AllowMovement -> Trigger Dejar Manzana
    //     // Open Door
    //     StopMission();
    //     yield return popups.ShowPopups(new string[] {
    //         "Las puertas y los cajones de la casa se pueden abrir tocándolos."
    //     });
    //     StartMission("Abrí la puerta y entrá a la casa.");
    //     yield return new WaitUntil(() => houseDoor.isOpen && !houseDoor.IsDoorMoving()); // AllowMovement -> Trigger Abrir Puerta
    //     yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Memorizing);
    //     yield return new WaitForSeconds(1.5f); // espera para las luces
    //     // Memorizing
    //     StopMission();
    //     yield return popups.ShowPopups(new string[] {
    //         "Hay objetos en el mueble que debes memorizar, puedes interactuar con ellos.",
    //         "Tendrás un tiempo limitado para hacerlo, que se puede ver en el reloj."
    //     });
    //     StartMission("Observá los objetos.");
    //     yield return new WaitUntil(() => Timer.IsTimeOver()); // AllowMovement -> Trigger Se Acaba el Tiempo
    //     yield return new WaitForSeconds(1.5f); // espera para las luces
    //     // Memorizing Over
    //     StopMission();
    //     yield return popups.ShowPopups(new string[] {
    //         "Dirigite siguiendo las flechas del piso hasta la habitación que hay más adelante."
    //     });
    //     StartMission("Entrá en la habitación que hay adelante.");
    //     roomIndicators.SetActive(true);
    //     yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Search); // AllowMovement -> Trigger Entra En Habitación
    //     yield return new WaitForSeconds(1.5f); // espera para las luces
    //     // Search
    //     StopMission();
    //     yield return popups.ShowPopups(new string[] {
    //         "En ésta habitación, estarán los objetos que viste en el hall.",
    //         "Cuando encuentres alguno podrás elegirlo tocándolo dos veces.",
    //         "Buscá la manzana que se encuentra en la habitación y elegila."
    //     });
    //     StartMission("Encontrá la manzana y elegila.");
    //     roomIndicators.SetActive(false);
    //     yield return new WaitUntil(() => GameStatus.ContainsItem("Manzana")); // AllowMovement -> Trigger Guardar Manzana
    //     GameStatus.SetNextPhase();
    //     // Search Over / End Tutorial
    //     StopMission();
    //     yield return popups.ShowPopups(new string[] {
    //         "Cuando creas que ya encontraste todos los objetos del Hall, salí de la habitación.",
    //         "¡Has completado el tutorial! Podés seguir practicando en la sección Práctica, sin límites de tiempo.",
    //         "Y cuando estés listo, podés jugar los diferentes niveles de dificultad desde el menú principal, en la sección Juego."
    //     });
    //     StartMission("Salí de la habitación para terminar el tutorial.");
    // }
}
