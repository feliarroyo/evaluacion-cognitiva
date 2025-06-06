using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WritePhase : MonoBehaviour
{
    TextMeshProUGUI phaseText;
    // Start is called before the first frame update
    void Start()
    {
        phaseText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        phaseText.text = "Fase: " + GameStatus.currentPhase;
    }
}
