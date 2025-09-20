using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawn : MonoBehaviour
{
    public string spawnName;
    public bool allowLargeItems; // whether the spawn area is big enough to allocate big items
    public bool isLevel1;
    public bool isLevel2;
    public ItemSpawning.SpawnType spawnType;
    public string description;
    public int logId;
    
    void OnDrawGizmos()
    {
        // Choose different colors for big and small spawn points
        Gizmos.color = allowLargeItems ? Color.red : Color.green;

        // Draw a sphere to indicate spawn points
        Gizmos.DrawWireSphere(transform.position, allowLargeItems ? 0.3f : 0.1f);
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected(){
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.2f, allowLargeItems ? "Big" : "Small");
    }
    #endif
    
    // Start is called before the first frame update    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}