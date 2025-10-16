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
        SetMaterial(false);
        swappedMaterials.Add(this);
    }

    public void SetMaterial(bool isLit)
    {
        Renderer rend = gameObject.GetComponent<Renderer>();
        Material[] mats = rend.materials;
        mats[replaceID] = isLit ? litMaterial : unlitMaterial;
        rend.materials = mats;
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
