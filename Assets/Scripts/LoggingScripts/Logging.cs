using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;
using System.IO;
using System.Linq;
using TMPro;

public class Logging : MonoBehaviour
{
    public class LogEvent
    {
        public string timeStamp;
        public string gamePhase; // (E = Espera, M = Memorizar, FM = Fin Memorizar, B = Búsqueda, FB = Fin Búsqueda)
        public string movementStatus; // (Q = Quieto, M = En movimiento, G = Giro)
        public Vector2 movementPosition; // (Posicion en el espacio del usuario)
        public Vector2 movementDirection; // (Direccion del movimiento del usuario)
        public string cameraStatus; // (Q = Quieto o M = En movimiento)
        public Vector2 cameraOrientation; // (Ángulo H o V)
        public Vector2 cameraMovementDirection; // (Vector de dirección)
        public Dictionary<string, SeenItem> seenItems;
        public string heldItem; // nombre del objeto sostenido
        public string heldItemStatus; // S = sostenido, E = elegido, R = retornado
        public Dictionary<string, SeenFurniture> seenFurniture;

        public void SetTimeStamp()
        {
            timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public LogEvent(LogEvent other)
        {
            timeStamp = other.timeStamp;
            gamePhase = other.gamePhase; // (E = Espera, M = Memorizar, FM = Fin Memorizar, B = Búsqueda, FB = Fin Búsqueda)
            movementStatus = other.movementStatus; // (Q = Quieto, M = En movimiento, G = Giro)
            movementPosition = other.movementPosition;
            movementDirection = other.movementDirection;
            cameraStatus = other.cameraStatus; // (Q = Quieto o M = En movimiento)
            cameraOrientation = other.cameraOrientation; // (Contexto: Ángulo H o V -si Q- o Vector de dirección -si M-)
            cameraMovementDirection = other.cameraMovementDirection;
            seenItems = other.seenItems;
            heldItem = other.heldItem; // nombre del objeto sostenido
            heldItemStatus = other.heldItemStatus; // S = sostenido, E = elegido, R = retornado
            seenFurniture = other.seenFurniture;
        }

        public LogEvent()
        {
        }

        public class SeenItem
        {
            public string objectName;
            public string itemType;
            public bool isInteractable;
            public float distance;
            public Vector2 screenPosition;
            public override string ToString()
            {
                return "<" + objectName + ", " + itemType + ", INTERACTUABLE: " + isInteractable + ", DISTANCIA: " + distance + " POSICIÓN EN PANTALLA: " + screenPosition + ">";
            }
        }

        public class SeenFurniture
        {
            public string objectName;
            public bool isInteractable;
            public bool isOpen;
            public float distance;
            public Vector2 screenPosition;
            public override string ToString()
            {
                return "<NOMBRE: " + objectName + "\nINTERACTUABLE: " + isInteractable + "\nABIERTO: " + isOpen + "\nDISTANCIA: " + distance + "\nPOSICIÓN EN PANTALLA: " + screenPosition + ">";
            }
        }
        public override string ToString()
        {
            string result = "TIEMPO: " + timeStamp
            + "\nFASE: " + gamePhase
            + "\nESTADO FÍSICO: " + movementStatus
            + "\nPOSICIÓN: " + movementPosition
            + "\nDIRECCIÓN DE MOVIMIENTO: " + movementDirection
            + "\nESTADO DE CÁMARA: " + cameraStatus
            + "\nORIENTACIÓN DE CÁMARA: " + cameraOrientation
            + "\nDIRECCIÓN DE MOVIMIENTO: " + cameraMovementDirection
            + "\nOBJETOS VISIBLES: { \n";
            if (seenItems != null)
            {
                foreach (string item in seenItems.Keys)
                {
                    result += seenItems[item].ToString() + "\n";
                }
            }
            else result += "- }";
            result += "\nOBJETO EN MANO: " + heldItem
            + " (" + heldItemStatus
            + ")\nMUEBLES VISIBLES: { \n";
            if (seenFurniture != null)
            {
                foreach (string furniture in seenFurniture.Keys)
                {
                    result += seenFurniture[furniture].ToString() + "\n";
                }
            }
            else result += "-";
            return result + " }";
        }

        public static void SaveLogToFile(string fileName = "log.json")
        {
            // Define file path (persistentDataPath is safe for all platforms)
            string path = Path.Combine(Application.persistentDataPath, fileName);

            // Write file
            File.WriteAllText(path, logList);

            DebugLog("Log saved to: " + path);
        }
    }


    public enum EventType
    {
        PlayerMovementStart,
        PlayerMovementChange,
        PlayerMovementEnd,
        PlayerRotationStart,
        PlayerRotationChange,
        PlayerRotationEnd,
        ItemGrab,
        ItemStore,
        ItemReturn,
        PhaseChange,
        ElementOpen,
        ElementClose,
        ElementSeen,
        ElementUnseen,
        SeenElementsChange,
        ObjectUnseen,
        SeenObjectInteractivityChange,
        NoItem
    };

    public static LogEvent currentLog = new();

    public static string logList = "";
    public static List<string> itemsInLevel;
    public static List<Interactable> furnitureSeen;
    public static List<HeldItem> itemsSeen;
    public static void DebugLog(string msg)
    {
#if UNITY_EDITOR
        Debug.Log(msg);
#endif
    }

    void Start()
    {

        currentLog = new()
        {
            gamePhase = "E",
            movementStatus = "Q",
            movementPosition = new(PlayerMovement.currentPosition.x, PlayerMovement.currentPosition.z), // Get starting position from some script
            movementDirection = new(0, 0),
            cameraStatus = "Q",
            cameraOrientation = new(0, 0),
            cameraMovementDirection = new(0, 0),
            seenItems = new(),
            seenFurniture = new(),
            heldItem = "-", // nombre del objeto sostenido
            heldItemStatus = "-", // S = sostenido, E = elegido, R = retornado

        };
        currentLog.SetTimeStamp();
        itemsSeen = new();
        furnitureSeen = new();
        logList = currentLog.ToString();
        GameObject.Find("Status").GetComponent<TextMeshProUGUI>().text = currentLog.ToString();
        InvokeRepeating(nameof(CheckVisibility), 0f, 0.5f);
    }

    private static bool CalculateItemStatus(HeldItem hi)
    {
        bool interactChange = false;
        if (currentLog.seenItems[hi.itemName].isInteractable != hi.interactable.isInteractable)
        {
            interactChange = true;
        }
        currentLog.seenItems[hi.itemName].isInteractable = hi.interactable.isInteractable;
        currentLog.seenItems[hi.itemName].distance = Vector3.Distance(PlayerMovement.currentPosition, hi.originalPosition);
        currentLog.seenItems[hi.itemName].screenPosition = Camera.main.WorldToViewportPoint(hi.originalPosition);
        return interactChange;
    }

    private static bool CalculateFurnitureStatus(Interactable i)
    {
        bool interactChange = false;
        string name = i.GetID();
        bool isOpen = i.GetComponent<OpenDrawer>() != null ? i.GetComponent<OpenDrawer>().isOpen : i.GetComponent<OpenDoor>().isOpen;
        if ((currentLog.seenFurniture[name].isInteractable != i.isInteractable) || isOpen)
        {
            interactChange = true;
        }
        currentLog.seenFurniture[name].isInteractable = i.isInteractable;
        currentLog.seenFurniture[name].isOpen = isOpen;
        currentLog.seenFurniture[name].distance = Vector3.Distance(PlayerMovement.currentPosition, i.transform.position);
        currentLog.seenFurniture[name].screenPosition = Camera.main.WorldToViewportPoint(i.transform.position);
        return interactChange;
    }

    private void CheckVisibility()
    {
        bool changesDetected = false;
        foreach (HeldItem hi in HeldItem.itemsInScene)
        {
            if (hi.CheckVisibility())
            {
                if (!itemsSeen.Contains(hi)) // new item added to visible objects
                {
                    itemsSeen.Add(hi);
                    currentLog.seenItems.Add(hi.itemName, new()
                    {
                        objectName = hi.itemName,
                        itemType = hi.isEnvironmentItem ?
                            (hi.isKeyItem ? "A MEMORIZAR" : "DISTRACTOR") : "HALL"
                    });
                }
                if (CalculateItemStatus(hi)) // check changes in visible item info
                    changesDetected = true;
            }
            else if (!hi.CheckVisibility() && itemsSeen.Contains(hi)) // unseen object removed
            {
                itemsSeen.Remove(hi);
                currentLog.seenItems.Remove(hi.itemName);
                changesDetected = true;
            }
        }
        foreach (Interactable i in Interactable.interactablesInScene)
        {
            if (i.CheckVisibility())
            {
                if (!furnitureSeen.Contains(i))
                {
                    furnitureSeen.Add(i);
                    Debug.Log("NUEVO FURNITURE: " + furnitureSeen);
                    currentLog.seenFurniture.Add(i.GetID(), new()
                    {
                        objectName = i.GetID(),
                        isInteractable = i.isInteractable,
                        isOpen = i.GetComponent<OpenDrawer>() != null ? i.GetComponent<OpenDrawer>().isOpen : i.GetComponent<OpenDoor>().isOpen,
                        distance = Vector3.Distance(PlayerMovement.currentPosition, i.transform.position),
                        screenPosition = Camera.main.WorldToViewportPoint(i.transform.position)
                    });
                }
                if (CalculateFurnitureStatus(i)) // check changes in visible furniture info
                    changesDetected = true;
            }
            else if (!i.CheckVisibility() && furnitureSeen.Contains(i))
            {
                furnitureSeen.Remove(i);
                currentLog.seenFurniture.Remove(i.GetID());
                changesDetected = true;
            }
        }
        if (changesDetected)
        {
            Log(EventType.SeenElementsChange);
        }
    }

    public static void Log(EventType ev, Object[] parameters = null)
    {
        //return; // for builds until Logging is complete
        switch (ev)
        {
            case EventType.PlayerMovementStart:
                currentLog.movementStatus = "M";
                Vector3 pos = (Vector3)parameters[0];
                Vector3 mov = (Vector3)parameters[1];
                currentLog.movementPosition = new(pos.x, pos.z);
                currentLog.movementDirection = new(mov.x, mov.y);
                break;
            case EventType.PlayerMovementChange:
                currentLog.movementStatus = "M";
                pos = (Vector3)parameters[0];
                mov = (Vector3)parameters[1];
                currentLog.movementPosition = new(pos.x, pos.z);
                currentLog.movementDirection = new(mov.x, mov.y);
                break;
            case EventType.PlayerMovementEnd:
                currentLog.movementStatus = "Q";
                pos = (Vector3)parameters[0];
                currentLog.movementPosition = new(pos.x, pos.z);
                currentLog.movementDirection = new(0, 0);
                break;
            case EventType.PlayerRotationStart:
                currentLog.cameraStatus = "M";
                currentLog.cameraOrientation = (Vector2)parameters[0];
                currentLog.cameraMovementDirection = (Vector2)parameters[0];
                break;
            case EventType.PlayerRotationChange:
                currentLog.cameraStatus = "M";
                currentLog.cameraOrientation = (Vector2)parameters[0];
                currentLog.cameraMovementDirection = (Vector2)parameters[0];
                break;
            case EventType.PlayerRotationEnd:
                currentLog.cameraStatus = "Q";
                currentLog.cameraOrientation = (Vector2)parameters[0];
                currentLog.cameraMovementDirection = new(0, 0);
                break;
            case EventType.ItemGrab: // hay item grab tanto en Item como en HeldItem, chequear luego
                currentLog.heldItem = (string)parameters[0];
                currentLog.heldItemStatus = "T";
                break;
            case EventType.ItemStore:
                currentLog.heldItemStatus = "E";
                break;
            case EventType.ItemReturn:
                currentLog.heldItemStatus = "S";
                break;
            case EventType.NoItem:
                currentLog.heldItem = "-";
                currentLog.heldItemStatus = "-";
                break;
            case EventType.PhaseChange:
                currentLog.gamePhase = (string)parameters[0];
                break;
            case EventType.SeenElementsChange: // objetos y furniture
                // se agregaron a currentLog aparte, es solo actualizarlo
                break;
            case EventType.SeenObjectInteractivityChange:
                currentLog.seenItems[(string)parameters[0]].isInteractable = !currentLog.seenItems[(string)parameters[0]].isInteractable;
                break;
            case EventType.ObjectUnseen:
                // currentLog.seenItems.Remove((string)parameters[0]);
                break;
            case EventType.ElementSeen:
                // currentLog.seenFurniture.Add((string)parameters[0], new LogEvent.SeenFurniture
                // {
                //     objectName = (string)parameters[0],
                //     isInteractable = (bool)parameters[1],
                //     isOpen = (bool)parameters[2],
                //     distance = (float)parameters[3],
                //     screenPosition = (Vector2)parameters[4]
                // });
                break;
            case EventType.ElementUnseen:
                // currentLog.seenFurniture.Remove((string)parameters[0]);
                break;
            case EventType.ElementOpen:
                // currentLog.seenFurniture[(string)parameters[0]].isOpen = true;
                break;
            case EventType.ElementClose:
                // currentLog.seenFurniture[(string)parameters[0]].isOpen = false;
                break;
        }
        currentLog.SetTimeStamp();
        LogEvent newLog = new(currentLog);
        Debug.Log(newLog.ToString());
        GameObject.Find("Status").GetComponent<TextMeshProUGUI>().text = newLog.ToString();
        logList += newLog.ToString();
    }

    public static void ItemInfoLog(string itemName, bool isEnvironmentItem, string spawnName, float spawnPositionX, float spawnPositionY)
    {
        string itemInfo = "TIEMPO: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "INFORMACIÓN DE OBJETO: <" + itemName + ", TIPO: ";
        itemInfo += isEnvironmentItem ? "LIVING" : "HALL";
        itemInfo += ", SPAWN: " + spawnName + " (" + spawnPositionX + ", " + spawnPositionY + ")>";
        logList += itemInfo;
    }
}