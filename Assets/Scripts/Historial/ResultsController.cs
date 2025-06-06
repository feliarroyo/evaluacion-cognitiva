using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultsController : MonoBehaviour
{

    public static ResultsController Instance;
    
    public HistoryUIManager historyUI;
    public ResultUIManager resultUI;
    public GameObject historyPanel;
    public GameObject resultPanel;
    private List<ResultData> historyResults;
    private int lastId;
    private bool userSelectedID = false;
    private void Awake()
        {
            Instance = this;
        }

    private void Start()
    {
        HistoryLoader.OnHistoryUpdated += UpdateHistoryResults; // Suscribirse al evento
        UpdateHistoryResults();
        // historyResults = HistoryLoader.Instance.historyResults;
        // if (historyResults.Count > 0)
        // {
        //     lastId = historyResults[historyResults.Count - 1].id;
        // }
        // historyUI.GenerateHistorialListUI(historyResults);
    }

    private void UpdateHistoryResults()
    {
        historyResults = new List<ResultData>(HistoryLoader.Instance.historyResults); // Clonar la lista
        historyUI.ClearHistoryUI();
        historyUI.GenerateHistorialListUI(historyResults);
    }

    public void OnResultSelected(int resultId)
    {
        userSelectedID = true;
        ShowResult(resultId);
    }

    private void ShowResult(int id)
    {
        ResultData result = historyResults.Find(r => r.id == id);
        if (result != null)
        {
            resultUI.ShowResult(result);
            historyPanel.SetActive(false);
            resultPanel.SetActive(true);
        }
    }

}
