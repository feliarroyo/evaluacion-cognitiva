using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    public static GameConfig instance;
    public List<GameObject> allItemsList = new List<GameObject>();
    public List<GameObject> keyItemList = new List<GameObject>(); // Contains items to be searched
    public List<GameObject> decoyItemList = new List<GameObject>(); // Contains items to fill out room
    public List<GameObject> tutorialKeyItemList = new List<GameObject>(); // Contains items used in pre-evaluation
    public List<GameObject> tutorialDecoyItemList = new List<GameObject>(); // Contains items used in pre-evaluation
    public List<GameObject> preevaluationKeyItemList = new List<GameObject>(); // Contains items used in pre-evaluation
    public List<GameObject> preevaluationDecoyItemList = new List<GameObject>(); // Contains items used in pre-evaluation
    
    public static int memorizeTime = 15, searchTime = 18;
    // public static int keyItemQuantity, decoyItemQuantity;
    public static Dictionary<string, string> keyItems, decoyItems;

    private PatientData configData;

    // Start is called before the first frame update
    // void Start(){
    //     if (instance == null) {
    //         instance = this;
    //         Debug.Log("GameConfig Start ejecutado. Instancia creada.");
    //     }
    //     Debug.Log("Dificultad actual " + Settings.currentDifficulty);
    // }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameConfig Awake ejecutado. Instancia creada.");
        }
        else
        {
            Destroy(gameObject); // Por si accidentalmente hay más de uno
        }
    }
    
    /// <summary>
    /// Reads the configuration from the patient_info file, previously loaded at login.
    /// </summary>
    void LoadConfigFromJson()
    {
        string path = Path.Combine(Application.persistentDataPath, "patient_info.json"); // o persistentDataPath si estás en móvil
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            configData = JsonUtility.FromJson<PatientData>(json);
            Debug.Log("Configuración cargada desde JSON");
        }
        else
        {
            Debug.LogError("No se encontró el archivo config.json en: " + path);
        }
    }

/// <summary>
/// 
/// </summary>
public static void SetDifficultyParameters()
    {
        instance.LoadConfigFromJson(); // Read configuration from JSON file.
        string difficultyId = "";
        switch (Settings.currentDifficulty)
        {
            case Settings.Difficulty.Preevaluación:
                memorizeTime = 10;
                searchTime = 90;
                break;
            case Settings.Difficulty.Bajo:
                difficultyId = "lowLevel";
                break;
            case Settings.Difficulty.Alto:
                difficultyId = "highLevel";
                break;
        }
        
        // If there's a valid level, initialize values
        LevelConfig selected = instance.configData.levels.Find(l => l.id == difficultyId);
        if (selected != null)
        {
            // Initialize time values
            memorizeTime = selected.memorizeTime;
            searchTime = selected.searchTime;
            
            // Add search items to the key items list
            keyItems = new Dictionary<string, string>();
            foreach (SpawnInfo si in selected.searchObjects)
            {
                keyItems.Add(si.key, si.item);
            }
            
            // Add distracting items to the decoy items list
            decoyItems = new Dictionary<string, string>();
            foreach (SpawnInfo si in selected.distractingObjects)
            {
                decoyItems.Add(si.key, si.item);
            }
        }
    }


    // public static void SetDifficultyParameters(){
    //     switch (Settings.currentDifficulty) {
    //         case Settings.Difficulty.Preevaluación:
    //             memorizeTime = 0;
    //             searchTime = 0;
    //             keyItemQuantity = 7;
    //             decoyItemQuantity = 9;
    //             break;
    //         case Settings.Difficulty.Bajo:
    //             memorizeTime = 45;
    //             searchTime = 65;
    //             keyItemQuantity = 3;
    //             decoyItemQuantity = 5;
    //             break;
    //         case Settings.Difficulty.Medio:
    //             memorizeTime = 35;
    //             searchTime = 55;
    //             keyItemQuantity = 5;
    //             decoyItemQuantity = 7;
    //             break;
    //         case Settings.Difficulty.Alto:
    //             memorizeTime = 5;
    //             searchTime = 5;
    //             keyItemQuantity = 7;
    //             decoyItemQuantity = 9;
    //             break;
    //     }
    // }


    public Dictionary<string, GameObject> GenerateKeyItems()
    {
        Dictionary<string, GameObject> result = new();
        foreach (string spawnType in keyItems.Keys)
        {
            // Busca el objeto en lista en el inspector
            GameObject itemToAdd = allItemsList.FirstOrDefault(go =>
            {
                HeldItem item = go.GetComponent<HeldItem>();
                return item != null && item.itemName == keyItems[spawnType];
            });
            // Agrega a la lista.
            if (itemToAdd != null) {
                Debug.Log(itemToAdd);
                itemToAdd.GetComponent<HeldItem>().isKeyItem = true;
                keyItemList.Add(itemToAdd);
                result[spawnType] = itemToAdd;
            }
            else {
                Debug.Log("Item con nombre inválido: " + keyItems[spawnType]);
                }
        }
        return result;
    }

    public Dictionary<string, GameObject> GenerateDecoyItems()
    {
        Dictionary<string, GameObject> result = new();
        foreach (string spawnType in decoyItems.Keys)
        {
            // Busca el objeto en lista en el inspector
            GameObject itemToAdd = allItemsList.FirstOrDefault(go =>
            {
                HeldItem item = go.GetComponent<HeldItem>();
                return item != null && item.itemName == decoyItems[spawnType];
            });
            if (itemToAdd != null)
            {
                itemToAdd.GetComponent<HeldItem>().isKeyItem = false;
                decoyItemList.Add(itemToAdd);
                result[spawnType] = itemToAdd;
            }
            else
                Debug.Log("Item con nombre inválido: " + decoyItems[spawnType]);
        }
        return result;
    }


    public List<GameObject> GenerateRandomKeyItems()
    {
        for (int i = 0; i < 8; i++){
            GameObject itemToAdd = allItemsList[Random.Range(0, allItemsList.Count)];
            itemToAdd.GetComponent<HeldItem>().isKeyItem = true;
            keyItemList.Add(itemToAdd);
            allItemsList.Remove(itemToAdd);
        }
        return new List<GameObject>(keyItemList);
    }

    public List<GameObject> GenerateRandomDecoyItems()
    {
        for (int i = 0; i < 18; i++){
            GameObject itemToAdd = allItemsList[Random.Range(0, allItemsList.Count)];
            itemToAdd.GetComponent<HeldItem>().isKeyItem = false;
            decoyItemList.Add(itemToAdd);
            allItemsList.Remove(itemToAdd);
        }
        return new List<GameObject>(decoyItemList);
    }

    public List<GameObject> GenerateTutorialKeyItems()
    {
        return new List<GameObject>(tutorialKeyItemList);
    }

    public List<GameObject> GenerateTutorialDecoyItems()
    {
        return new List<GameObject>(tutorialDecoyItemList);
    }

    
}
