using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;


public class Results : MonoBehaviour
{

    public ResultUIManager resultUI;
    private FirebaseDatabase db;
    private string patientEmail;

    void Start()
    {
        db = FirebaseDatabase.DefaultInstance;
        patientEmail = PatientInfoLoader.Instance.GetPatientEmail();

        if (string.IsNullOrEmpty(PatientInfoLoader.Instance.GetPatientEmail()))
        {
            PatientInfoLoader.Instance.LoadEmailFromJson();
        }

        // Espera un frame para asegurar la carga del email
        StartCoroutine(WaitForEmailLoad());

        // List<string> keyItemsNames = new List<string>();
        // List<string> foundItemsNames = new List<string>();

        // foreach (var item in GameStatus.keyItems){
        //     keyItemsNames.Add(item.itemName);
        // }
        // foreach (var item in GameStatus.savedItems){
        //     foundItemsNames.Add(item.itemName);
        // }
        // GameStatus.keyItems.Clear();
        // Debug.Log("TIMER MEM " + GameStatus.timeUsedMemorizing + "search " + GameStatus.timeUsedSearching);
        // SaveResults(keyItemsNames, foundItemsNames);
    }

    IEnumerator WaitForEmailLoad()
    {
        yield return new WaitForEndOfFrame();

        patientEmail = PatientInfoLoader.Instance.GetPatientEmail();

        if (string.IsNullOrEmpty(patientEmail))
        {
            yield break;
        }

        List<string> keyItemsNames = new List<string>();
        List<string> decoyItemsNames = new List<string>();
        List<string> foundItemsNames = new List<string>();
        

        foreach (var item in GameStatus.keyItems)
        {
            keyItemsNames.Add(item.itemName);
        }
        foreach (var item in GameStatus.savedItems)
        {
            foundItemsNames.Add(item.itemName);
        }
        foreach (var item in GameStatus.decoyItems)
        {
            decoyItemsNames.Add(item.itemName);
        }
        GameStatus.keyItems.Clear();
        GameStatus.decoyItems.Clear();
        SaveResults(keyItemsNames, foundItemsNames, decoyItemsNames);
    }

    public void SaveResults(List<string> keyItemsNames, List<string> foundItemsNames, List<string> decoyItemsNames)
    {
        DatabaseReference resultsRef = db.GetReference("results");
        resultsRef.GetValueAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsFaulted)
        {
            Debug.LogError("Error al obtener resultados: " + task.Exception);
            return;
        }

        DataSnapshot root = task.Result;
        string patientNodeKey = null;

        int maxPatientId = 0;

        foreach (var child in root.Children)
        {
            if (int.TryParse(child.Key, out int currentId) && currentId > maxPatientId)
                maxPatientId = currentId;

            var idPatientSnapshot = child.Child("idPatient");
            if (idPatientSnapshot.Exists && idPatientSnapshot.Value.ToString() == patientEmail)
            {
                patientNodeKey = child.Key;
            }
        }

        // Si no hay nodo para este paciente, creamos uno nuevo
        if (patientNodeKey == null)
        {
            maxPatientId++;
            patientNodeKey = maxPatientId.ToString("D6");
        }

        DatabaseReference patientRef = resultsRef.Child(patientNodeKey);
        DatabaseReference recordsRef = patientRef.Child("results");

        recordsRef.GetValueAsync().ContinueWithOnMainThread(recordTask =>
        {
            if (recordTask.IsFaulted)
            {
                Debug.LogError("Error al obtener records del paciente: " + recordTask.Exception);
                return;
            }

            int nextRecordId = 1;

            if (recordTask.Result.Exists)
            {
                foreach (var record in recordTask.Result.Children)
                {
                    if (int.TryParse(record.Key, out int recordId) && recordId >= nextRecordId)
                        nextRecordId = recordId + 1;
                }
            }

            string newRecordId = nextRecordId.ToString("D6");

            ResultData newResult = new ResultData
            {
                id = nextRecordId,
                date = System.DateTime.Now.ToString("yyyy-MM-dd"),
                time = System.DateTime.Now.ToString("HH:mm:ss"),
                level = Settings.currentDifficulty.ToString(),
                memorizeTime = GameStatus.timeUsedMemorizing.ToString(),
                searchTime = GameStatus.timeUsedSearching.ToString(),
                keyImageName = keyItemsNames,
                foundImageName = foundItemsNames
            };

            Dictionary<string, object> resultData = new Dictionary<string, object>
            {
                { "id", newResult.id },
                { "date", newResult.date },
                { "time", newResult.time },
                { "level", newResult.level },
                { "searchTime", newResult.searchTime },
                { "availableSearchTime", GameConfig.searchTime },
                { "memTime", newResult.memorizeTime },
                { "memObjects", newResult.keyImageName },
                { "foundObjects", newResult.foundImageName },
                { "decoyObjects", decoyItemsNames},
                { "logging", Logging.logList }
            };

            // Si es un nuevo paciente, primero aseguramos que tenga idPatient
            patientRef.Child("idPatient").SetValueAsync(patientEmail).ContinueWithOnMainThread(_ =>
            {
                // Guardamos el resultado en /results/{patientNodeKey}/records/{newRecordId}
                recordsRef.Child(newRecordId).SetValueAsync(resultData).ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsFaulted)
                    {
                        Debug.LogError("Error al guardar resultado: " + setTask.Exception);
                    }
                    else
                    {
                        Debug.Log("Resultado guardado correctamente para paciente " + patientEmail);
                        resultUI.ShowResult(newResult);
                    }
                });
            });
        });
    });
    }

    // private async System.Threading.Tasks.Task<string> GetNextRecordId()
    // {
    //     CollectionReference resultsRef = db.Collection("results");
    //     QuerySnapshot snapshot = await resultsRef.GetSnapshotAsync();
    //     //int nextId = snapshot.Count + 1;
    //     //return "record_" + nextId.ToString("D3");
    // //}

}