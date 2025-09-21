using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameStatus : MonoBehaviour
{
    private static GameStatus instance;

    public enum GamePhase
    {
        Tutorial_Start,
        Tutorial_ReachApple,
        Tutorial_Memorizing,
        Tutorial_BeforeSearch,
        Tutorial_Search,
        Tutorial_SearchOver,
        Waiting,
        BeforeMemorizing,
        Memorizing,
        BeforeSearch,
        Search,
        SearchOver
    };
    public static GamePhase currentPhase;
    public static List<HeldItem> savedItems = new List<HeldItem>();
    public static List<HeldItem> keyItems = new List<HeldItem>();
    public static List<HeldItem> decoyItems = new List<HeldItem>();
    public static List<GameObject>
        itemsToMemorize = new(),
        itemsInEnvironment = new();

    public static int
        timeUsedMemorizing = 0,
        timeUsedSearching = 0;

    //public Light memorizeLight;
    //public Light searchLight;
    public List<GameObject> lights;

    public GameObject invisibleWall;
    private const float lightIntensity = 0.71f;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        if (currentPhase == GamePhase.Tutorial_Start)
        {
            instance.StartTutorial();
        }
        else
        {
            Interactable.allowAllInteractions = true;

            if (currentPhase == GamePhase.Waiting && Settings.currentDifficulty == Settings.Difficulty.Preevaluación)
            {
                StartCoroutine(ShowPracticePopup());
            }
        }
        Debug.Log(currentPhase);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Add the item passed as parameter to the list of items retrieved by the user.
    /// </summary>
    /// <param name="item"></param>
    public static void SaveItem(HeldItem item)
    {
        savedItems.Add(item);
        InventoryDisplay.AddItemToInventory(item.uiIcon);
    }

    public static bool ContainsItem(string itemName)
    {
        return savedItems.Any(item => item.itemName == itemName);
    }

    public static void SetNextPhase()
    {
        switch (currentPhase)
        {
            case GamePhase.Tutorial_ReachApple:
                TurnLightsOn(instance.lights[0]);
                Timer.StartTimer(32);
                SwapMaterial.SetMaterials(true);
                Debug.Log("SWAPMATERIAL SET TO TRUE");
                break;
            case GamePhase.Tutorial_Memorizing:
                instance.StartCoroutine(instance.TurnLightsOff(instance.lights[0], 0f, false));
                if (HeldItem.currentlyHeldItem != null)
                {
                    HeldItem.ReturnItem();
                }
                foreach (GameObject go in itemsToMemorize)
                {
                    Destroy(go);
                }
                instance.invisibleWall.SetActive(false);
                instance.lights[1].SetActive(true);
                break;
            case GamePhase.Tutorial_BeforeSearch:
                instance.lights[1].SetActive(false);
                SwapMaterial.SetMaterials(true);
                TurnLightsOn(instance.lights[2]);
                Timer.StartTimer(125);
                break;
            case GamePhase.Tutorial_Search:
                savedItems.Clear();
                keyItems.Clear();
                decoyItems.Clear();
                OpenDoor.EnableInteractions(false);
                OpenDrawer.EnableInteractions(false);
                // Agregado para que no se vea el borde blanco cuando se apaga la luz
                foreach (GameObject go in itemsInEnvironment)
                {
                    go.GetComponent<Outline>().enabled = false;
                    go.GetComponent<Interactable>().stoppedInteraction = true;
                }
                instance.StartCoroutine(instance.TurnLightsOff(instance.lights[2], 1f, false));
                SwapMaterial.SetMaterials(false);
                Timer.StopTimer();
                break;
            case GamePhase.Tutorial_SearchOver:
                break;
            case GamePhase.Waiting:
                Logging.Log(Logging.EventType.PhaseChange, new[] { "IM" });
                break;
            case GamePhase.BeforeMemorizing:
                TurnLightsOn(instance.lights[0]);
                //instance.StartCoroutine(instance.FadeInLight(instance.memorizeLight, 0f));
                Timer.StartTimer(GameConfig.memorizeTime);
                SwapMaterial.SetMaterials(true);
                Logging.Log(Logging.EventType.PhaseChange, new[] { "M" });
                break;
            case GamePhase.Memorizing:
                if (GameConfig.memorizeTime != 0)
                {
                    instance.StartCoroutine(instance.TurnLightsOff(instance.lights[0], 0f, false));
                }
                //if (Timer.timerOn) eliminado para que funcione el tiempo de memorización cuando se llega a 0
                Timer.StopTimer();
                timeUsedMemorizing = Timer.spentTime;
                PlayerMovement.allowPlayerMovement = true;
                TouchController.allowCameraMovement = true;
                if (GameConfig.memorizeTime != 0)
                {
                    if (HeldItem.currentlyHeldItem != null)
                    {
                        HeldItem.ReturnItem();
                    }
                    // Quitando el siguiente foreach los objetos se quedan post memorizar
                    // foreach (GameObject go in itemsToMemorize){
                    //     Destroy(go);
                    // }
                    // foreach que permite quitar la interacción y los bordes con los objetos una vez finalizado el tiempo de memorización                    
                    foreach (GameObject go in itemsToMemorize)
                    {
                        go.GetComponent<Outline>().enabled = false;
                        go.GetComponent<Interactable>().isInteractable = false;
                        go.GetComponent<Interactable>().stoppedInteraction = true;
                    }
                }
                instance.invisibleWall.SetActive(false);
                instance.lights[1].SetActive(true);
                SwapMaterial.SetMaterials(false);
                Logging.Log(Logging.EventType.PhaseChange, new[] { "FM" });
                break;
            case GamePhase.BeforeSearch:
                instance.lights[1].SetActive(false);
                SwapMaterial.SetMaterials(true);
                TurnLightsOn(instance.lights[2]);
                //instance.StartCoroutine(instance.FadeInLight(instance.searchLight, 0f));
                savedItems.Clear();
                if (GameConfig.searchTime != 0)
                {
                    Timer.StartTimer(GameConfig.searchTime);
                }
                Logging.Log(Logging.EventType.PhaseChange, new[] { "B" });
                break;
            case GamePhase.Search:
                //if (Timer.timerOn) eliminado para que funcione el tiempo de memorización cuando se llega a 0
                Logging.Log(Logging.EventType.PhaseChange, new[] { "FB" });
                if (!Timer.timerOn)
                {
                    OpenDoor.EnableInteractions(false);
                    OpenDrawer.EnableInteractions(false);
                    // Agregado para que no se vea el borde blanco cuando se apaga la luz
                    foreach (GameObject go in itemsInEnvironment)
                    {
                        go.GetComponent<Outline>().enabled = false;
                        go.GetComponent<Interactable>().stoppedInteraction = true;
                    }
                    SwapMaterial.SetMaterials(false);
                    instance.StartCoroutine(instance.TurnLightsOff(instance.lights[2], 1f, Settings.currentDifficulty != Settings.Difficulty.Preevaluación));
                    Timer.StopTimer();
                    timeUsedSearching = Timer.spentTime;
                    if (Settings.currentDifficulty == Settings.Difficulty.Preevaluación)
                    {
                        instance.StartCoroutine(WaitAndExitWithoutSaving());
                    }
                }
                else
                {
                    Timer.StopTimer();
                    timeUsedSearching = Timer.spentTime;
                    if (Settings.currentDifficulty == Settings.Difficulty.Preevaluación)
                    {
                        instance.StartCoroutine(WaitAndExitWithoutSaving());
                    }
                    else
                    {
                        Debug.Log("LOAD SCENE 3");
                        Logging.LogEvent.SaveLogToFile();
                        SceneLoader.LoadScene("Results");
                    }
                }
                break;
            case GamePhase.SearchOver:
                if (Settings.currentDifficulty != Settings.Difficulty.Preevaluación)
                {
                    Logging.LogEvent.SaveLogToFile();
                    Debug.Log("LOAD SCENE 2");
                    SceneLoader.LoadScene("Results");
                }
                return; // Final phase
        }
        currentPhase++;
    }

    public void StartTutorial()
    {
        StartCoroutine(TutorialManager.instance.TutorialSequence());
    }

    private static void TurnLightsOn(GameObject lights)
    {
        lights.SetActive(true);
        RenderSettings.ambientLight = new Color(lightIntensity, lightIntensity, lightIntensity);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lights">Light object to set off</param>
    /// <param name="duration"></param>
    /// <param name="sendToResults"></param>
    /// <returns></returns>
    private IEnumerator TurnLightsOff(GameObject lights, float duration, bool sendToResults)
    {
        float elapsedTime = 0f;
        lights.SetActive(false);
        RenderSettings.ambientLight = new Color(0.1019608f, 0.1019608f, 0.1019608f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (sendToResults)
        {
            Logging.LogEvent.SaveLogToFile();
            Debug.Log("LOAD SCENE 1");
            SceneLoader.LoadScene("Results");
        }
    }

    /// <summary>
    /// Quits the application while deleting items saved by the player.
    /// </summary>
    public static void ExitWithoutSaving()
    {
        keyItems.Clear();
        savedItems.Clear();
        decoyItems.Clear();
        Time.timeScale = 1;
        SceneLoader.LoadScene("MainMenu");
    }

    public static IEnumerator WaitAndExitWithoutSaving()
    {
        yield return new WaitForSeconds(2f);
        ExitWithoutSaving();
    }

    private IEnumerator ShowPracticePopup()
    {
        PopUpManager popups = PopUpManager.instance;

        PlayerMovement.allowPlayerMovement = false;
        TouchController.allowCameraMovement = false;

        yield return popups.ShowPopups("En esta práctica, es posible recorrer\n el ambiente en el que se desarrolla el juego,\n y experimentar las tareas descriptas en el tutorial.");

        PlayerMovement.allowPlayerMovement = true;
        TouchController.allowCameraMovement = true;
    }

    public static bool IsKeyItem(string itemName)
    {
        return keyItems.Any(item => item.itemName == itemName);
    }

}
