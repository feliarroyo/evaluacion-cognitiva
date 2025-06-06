using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HistoryUIManager : MonoBehaviour
{
    public Transform resultContent;

    public GameObject resultPrefab;

    public void GenerateHistorialListUI( List<ResultData> historyResults){
        foreach (var result in historyResults)
            {
                GameObject newResult = Instantiate(resultPrefab, resultContent);
                newResult.transform.Find("ResultBox/DateResult").GetComponent<TextMeshProUGUI>().text = result.date;
                newResult.transform.Find("ResultBox/TimeResult").GetComponent<TextMeshProUGUI>().text = result.time;
                newResult.transform.Find("ResultBox/LevelResult").GetComponent<TextMeshProUGUI>().text = result.level;

                Button button = newResult.transform.Find("ResultBox/ViewResult").GetComponent<Button>();
                button.onClick.AddListener(() => ResultsController.Instance.OnResultSelected(result.id));
            }
    }

    public void ClearHistoryUI()
    {
        if (resultContent == null) return;

        foreach (Transform child in resultContent)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
