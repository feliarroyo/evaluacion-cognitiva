using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleShaders : MonoBehaviour
{
    List<Material> itemMaterials = new(); // List of every material in the item.
    Shader shader;
    public static List<ToggleShaders> itemList = new();
    
    // Start is called before the first frame update
    void Start()
    {
        shader = Shader.Find("Universal Render Pipeline/Unlit");
        // Conseguir todos los materiales del objeto
        foreach (Renderer r in GetComponentsInChildren<Renderer>()){
            itemMaterials.AddRange(r.materials);
        }
        itemList.Add(this);
    }

    public static void ChangeShadersToAll(){
        foreach (ToggleShaders ts in itemList){
            ts.ChangeShaders();
        }
    }

    // Changes shader to unlit
    void ChangeShaders()
    {
        foreach (Material m in itemMaterials){
            m.shader = shader;
        }
    }
}