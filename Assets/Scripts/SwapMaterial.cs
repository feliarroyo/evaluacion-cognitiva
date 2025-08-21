using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Monetization;

public class SwapMaterial : MonoBehaviour
{
    public int replaceID; // which material to replace
    public Material unlitMaterial;
    public Material litMaterial;
    private static List<SwapMaterial> swappedMaterials = new();
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Renderer>().material = unlitMaterial;
        swappedMaterials.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetMaterial(bool isLit){
        gameObject.GetComponent<Renderer>().materials[replaceID] = isLit? litMaterial : unlitMaterial;
    }

    public static void SetMaterials(bool isLit){
        foreach (SwapMaterial sm in swappedMaterials){
            sm.SetMaterial(isLit);
        }
    }

    void OnDestroy()
    {
        swappedMaterials.Remove(this);
    }
}
