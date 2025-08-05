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
        switch (currentPhase){
            case GamePhase.Tutorial_ReachApple:
                TurnLightsOn(instance.lights[0]);
                //instance.StartCoroutine(instance.FadeInLight(instance.memorizeLight, 0f));
                Timer.StartTimer(10);
                break;
            case GamePhase.Tutorial_Memorizing:
                instance.StartCoroutine(instance.TurnLightsOff(instance.lights[0], 0f, false));
                // if (Timer.timerOn)
                //     Timer.StopTimer();
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
                //instance.StartCoroutine(instance.FadeInLight(instance.searchLight, 0f));
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
                }else{
                    Timer.StopTimer();
                    timeUsedSearching = Timer.spentTime;
                    SceneLoader.LoadScene("Results");
                }
                
                break;
            case GamePhase.SearchOver:
                if (Settings.currentDifficulty != Settings.Difficulty.Preevaluación){
                    SceneLoader.LoadScene("Results");
                }
                else {
                    keyItems.Clear();
                    savedItems.Clear();
                    SceneLoader.LoadScene("MainMenu");
                }

                return; // Final phase
        }
        Logging.Log(Logging.EventType.PhaseEnd, new[] { currentPhase.ToString() });
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

    // private IEnumerator FadeInLight(GameObject lights, float duration)
    // {
    //     //float elapsedTime = 0f;
    //     // float startIntensity = 0f;
    //     //float targetIntensity = 3f;

    //     // light.intensity = startIntensity;
    //     lights.SetActive(true);
    //     RenderSettings.ambientLight = new Color(152,152,152);

    //     //while (elapsedTime < duration)
    //     //{
    //         //elapsedTime += Time.deltaTime;
    //         //light.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / duration);
    //         //yield return null;
    //     //}

    //     //light.intensity = targetIntensity;
    //     yield return n
    // }

private IEnumerator TurnLightsOff(GameObject lights, float duration, bool searchPhase)
{
    float elapsedTime = 0f;
    //float startIntensity = light.intensity;
    //float startRange = light.range;
    //float startAngle = light.spotAngle;
    
    lights.SetActive(false);
    RenderSettings.ambientLight = new Color(0.1019608f,0.1019608f,0.1019608f);
    
    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        // float t = elapsedTime / duration;
        // light.intensity = Mathf.Lerp(startIntensity, 0f, t);
        // light.range = Mathf.Lerp(startRange, 5f, t);
        // light.spotAngle = Mathf.Lerp(startAngle, 15f, t);
        yield return null;
    }

    //light.intensity = 0f;
    //light.range = 5f;
    //light.spotAngle = 15f;
    
    if(searchPhase)
        SceneLoader.LoadScene("Results");
}


}
