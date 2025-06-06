using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatToStringWrite : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Write the TMProUGUI component's text with the number passed as parameter.
    /// </summary>
    /// <param name="number">Float number.</param>
    public void WriteNumberAsString(float number){
        GetComponent<TextMeshProUGUI>().text = number.ToString("F2");
    }
}
