using System;
using System.Collections.Generic;

[Serializable]
public class HistoryData
{
    public List<ResultData> results = new List<ResultData>();
}

[Serializable]
public class ResultData
{
    public int id;
    public string date;
    public string time;
    public string level;
    public string memorizeTime;
    public string searchTime;
    public List<string> keyImageName;
    public List<string> foundImageName;
}
