using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PatientData
{
    public string email = "";
    public List<LevelConfig> levels = new List<LevelConfig>();

    public void SetEmail(string userEmail)
    {
        email = userEmail;
    }
}

[System.Serializable]
public class LevelConfig{
    public string id;
    public int memorizeTime;
    public int searchTime;
    public List<SpawnInfo> searchObjects;
    public List<SpawnInfo> distractingObjects;
}

[System.Serializable]
public class SpawnInfo{
    public string key;
    public string item;

    public SpawnInfo(string key, string stringItems)
    {
        this.key = key;
        this.item = stringItems;
    }
}
