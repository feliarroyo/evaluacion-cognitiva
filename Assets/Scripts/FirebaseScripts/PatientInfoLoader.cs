using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Linq;

public class PatientInfoLoader : MonoBehaviour
{
    public static PatientInfoLoader Instance { get; private set; }
    private FirebaseDatabase db;
    private string filePath;
    private string email;
    private PatientData patientData;

    public delegate void EmailLoadedHandler(string email);
    public static event EmailLoadedHandler OnEmailLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            db = FirebaseDatabase.DefaultInstance;
            OnFirebaseReady();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
    }

    // private void OnDestroy()
    // {
    //     //StopListeningToPatientData();
    // }

    private void OnFirebaseReady()
    {
        // db = FirebaseInit.FirestoreInstance;
        filePath = Path.Combine(Application.persistentDataPath, "patient_info.json");
        LoadEmailFromJson();

        if (patientData != null && !string.IsNullOrEmpty(patientData.email))
        {
            SetEmailAndLoadData(patientData.email);
        }
    }

    /// <summary>
    /// Create patient data, if not previously loaded.
    /// </summary>
    public void LoadEmailFromJson()
    {
        if (patientData != null && !string.IsNullOrEmpty(patientData.email))
        {
            return; // Ya cargado
        }

        if (!File.Exists(filePath))
        {
            patientData = new PatientData(); // archivo no existe, inicializo vacío
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(filePath);
            PatientData data = JsonUtility.FromJson<PatientData>(jsonContent);

            patientData = data ?? new PatientData();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error leyendo patient_info.json: {ex.Message}");
            patientData = new PatientData();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userEmail"></param>
    public void SetEmailAndLoadData(string userEmail)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogWarning("[PatientInfoLoader] Email recibido vacío.");
            return;
        }

        email = userEmail;
        LoadPatientData(); // Carga local
        FetchPatientDataFromFirestore(email); // Listener en Firestore

        OnEmailLoaded?.Invoke(email);
    }

    private void FetchPatientDataFromFirestore(string email)
    {
        //if (patientListener != null)
        //{
        //    patientListener.Stop();
        //    patientListener = null;
        //}

        Query mailReference = db.GetReference("pacientes").OrderByChild("email").EqualTo(email);

        mailReference.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the error...
            }
            else if (task.IsCompleted)
            {
                DataSnapshot baseSnapshot = task.Result;
                foreach (DataSnapshot snapshot in baseSnapshot.Children)
                {

                    Dictionary<string, object> patientDataMap = (Dictionary<string, object>)snapshot.GetValue(false);
                    var patientConfigLevels = new List<LevelConfig>();

                    if (patientDataMap["levelsConfig"] is Dictionary<string, object> levelsMap)
                    {
                        foreach (var kvp in levelsMap)
                        {
                            if (kvp.Value is Dictionary<string, object> levelData)
                            {
                                LevelConfig config = new LevelConfig
                                {
                                    id = kvp.Key,
                                    memorizeTime = levelData.ContainsKey("timeMem") ? int.Parse(levelData["timeMem"].ToString()) : 0,
                                    searchTime = levelData.ContainsKey("timeSearch") ? int.Parse(levelData["timeSearch"].ToString()) : 0,
                                    searchObjects = SaveObjects(levelData, new List<SpawnInfo>(), "searchItems"),
                                    distractingObjects = SaveObjects(levelData, new List<SpawnInfo>(), "distractingItems")
                                };
                                patientConfigLevels.Add(config);
                            }
                        }
                    }
                    SavePatientData(patientConfigLevels);
                }
            }
        }
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="levelData"></param>
    /// <param name="config"></param>
    /// <param name="itemsName"></param>
    /// <returns></returns>
    private List<SpawnInfo> SaveObjects(Dictionary<string, object> levelData, List<SpawnInfo> config, string itemsName)
    {
        // In case of error, return empty list
        if (!levelData.ContainsKey(itemsName)) return new List<SpawnInfo>();

        //Debug.Log("SetupScene 1");
        // In case of wrong format, return empty list
        if (levelData[itemsName] is not Dictionary<string, object> searchMap) 
        return new List<SpawnInfo>();

        //Debug.Log("SetupScene 2");

        config.Clear();

        foreach (var zone in searchMap)
        {
            if (zone.Value is string v) {
                //Debug.Log("For iteration");
                config.Add(new SpawnInfo(zone.Key, v));
            }
        }
        Debug.Log("SetupScene 3");

        return config;
    }

    private void LoadPatientData()
    {
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                patientData = JsonUtility.FromJson<PatientData>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error leyendo patient_info.json: {ex.Message}");
                patientData = new PatientData { email = "", levels = new List<LevelConfig>() };
            }
        }
        else
        {
            Debug.LogWarning("[PatientInfoLoader] No se encontró el archivo patient_info.json");
            patientData = new PatientData { email = "", levels = new List<LevelConfig>() };
        }
    }

    private void SavePatientData(List<LevelConfig> levels)
    {
        if (patientData == null)
            patientData = new PatientData();
 
        patientData.email = email;
        patientData.levels = levels ?? new List<LevelConfig>();
        try
        {
            string json = JsonUtility.ToJson(patientData, true);
            File.WriteAllText(filePath, json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error guardando patient_info.json: {ex.Message}");
        }
    }

    public void CleanPatientFile()
    {
        //StopListeningToPatientData();

        patientData = new PatientData { email = "", levels = new List<LevelConfig>() };

        try
        {
            string json = JsonUtility.ToJson(patientData, true);
            File.WriteAllText(filePath, json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error limpiando patient_info.json: {ex.Message}");
        }
    }

    // public void StopListeningToPatientData()
    // {
    //     if (patientListener != null)
    //     {
    //         patientListener.Stop();
    //         patientListener = null;
    //         Debug.Log("[PatientInfoLoader] Listener detenido.");
    //     }
    // }

    public string GetPatientEmail()
    {
        return patientData?.email ?? "";
    }
}