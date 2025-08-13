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

    public enum GamePhase {
        Tutorial_Start, 
        Tutorial_ReachApple, 
        Tutorial_Memorizing, 
        Tutorial_BeforeSearch, 
        Tutorial_Search, 
        Tutorial_SearchOver, 
        Waiting, 
        Memorizing, 
        BeforeSearch, 
        Search, 
        SearchOver
    };
    public static GamePhase currentPhase;
    public static List<HeldItem> savedItems = new List<HeldItem>();
    public static List<HeldItem> keyItems = new List<HeldItem>();
    public static List<GameObject> 
        itemsToMemorize = new(), 
        itemsInEnvironment = new();

    public static int 
        timeUsedMemorizing = 0,
        timeUsedSearching = 0
        ;

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
    public static void SaveItem(HeldItem item){
        savedItems.Add(item);
        InventoryDisplay.AddItemToInventory(item.uiIcon);
    }

    public static bool ContainsItem(string itemName){
        return savedItems.Any(item => item.itemName == itemName);
    }
    
    public static void SetNextPhase(){
        Debug.Log(currentPhase);
        Logging.Log(Logging.EventType.PhaseEnd, new[] { currentPhase.ToString() });
        switch (currentPhase){
            case GamePhase.Tutorial_ReachApple:
                TurnLightsOn(instance.lights[0]);
                Timer.StartTimer(10);
                break;
            case GamePhase.Tutorial_Memorizing:
                instance.StartCoroutine(instance.TurnLightsOff(instance.lights[0], 0f, false));
                if (HeldItem.currentlyHeldItem != null) {
                    HeldItem.ReturnItem();
                }
                foreach (GameObject go in itemsToMemorize){
                    Destroy(go);
                }
                instance.invisibleWall.SetActive(false);
                instance.lights[1].SetActive(true);
                break;
            case GamePhase.Tutorial_BeforeSearch:
                instance.lights[1].SetActive(false);
                TurnLightsOn(instance.lights[2]);
                Timer.StartTimer(60);
                break;
            case GamePhase.Tutorial_Search:
                savedItems.Clear();
                keyItems.Clear();
                SceneManager.LoadScene("MainMenu");
                break;
            case GamePhase.Tutorial_SearchOver:
                break;
            case GamePhase.Waiting:
                TurnLightsOn(instance.lights[0]);
                //instance.StartCoroutine(instance.FadeInLight(instance.memorizeLight, 0f));
                Timer.StartTimer(GameConfig.memorizeTime);
                break;
            case GamePhase.Memorizing:
                if (GameConfig.memorizeTime != 0) {
                    instance.StartCoroutine(instance.TurnLightsOff(instance.lights[0], 0f, false));
                }
                //if (Timer.timerOn) eliminado para que funcione el tiempo de memorización cuando se llega a 0
                Timer.StopTimer();
                timeUsedMemorizing = Timer.spentTime;
                PlayerMovement.allowPlayerMovement = true;
                TouchController.allowCameraMovement = true;
                if (GameConfig.memorizeTime != 0) {
                    if (HeldItem.currentlyHeldItem != null) {
                        HeldItem.ReturnItem();
                    }
                    // Quitando el siguiente foreach los objetos se quedan post memorizar
                    // foreach (GameObject go in itemsToMemorize){
                    //     Destroy(go);
                    // }
                    // foreach que permite quitar la interacción y los bordes con los objetos una vez finalizado el tiempo de memorización                    
                    foreach (GameObject go in itemsToMemorize){
                        go.GetComponent<Outline>().enabled = false;
                        go.GetComponent<Interactable>().isInteractable = false;
                        go.GetComponent<Interactable>().stoppedInteraction = true;
                    }
                }
                instance.invisibleWall.SetActive(false);
                instance.lights[1].SetActive(true);
                break;
            case GamePhase.BeforeSearch:
                instance.lights[1].SetActive(false);
                TurnLightsOn(instance.lights[2]);
                //instance.StartCoroutine(instance.FadeInLight(instance.searchLight, 0f));
                savedItems.Clear();
                if (GameConfig.searchTime != 0) {
                    Timer.StartTimer(GameConfig.searchTime);
                }
                break;
            case GamePhase.Search:
                //if (Timer.timerOn) eliminado para que funcione el tiempo de memorización cuando se llega a 0
                if (!Timer.timerOn){
                    OpenDoor.EnableInteractions(false);
                    OpenDrawer.EnableInteractions(false);
                    // Agregado para que no se vea el borde blanco cuando se apaga la luz
                    foreach (GameObject go in itemsInEnvironment){
                        go.GetComponent<Outline>().enabled = false;
                        go.GetComponent<Interactable>().stoppedInteraction = true;
                    }
                    instance.StartCoroutine(instance.TurnLightsOff(instance.lights[2], 1f, Settings.currentDifficulty != Settings.Difficulty.Preevaluación));  
                    Timer.StopTimer();
                    timeUsedSearching = Timer.spentTime;
                    ExitWithoutSaving();
                }else{
                    Timer.StopTimer();
                    timeUsedSearching = Timer.spentTime;
                    Logging.LogEvent.SaveLogToFile();
                    SceneLoader.LoadScene("Results");
                }
                
                break;
            case GamePhase.SearchOver:
                if (Settings.currentDifficulty != Settings.Difficulty.Preevaluación){
                    Logging.LogEvent.SaveLogToFile();
                    SceneLoader.LoadScene("Results");
                }
                return; // Final phase
        }
        currentPhase++;
        Logging.Log(Logging.EventType.PhaseStart, new[] { currentPhase.ToString() });
    }

    public void StartTutorial() {
        StartCoroutine(TutorialManager.instance.TutorialSequence());
    }

    private static void TurnLightsOn(GameObject lights){
         lights.SetActive(true);
         RenderSettings.ambientLight = new Color(lightIntensity,lightIntensity,lightIntensity);
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
    RenderSettings.ambientLight = new Color(0.1019608f,0.1019608f,0.1019608f);
    
    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        yield return null;
    }

    if(sendToResults){
        Logging.LogEvent.SaveLogToFile();
        SceneLoader.LoadScene("Results");
    }
}

/// <summary>
/// Quits the application while deleting items saved by the player.
/// </summary>
public static void ExitWithoutSaving(){
    keyItems.Clear();
    savedItems.Clear();
    SceneLoader.LoadScene("MainMenu");
}

}
