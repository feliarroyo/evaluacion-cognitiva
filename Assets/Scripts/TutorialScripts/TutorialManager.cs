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
    public GameObject player;
    public GameObject roomIndicators; // arrows showing the way to the search room.
    public GameObject preevaluationElements;
    
    // Start is called before the first frame update
    void Start(){
        if (instance == null){
            instance = this;
        }
        Debug.Log("La fase en este momento es : " + GameStatus.currentPhase);
        missionText = mission.GetComponentInChildren<TextMeshProUGUI>();
        if (GameStatus.currentPhase != GameStatus.GamePhase.Tutorial_Start){
            preevaluationElements.SetActive(false);
            PlayerMovement.allowPlayerMovement = true;
            TouchController.allowCameraMovement = true;
        }
        
    }

    private void StartMission(string text){
        missionText.text = text;
        mission.SetActive(true);
        Time.timeScale = 1;
    }

    private void StopMission(){
        Time.timeScale = 0;
        mission.SetActive(false);
    }
    public IEnumerator TutorialSequence()
    {
        // Introduction / Player movement
        StopMission();
        TouchController.allowCameraMovement = false;
        PopUpManager popups = PopUpManager.instance;
        yield return popups.ShowPopups(new string[] {
            "Para moverte, incliná el joystick que está a tu izquierda hacia la dirección deseada."
        });
        StartMission("Dirigite hacia la casa que está adelante.");
        yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_ReachApple); // Trigger Acercarse a Manzana        
        // Camera control - Part 1
        StopMission();
        yield return popups.ShowPopups(new string[] {
            "Para mover la cámara, deslizá en la pantalla hacia la dirección deseada."
        });
        TouchController.allowCameraMovement = true;
        PlayerMovement.allowPlayerMovement = false;
        StartMission("Mové la cámara para mirar detrás tuyo.");
        yield return new WaitUntil(() => player.transform.eulerAngles.y > 170 && player.transform.eulerAngles.y < 190); // AllowMovement -> Trigger darse vuelta
        // Camera control - Part 2
        StopMission();
        yield return popups.ShowPopups("Ahora, volvé a mirar hacía la casa.");
        StartMission("Mirá la manzana.");
        yield return new WaitUntil(() => player.transform.eulerAngles.y < 10 || player.transform.eulerAngles.y > 350); // AllowMovement -> Trigger darse vuelta
        // Grab item
        StopMission();
        yield return popups.ShowPopups(new string[] {
            "Para interactuar con un elemento, acercate a él hasta que tenga un borde blanco, y presionalo."
        });
        StartMission("Acercate y tomá la manzana.");
        PlayerMovement.allowPlayerMovement = true;
        yield return new WaitUntil(() => HeldItem.currentlyHeldItem != null && !HeldItem.currentlyHeldItem.isMoving); // AllowMovement -> Trigger Tomar Manzana
        // Drop Item
        StopMission();
        yield return popups.ShowPopups(new string[] {
            "Los objetos podés rotarlos deslizando en la pantalla.",
            "Para dejar un objeto que estás sosteniendo, presioná el botón rojo."
        });
        StartMission("Dejá la manzana.");
        HeldItem tutorialItem = HeldItem.currentlyHeldItem;
        yield return new WaitUntil(() => HeldItem.currentlyHeldItem == null && !tutorialItem.isReturning); // AllowMovement -> Trigger Dejar Manzana
        // Open Door
        StopMission();
        yield return popups.ShowPopups(new string[] {
            "Las puertas y los cajones de la casa se pueden abrir tocándolos."
        });
        StartMission("Abrí la puerta y entrá a la casa.");
        yield return new WaitUntil(() => houseDoor.isOpen && !houseDoor.IsDoorMoving()); // AllowMovement -> Trigger Abrir Puerta
        yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Memorizing);
        yield return new WaitForSeconds(1.5f); // espera para las luces
        // Memorizing
        StopMission();
        yield return popups.ShowPopups(new string[] {
            "Hay objetos en el mueble que debes memorizar, puedes interactuar con ellos.",
            "Tendrás un tiempo limitado para hacerlo, que se puede ver en el reloj."
        });
        StartMission("Observá los objetos.");
        yield return new WaitUntil(() => Timer.IsTimeOver()); // AllowMovement -> Trigger Se Acaba el Tiempo
        yield return new WaitForSeconds(1.5f); // espera para las luces
        // Memorizing Over
        StopMission();
        yield return popups.ShowPopups(new string[] {
            "Dirigite siguiendo las flechas del piso hasta la habitación que hay más adelante."
        });
        StartMission("Entrá en la habitación que hay adelante.");
        roomIndicators.SetActive(true);
        yield return new WaitUntil(() => GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Search); // AllowMovement -> Trigger Entra En Habitación
        yield return new WaitForSeconds(1.5f); // espera para las luces
        // Search
        StopMission();
        yield return popups.ShowPopups(new string[] {
            "En ésta habitación, estarán los objetos que viste en el hall.",
            "Cuando encuentres alguno podrás elegirlo tocándolo dos veces.",
            "Buscá la manzana que se encuentra en la habitación y elegila."
        });
        StartMission("Encontrá la manzana y elegila.");
        roomIndicators.SetActive(false);
        yield return new WaitUntil(() => GameStatus.ContainsItem("Manzana")); // AllowMovement -> Trigger Guardar Manzana
        GameStatus.SetNextPhase();
        // Search Over / End Tutorial
        StopMission();
        yield return popups.ShowPopups(new string[] {
            "Cuando creas que ya encontraste todos los objetos del Hall, salí de la habitación.",
            "¡Has completado el tutorial! Podés seguir practicando en la sección Práctica, sin límites de tiempo.",
            "Y cuando estés listo, podés jugar los diferentes niveles de dificultad desde el menú principal, en la sección Juego."
        });
        StartMission("Salí de la habitación para terminar el tutorial.");
    }
}
