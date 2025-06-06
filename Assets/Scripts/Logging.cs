using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine;
using Object = System.Object;

public class Logging : MonoBehaviour
{  
    struct LogEvent {
        public EventType eventType;
        public DateTime timeStamp;
        public Object[] parameters;
        public LogEvent(Logging.EventType eventType, Object[] parameters){
            this.eventType = eventType;
            timeStamp = DateTime.Now;
            this.parameters = parameters;
        }
        public override readonly string ToString()
            {
            return eventType switch
            {
                EventType.PlayerMovement => timeStamp + " - Posición del usuario: " + parameters[0],
                EventType.PlayerRotation => timeStamp + " - Rotación del usuario: " + parameters[0],
                EventType.ItemGrab => timeStamp + " - Usuario tomó el objeto " + parameters[0],
                EventType.ItemStore => timeStamp + " - Usuario se llevó el objeto " + parameters[0],
                EventType.ItemReturn => timeStamp + " - Usuario dejó el objeto " + parameters[0],
                EventType.PhaseStart => timeStamp + " - Inicio de la fase " + parameters[0],
                EventType.PhaseEnd => timeStamp + " - Fin de la fase " + parameters[0],
                EventType.TimeIsUp => timeStamp + " - Se acabó el tiempo",
                EventType.DifficultySet => timeStamp + " - Inicio de actividad con dificultad " + parameters[0],
                _ => eventType.ToString(),
            };
        }
    }
    public enum EventType {PlayerMovement, PlayerRotation, ItemGrab, ItemStore, ItemReturn, PhaseStart, PhaseEnd, TimeIsUp, DifficultySet};
    private static List<LogEvent> eventLog = new List<LogEvent>();
    public static void DebugLog(string msg){
        #if UNITY_EDITOR
            Debug.Log(msg);
        #endif
    }

    public static string GetStringLog(){
        String s = "";
        foreach (LogEvent ev in eventLog){
            s += ev.ToString() + "\n";
        }
        return s;
    }

    public static void Log(EventType ev, Object[] parameters) {
        LogEvent newLogEvent = new LogEvent(ev, parameters);
        eventLog.Add(newLogEvent);
        DebugLog(newLogEvent.timeStamp + ": Evento de tipo " + ev + " loggeado, parámetros: " + String.Join(",", parameters));
    }
}