using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System.Linq;

public class HistoryLoader : MonoBehaviour
{
    public static HistoryLoader Instance { get; private set; }

    public List<ResultData> historyResults = new List<ResultData>();
    private FirebaseDatabase db;
    //private ListenerRegistration listener;

    public delegate void HistoryUpdated();
    public static event HistoryUpdated OnHistoryUpdated;

    private bool firebaseReady = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadHistory();

        // Esperar a que Firebase se inicialice
        //FirebaseInit.OnFirebaseInitialized += OnFirebaseInitialized;
        db = FirebaseDatabase.DefaultInstance;

        // Intentar comenzar a escuchar si ya teníamos el email
        string email = PatientInfoLoader.Instance?.GetPatientEmail();

        if (!string.IsNullOrEmpty(email))
        {
            StartDatabase(email);
        }

        // Suscribirse al evento del email
        PatientInfoLoader.OnEmailLoaded += StartDatabase;
    }

    // private void OnDestroy()
    // {
    //     //FirebaseInit.OnFirebaseInitialized -= OnFirebaseInitialized;
    //     PatientInfoLoader.OnEmailLoaded -= StartListeningToFirestore;
    // }

    public void StartDatabase(string email)
    {
        Query resultsRef = db.GetReference("results");

        resultsRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("[HistoryLoader] Error al buscar resultados: " + task.Exception);
                return;
            }

            if (!task.Result.Exists)
            {
                Debug.LogWarning("[HistoryLoader] No hay resultados registrados.");
                return;
            }

            DataSnapshot resultsSnapshot = task.Result;
            DataSnapshot patientSnapshot = null;

            // Buscar el nodo que tenga el email como idPatient
            foreach (DataSnapshot child in resultsSnapshot.Children)
            {
                var idPatientSnap = child.Child("idPatient");
                if (idPatientSnap.Exists && idPatientSnap.Value.ToString() == email)
                {
                    patientSnapshot = child;
                    break;
                }
            }

            if (patientSnapshot == null)
            {
                Debug.LogWarning("[HistoryLoader] No se encontró ningún paciente con ese email.");
                return;
            }

            historyResults.Clear();
            
            // Ahora accedemos a los registros del paciente
            DataSnapshot recordsSnapshot = patientSnapshot.Child("results");

            foreach (DataSnapshot recordSnap in recordsSnapshot.Children)
            {
                Dictionary<string, object> data = (Dictionary<string, object>)recordSnap.GetValue(false);
                List<string> memObjectsList = new();
                List<string> foundObjectsList = new();

                if (data.TryGetValue("memObjects", out var memObjects) && memObjects is List<object> memList)
                    memObjectsList = memList.Select(item => item.ToString()).ToList();

                if (data.TryGetValue("foundObjects", out var foundObjects) && foundObjects is List<object> foundList)
                    foundObjectsList = foundList.Select(item => item.ToString()).ToList();

                ResultData newResult = new()
                {
                    id = data.TryGetValue("id", out var idVal) ? int.Parse(idVal.ToString()) : 0,
                    date = data.TryGetValue("date", out var dateVal) ? dateVal.ToString() : "",
                    time = data.TryGetValue("time", out var timeVal) ? timeVal.ToString() : "",
                    level = data.TryGetValue("level", out var levelVal) ? levelVal.ToString() : "",
                    memorizeTime = data.TryGetValue("memTime", out var memTimeVal) ? memTimeVal.ToString() : "",
                    searchTime = data.TryGetValue("searchTime", out var searchTimeVal) ? searchTimeVal.ToString() : "",
                    keyImageName = memObjectsList,
                    foundImageName = foundObjectsList
                };

                historyResults.Add(newResult);
            }

            SaveHistory();
            Debug.Log("[HistoryLoader] Registros cargados exitosamente para " + email);
            OnHistoryUpdated?.Invoke();
        });
    }

    public void StopListeningToFirestore()
    {
        // if (listener != null)
        // {
        //     listener.Stop();
        //     listener = null;
        //     Debug.Log("[HistoryLoader] Listener detenido.");
        // }
    }

    private void SetEmptyHistorialFile(string filePath)
    {
        HistoryData emptyHistoryData = new() { results = new List<ResultData>() };
        string json = JsonUtility.ToJson(emptyHistoryData, true);
        File.WriteAllText(filePath, json);
    }

    public void SaveHistory()
    {
        HistoryData historyData = new() { results = historyResults };
        string json = JsonUtility.ToJson(historyData, true);
        File.WriteAllText(GetFilePath(), json);
    }

    public void CleanHistorialFile()
    {
        StopListeningToFirestore();
        historyResults.Clear();
        SetEmptyHistorialFile(GetFilePath());
    }

    public void AddResult(ResultData newResult)
    {
        historyResults.Add(newResult);
        SaveHistory();
    }

    public int GenerateUniqueId()
    {
        return historyResults.Count == 0 ? 1 : historyResults.Max(r => r.id) + 1;
    }

    private string GetFilePath()
    {
        return Path.Combine(Application.persistentDataPath, "history.json");
    }

    private void OnApplicationQuit()
    {
        StopListeningToFirestore();
    }

    private void LoadHistory()
    {
        string filePath = GetFilePath();
        if (!File.Exists(filePath))
        {
            SetEmptyHistorialFile(filePath);

            Debug.LogWarning("No se encontró el archivo JSON.");
            return;
        }
        string json = File.ReadAllText(filePath);
        HistoryData historyData = JsonUtility.FromJson<HistoryData>(json);

        historyResults = historyData.results;
    }
}