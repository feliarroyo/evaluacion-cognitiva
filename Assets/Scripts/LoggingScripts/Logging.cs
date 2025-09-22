using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;
using System.IO;
using System.Linq;
using TMPro;
using System.Globalization;

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
        public Dictionary<int, SeenItem> seenItems;
        public string heldItem; // nombre del objeto sostenido
        public string heldItemStatus; // S = sostenido, E = elegido, R = retornado
        public Dictionary<int, SeenFurniture> seenFurniture;

        public void SetTimeStamp()
        {
            timeStamp = DateTime.Now.ToString("dd-MM-yyyy;HH:mm:ss");
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
            public int id;
            public string objectName;
            public string itemType;
            public bool isInteractable;
            public float distance;
            public Vector2 screenPosition;
            public override string ToString()
            {
                return "<" + id + ":" + itemType + "|" + (isInteractable ? "I" : "NI") + "|" + distance.ToString(CultureInfo.InvariantCulture) + "|" + screenPosition + ">";
            }
        }

        public class SeenFurniture
        {
            public int id;
            public string objectName;
            public bool isInteractable;
            public bool isOpen;
            public float distance;
            public Vector2 screenPosition;
            public override string ToString()
            {
                return "<" + id + ":" + (isInteractable ? "I" : "NI") + "|" + (isOpen ? "A" : "C") + "|" + distance.ToString(CultureInfo.InvariantCulture) + "|" + screenPosition + ">";
            }
        }
        public override string ToString()
        {
            string result = timeStamp
            + ";" + gamePhase
            + ";" + movementStatus
            + ";" + movementPosition
            + ";" + movementDirection
            + ";" + cameraStatus
            + ";" + cameraOrientation
            + ";" + cameraMovementDirection
            + ";{";
            if (seenItems != null && seenItems.Count > 0)
            {
                foreach (int item in seenItems.Keys)
                {
                    result += seenItems[item].ToString() + ", ";
                }
                result = result.Substring(0, result.Length - 2) + "}";
            }
            else result += "-}";
            result += ";" + heldItem
            + ";" + heldItemStatus
            + ";{";
            if (seenFurniture != null && seenFurniture.Count > 0)
            {
                foreach (int furniture in seenFurniture.Keys)
                {
                    result += seenFurniture[furniture].ToString() + ", ";
                }
                result = result.Substring(0, result.Length - 2);
            }
            else result += "-";
            return result + "}\n";
        }

        public static string GetLog(string fileName = "log.txt")
        {
            // Define file path (persistentDataPath is safe for all platforms)
            string path = Path.Combine(Application.persistentDataPath, fileName);
            string content = "ID;OBJETO;TIPO;ID SPAWN;POSICION" + "\n"
                + extraItemInfo + "\n"
                + "ID;SPAWN;DESCRIPCION" + "\n"
                + extraSpawnInfo + "\n"
                + "ID;INTERACTUABLE;POSICION" + "\n"
                + extraFurnitureInfo + "\n"
                + "FECHA;TIEMPO;FASE;ESTADO MOVIMIENTO;POSICION;DIRECCIÓN MOVIMIENTO;ESTADO CAMARA;ORIENTACION CAMARA;INTENSIDAD MOV. DE CAMARA;OBJETOS VISIBLES;OBJETO SOSTENIDO;ESTADO OBJ SOSTENIDO;INTERACTUABLES VISIBLES\n"
                + logList;
            // Write file
            File.WriteAllText(path, content);
            logList = "";
            extraItemInfo = "";
            extraSpawnInfo = "";
            extraFurnitureInfo = "";
            DebugLog("Log saved to: " + path);
            return content;
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
    public static string extraItemInfo = "";
    public static string extraFurnitureInfo = "";
    public static string extraSpawnInfo = "";
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
            movementPosition = new(-53.37f, -120),
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
        //GameObject.Find("Status").GetComponent<TextMeshProUGUI>().text = currentLog.ToString();
        InvokeRepeating(nameof(CheckVisibility), 0f, 0.5f);
    }

    private static bool CalculateItemStatus(HeldItem hi)
    {
        int id = hi.logId;
        bool interactChange = false;
        if (currentLog.seenItems[id].isInteractable != hi.interactable.isInteractable)
        {
            interactChange = true;
        }
        currentLog.seenItems[id].isInteractable = hi.interactable.isInteractable;
        currentLog.seenItems[id].distance = (float)Math.Round(Vector3.Distance(PlayerMovement.currentPosition, hi.originalPosition), 2);
        currentLog.seenItems[id].screenPosition = Camera.main.WorldToViewportPoint(hi.originalPosition);
        return interactChange;
    }

    private static bool CalculateFurnitureStatus(Interactable i)
    {
        bool interactChange = false;
        int id = i.id;
        bool isOpen = i.GetComponent<OpenDrawer>() != null ? i.GetComponent<OpenDrawer>().isOpen : i.GetComponent<OpenDoor>().isOpen;
        if ((currentLog.seenFurniture[id].isInteractable != i.isInteractable) || isOpen)
        {
            interactChange = true;
        }
        currentLog.seenFurniture[id].isInteractable = i.isInteractable;
        currentLog.seenFurniture[id].isOpen = isOpen;
        currentLog.seenFurniture[id].distance = (float)Math.Round(Vector3.Distance(PlayerMovement.currentPosition, i.transform.position), 2);
        currentLog.seenFurniture[id].screenPosition = Camera.main.WorldToViewportPoint(i.transform.position);
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
                    currentLog.seenItems.Add(hi.logId, new()
                    {
                        id = hi.logId,
                        objectName = hi.itemName,
                        itemType = hi.isEnvironmentItem ?
                            (hi.isKeyItem ? "M" : "D") : "H"
                    });
                }
                if (CalculateItemStatus(hi)) // check changes in visible item info
                    changesDetected = true;
            }
            else if (!hi.CheckVisibility() && itemsSeen.Contains(hi)) // unseen object removed
            {
                itemsSeen.Remove(hi);
                currentLog.seenItems.Remove(hi.logId);
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
                    currentLog.seenFurniture.Add(i.id, new()
                    {
                        id = i.id,
                        objectName = i.GetName(),
                    });
                }
                if (CalculateFurnitureStatus(i)) // check changes in visible furniture info
                    changesDetected = true;
            }
            else if (!i.CheckVisibility() && furnitureSeen.Contains(i))
            {
                furnitureSeen.Remove(i);
                currentLog.seenFurniture.Remove(i.id);
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
                Vector3 mov = ((Vector3)parameters[1]).normalized;
                currentLog.movementPosition = new(pos.x, pos.z);
                currentLog.movementDirection = new(mov.x, mov.z);
                break;
            case EventType.PlayerMovementChange:
                currentLog.movementStatus = "M";
                pos = (Vector3)parameters[0];
                mov = ((Vector3)parameters[1]).normalized;
                currentLog.movementPosition = new(pos.x, pos.z);
                currentLog.movementDirection = new(mov.x, mov.z);
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
                currentLog.cameraMovementDirection = (Vector2)parameters[1];
                break;
            case EventType.PlayerRotationChange:
                currentLog.cameraStatus = "M";
                currentLog.cameraOrientation = (Vector2)parameters[0];
                currentLog.cameraMovementDirection = (Vector2)parameters[1];
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
                currentLog.seenItems[(int)parameters[0]].isInteractable = !currentLog.seenItems[(int)parameters[0]].isInteractable;
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
        //GameObject.Find("Status").GetComponent<TextMeshProUGUI>().text = newLog.ToString();
        logList += newLog.ToString();
    }

    public static void ItemInfoLog(int id, string itemName, bool isEnvironmentItem, int spawnId, float spawnPositionX, float spawnPositionY, float spawnPositionZ)
    {
        string itemInfo = id + ";" + itemName + ";" + (isEnvironmentItem ? "L" : "H") + ";" + spawnId + ";(" + Math.Round(spawnPositionX,2).ToString(CultureInfo.InvariantCulture) + ", " + Math.Round(spawnPositionY,2).ToString(CultureInfo.InvariantCulture) + ", " + Math.Round(spawnPositionZ,2).ToString(CultureInfo.InvariantCulture) + ")" + "\n";
        extraItemInfo += itemInfo;
    }

    public static void FurnitureInfoLog(int id, string furnitureName, float posX, float posY, float posZ)
    {
        string furnitureInfo = id + ";" + furnitureName + ";(" + Math.Round(posX,2).ToString(CultureInfo.InvariantCulture) + ", " + Math.Round(posY,2).ToString(CultureInfo.InvariantCulture) + ", " + Math.Round(posZ,2).ToString(CultureInfo.InvariantCulture) + ")\n";
        extraFurnitureInfo += furnitureInfo;
    }

    public static void SpawnInfoLog(List<ItemSpawn> startSpawns, List<ItemSpawn> searchSpawns)
    {
        int id = 0;
        string spawnInfo = "";
        foreach (ItemSpawn sp in startSpawns)
        {
            sp.logId = id;
            spawnInfo += sp.logId + ";" + sp.spawnName + ";" + sp.description + "\n";
            id++;
        }
        foreach (ItemSpawn sp in searchSpawns)
        {
            sp.logId = id;
            spawnInfo += sp.logId + ";" + sp.spawnName + ";" + sp.description + "\n";
            id++;
        }
        extraSpawnInfo += spawnInfo;
    }
}