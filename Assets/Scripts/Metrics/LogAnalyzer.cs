using System.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;


/// <summary>
/// LogAnalyzer is responsible for extracting metrics and events from the raw log string.
/// </summary>
public static class LogAnalyzer
{
    private static string logFilePath => Path.Combine(Application.persistentDataPath, "log.txt");

    /// <summary>
    /// Reads the log file contents.
    /// </summary>
    private static string ReadLogFile()
    {
        if (!File.Exists(logFilePath))
        {
            Debug.LogWarning("Log file not found: " + logFilePath);
            return "";
        }
        return File.ReadAllText(logFilePath);
    }

    private static string[] GetLogLines()
    {
        string content = ReadLogFile();
        return content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private static List<string[]> GetRows()
    {
        var lines = GetLogLines();
        return lines
            .Where(l => Regex.IsMatch(l, @"^\d{2}-\d{2}-\d{4};\d{2}:\d{2}:\d{2}"))
            .Select(l => l.Split(';'))
            .ToList();
    }

    /// <summary>
    /// Calculates the time spent in phase FM (intermediate time). Tiempo que le lleva al usuario caminar desde el hall al living.
    /// </summary>
    public static float GetIntermediateTime()
    {

        var rows = GetRows();

        DateTime? fmStart = null;
        DateTime? fmEnd = null;

        foreach (var row in rows)
        {
            string date = row[0].Trim();
            string time = row[1].Trim();
            string phase = row[2].Trim();

            DateTime timestamp;
            try
            {
                timestamp = DateTime.ParseExact(
                    $"{date} {time}",
                    "dd-MM-yyyy HH:mm:ss",
                    CultureInfo.InvariantCulture
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parseando fecha/hora: {date} {time}. Exception: {e.Message}");
                continue;
            }

            if (phase == "FM" && fmStart == null)
            {
                fmStart = timestamp;
            }

            if (fmStart.HasValue && phase == "B")
            {
                fmEnd = timestamp;
                break;
            }
        }

        if (fmStart.HasValue && fmEnd.HasValue)
        {
            float duration = (float)(fmEnd.Value - fmStart.Value).TotalSeconds;
            return duration;
        }
        else
        {
            return 0f;
        }
    }


    /// <summary>
    /// Represents a memory phase object.
    /// Medición del tiempo que ocupa el usuario en mirar cada objeto detalladamente en la fase de memorización.
    /// </summary>
    public class MemViewObject
    {
        public string ObjectName;
        public string StartTime;
        public string EndTime;
        public string TotalDuration;
    }

    public static List<MemViewObject> GetMemViewObjects()
    {

        var rows = GetRows();

        var eventsList = new List<(string obj, string time, string state)>();

        int rowIndex = 0;
        foreach (var row in rows)
        {
            rowIndex++;
            string phase = row[2].Trim();
            string time = row[1].Trim();
            string heldObject = row.Length > 10 ? row[10].Trim() : null;
            string objectState = row.Length > 11 ? row[11].Trim() : null;

            if (phase == "M" && !string.IsNullOrEmpty(heldObject) && heldObject != "-")
            {
                eventsList.Add((heldObject, time, objectState));
            }
        }

        var results = new List<MemViewObject>();
        var activeObjects = new Dictionary<string, string>();

        int evIndex = 0;
        foreach (var ev in eventsList)
        {
            evIndex++;
            if (ev.state == "T")
            {
                if (!activeObjects.ContainsKey(ev.obj))
                {
                    activeObjects[ev.obj] = ev.time;
                }
                else
                {
                    Debug.Log($"[GetMemViewObjects] -> IGNORADO START duplicado para {ev.obj} en {ev.time} (ya iniciado en {activeObjects[ev.obj]})");
                }
                continue;
            }

            if (ev.state == "S")
            {
                if (!activeObjects.ContainsKey(ev.obj))
                {
                    Debug.Log($"[GetMemViewObjects] -> FIN (S) para {ev.obj} en {ev.time} pero NO hay START registrado -> IGNORAR");
                    continue;
                }

                string start = activeObjects[ev.obj];
                string end = ev.time;
                string totalDuration;
                try
                {
                    totalDuration = CalculateTimeDifference(start, end);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GetMemViewObjects] ERROR calculando duración para {ev.obj} (start='{start}', end='{end}'): {ex}");
                    activeObjects.Remove(ev.obj);
                    continue;
                }

                results.Add(new MemViewObject
                {
                    ObjectName = ev.obj,
                    StartTime = start,
                    EndTime = end,
                    TotalDuration = totalDuration
                });
                activeObjects.Remove(ev.obj);
            }
        }
        return results;
    }

    /// <summary>
    /// Represents a search phase object.
    /// Medición del tiempo que ocupa el usuario en mirar cada objeto detalladamente en la fase de búsqueda detallando si el objeto termina siendo elegido o no.
    /// </summary>
    public class SearchViewObject
    {
        public string ObjectName;
        public string StartTime;
        public string EndTime;
        public string TotalDuration;
        public string Selected; // YES if ended with E, NO if S
    }

    public static List<SearchViewObject> GetSearchViewObjects()
    {

        var rows = GetRows();

        var eventsList = new List<(string obj, string time, string state)>();
        int rowIndex = 0;

        foreach (var row in rows)
        {
            rowIndex++;

            string phase = row[2].Trim();
            string time = row[1].Trim();
            string heldObject = row.Length > 10 ? row[10].Trim() : null;
            string objectState = row.Length > 11 ? row[11].Trim() : null;

            if (phase == "B" && !string.IsNullOrEmpty(heldObject) && heldObject != "-")
            {
                eventsList.Add((heldObject, time, objectState));
            }
        }

        var results = new List<SearchViewObject>();
        var activeObjects = new Dictionary<string, string>();

        foreach (var ev in eventsList)
        {
            if (ev.state == "T")
            {
                if (!activeObjects.ContainsKey(ev.obj))
                {
                    activeObjects[ev.obj] = ev.time;
                }
                continue;
            }

            if ((ev.state == "S" || ev.state == "E") && activeObjects.ContainsKey(ev.obj))
            {
                string start = activeObjects[ev.obj];
                string end = ev.time;
                string totalDuration = CalculateTimeDifference(start, end);
                string selected = ev.state == "E" ? "YES" : "NO";

                results.Add(new SearchViewObject
                {
                    ObjectName = ev.obj,
                    StartTime = start,
                    EndTime = end,
                    TotalDuration = totalDuration,
                    Selected = selected
                });
                activeObjects.Remove(ev.obj);
            }
        }
        return results;
    }


    /// <summary>
    /// Represents a search phase object.
    /// Medición del tiempo que ocupa el usuario desde que inicia la fase de búsqueda hasta que agarra cada objeto.
    /// </summary>
    public class SearchSelectionTime
    {
        public string ObjectName;
        public string SearchStart;   // time when phase B started or previous selection
        public string SelectionTime; // time when object was chosen (state E)
        public string TotalDuration; // hh:mm:ss difference between start and selection
    }

    public static List<SearchSelectionTime> GetSearchSelectionTimes()
    {

        var rows = GetRows();

        var firstBRow = rows.FirstOrDefault(r => r.Length > 2 && r[2].Trim() == "B");
        if (firstBRow == null)
        {
            return new List<SearchSelectionTime>();
        }

        string searchStartTime = firstBRow[1].Trim();

        var chosenRows = rows
            .Where(r => r.Length > 11 && r[2].Trim() == "B" && r[11].Trim() == "E" && r[10].Trim() != "-")
            .Select(r => new { Obj = r[10].Trim(), Time = r[1].Trim() })
            .ToList();

        var results = new List<SearchSelectionTime>();

        foreach (var chosen in chosenRows)
        {
            string totalDuration = CalculateTimeDifference(searchStartTime, chosen.Time);

            results.Add(new SearchSelectionTime
            {
                ObjectName = chosen.Obj,
                SearchStart = searchStartTime,
                SelectionTime = chosen.Time,
                TotalDuration = totalDuration
            });
        }

        return results;
    }


    /// <summary>
    /// Represents a search phase object.
    /// Medición del tiempo que ocupa el usuario en mirar cada objeto detalladamente desde que dejo el objeto anterior hasta que miro detalladamente el objeto actual.
    /// </summary>
    public class SearchChoiceInterval
    {
        public string PreviousObject;
        public string PreviousSelectionTime;
        public string CurrentObject;
        public string CurrentSelectionTime;
        public string TotalDuration;
    }

    public static List<SearchChoiceInterval> GetSearchChoiceIntervals()
    {

        var rows = GetRows();

        var firstBRow = rows.FirstOrDefault(r => r.Length > 2 && r[2].Trim() == "B");
        if (firstBRow == null)
        {
            return new List<SearchChoiceInterval>();
        }

        string searchStartTime = firstBRow[1].Trim();

        var chosenRows = rows
            .Where(r => r.Length > 11 && r[2].Trim() == "B" && r[11].Trim() == "E" && r[10].Trim() != "-")
            .Select(r => new { Obj = r[10].Trim(), Time = r[1].Trim() })
            .ToList();

        var results = new List<SearchChoiceInterval>();

        string previousObject = "-";
        string previousSelectionTime = searchStartTime;

        int index = 0;
        foreach (var chosen in chosenRows)
        {
            index++;
            string totalDuration = CalculateTimeDifference(previousSelectionTime, chosen.Time);

            results.Add(new SearchChoiceInterval
            {
                PreviousObject = previousObject,
                PreviousSelectionTime = previousSelectionTime,
                CurrentObject = chosen.Obj,
                CurrentSelectionTime = chosen.Time,
                TotalDuration = totalDuration
            });

            previousObject = chosen.Obj;
            previousSelectionTime = chosen.Time;
        }

        return results;
    }


    /// <summary>
    /// Calculates the difference between two times in format HH:mm:ss.
    /// </summary>
    private static string CalculateTimeDifference(string start, string end)
    {
        string[] timeFormats = new[] { "HH:mm:ss", "H:mm:ss", "hh:mm:ss", "h:mm:ss" };

        TimeSpan t1, t2;
        bool ok1 = TimeSpan.TryParseExact(start, new[] { @"hh\:mm\:ss", @"h\:mm\:ss", @"HH\:mm\:ss", @"H\:mm\:ss" }, CultureInfo.InvariantCulture, out t1);
        if (!ok1)
            ok1 = TimeSpan.TryParse(start, out t1);

        bool ok2 = TimeSpan.TryParseExact(end, new[] { @"hh\:mm\:ss", @"h\:mm\:ss", @"HH\:mm\:ss", @"H\:mm\:ss" }, CultureInfo.InvariantCulture, out t2);
        if (!ok2)
            ok2 = TimeSpan.TryParse(end, out t2);

        if (!ok1 || !ok2)
        {
            throw new FormatException($"CalculateTimeDifference: formato inválido start='{start}' ok1={ok1} end='{end}' ok2={ok2}");
        }

        if (t2 < t1) t2 = t2.Add(TimeSpan.FromHours(24));
        TimeSpan diff = t2 - t1;
        return diff.ToString(@"hh\:mm\:ss");
    }
}
