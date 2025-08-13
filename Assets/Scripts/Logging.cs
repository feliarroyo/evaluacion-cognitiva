using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine;
using Object = System.Object;
using System.IO;

public class Logging : MonoBehaviour
{
    [Serializable]
    public struct LogEvent
    {
        public EventType eventType;
        public string eventName;          // stores readable name
        public string timeStamp;
        public string[] parameters; // string para guardarlo en JSON
        public LogEvent(EventType eventType, Object[] parameters)
        {
            this.eventType = eventType;
            eventName = eventType.ToString(); // store enum as string
            timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.parameters = Array.ConvertAll(parameters, p => p?.ToString());
        }
        public override readonly string ToString()
        {
            return eventType switch
            {
                EventType.PlayerMovementStart => timeStamp + " - Usuario se movió desde: " + parameters[0],
                EventType.PlayerRotationStart => timeStamp + " - Usuario estuvo rotando en: " + parameters[0],
                EventType.ItemGrab => timeStamp + " - Usuario tomó el objeto " + parameters[0],
                EventType.ItemStore => timeStamp + " - Usuario se llevó el objeto " + parameters[0],
                EventType.ItemReturn => timeStamp + " - Usuario dejó el objeto " + parameters[0],
                EventType.PhaseStart => timeStamp + " - Inicio de la fase " + parameters[0],
                EventType.PhaseEnd => timeStamp + " - Fin de la fase " + parameters[0],
                EventType.TimeIsUp => timeStamp + " - Se acabó el tiempo",
                EventType.DifficultySet => timeStamp + " - Inicio de actividad con dificultad " + parameters[0],
                EventType.PlayerMovementEnd => timeStamp + " - Usuario dejó de moverse en: " + parameters[0],
                EventType.PlayerRotationEnd => timeStamp + " - Usuario dejó de rotar en: " + parameters[0],
                EventType.ElementOpen => timeStamp + " - Usuario abrió " + parameters[0],
                EventType.ElementClose => timeStamp + " - Usuario cerró " + parameters[0],
                _ => eventType.ToString(),
            };
        }

        [Serializable]
        public class LogEventList
        {
            public List<LogEvent> events;
            public LogEventList(List<LogEvent> events)
            {
                this.events = events;
            }
        }
        public static string GetJSONLog()
        {
            LogEventList wrapper = new(eventLog);
            return JsonUtility.ToJson(wrapper, true); // `true` for pretty print
        }


        public static void SaveLogToFile(string fileName = "log.json")
        {
            // Create JSON string
            string json = GetJSONLog();

            // Define file path (persistentDataPath is safe for all platforms)
            string path = Path.Combine(Application.persistentDataPath, fileName);

            // Write file
            File.WriteAllText(path, json);

            DebugLog("Log saved to: " + path);
        }
    }
    public enum EventType { PlayerMovementStart, PlayerRotationStart, ItemGrab, ItemStore, ItemReturn, PhaseStart, PhaseEnd, TimeIsUp, DifficultySet, PlayerMovementEnd, PlayerRotationEnd, ElementOpen, ElementClose };
    private static List<LogEvent> eventLog = new List<LogEvent>();
    public static void DebugLog(string msg)
    {
#if UNITY_EDITOR
        Debug.Log(msg);
#endif
    }

    public static string GetStringLog()
    {
        String s = "";
        foreach (LogEvent ev in eventLog)
        {
            s += ev.ToString() + "\n";
        }
        return s;
    }

    public static void Log(EventType ev, Object[] parameters)
    {
        LogEvent newLogEvent = new LogEvent(ev, parameters);
        eventLog.Add(newLogEvent);
        DebugLog(newLogEvent.timeStamp + ": Evento de tipo " + ev + " loggeado, parámetros: " + string.Join(",", parameters));
    }
}