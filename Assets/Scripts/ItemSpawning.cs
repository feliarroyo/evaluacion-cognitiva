using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemSpawning : MonoBehaviour
{
    public List<ItemSpawn> itemSpawnPoints_Start = new();
    public List<ItemSpawn> itemSpawnPoints_Search = new();
    public List<ItemSpawn> itemSpawnPoints_Preevaluation = new();
    private readonly List<ItemSpawn> smallSearchSpawns = new();
    private readonly List<ItemSpawn> largeSearchSpawns = new();
    public GameConfig gc;
    public List<GameObject> specialItems;
    readonly Dictionary<int, List<int>> spawnsEnabledInHall =
    new Dictionary<int, List<int>>{
        {0, new() {}},
        {1, new() {2}},
        {2, new() {1, 3}},
        {3, new() {1, 3, 7}},
        {4, new() {0, 3, 6, 9}},
        {5, new() {0, 2, 4, 6, 8}},
        {6, new() {1, 2, 3, 6, 7, 8}},
        {7, new() {0, 1, 3, 4, 6, 7, 8}},
        {8, new() {0, 1, 3, 4, 5, 6, 8, 9}},
        {9, new() {0, 1, 2, 3, 4, 5, 6}},
        {10, new() {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}},
    };
    // private TagField groundTag;

    public enum SpawnType
    {
        aboveEntrance,
        insideEntrance,
        //    aboveTVShelf,
        aboveTVFurniture,
        insideTVShelf,
        //    insideTVDoor,
        oppositeBookshelf,
        aboveLongFurniture,
        //    insideLongFurnitureShelf,
        //    insideLongFurnitureDoor,
        //    insideLongFurnitureDrawer,
        //    aboveOppositeDrawer,
        insideOppositeDrawer,
        couch,
        table,
        chair,
        sideTable,
        rack,
        insideDoor


    }

    // Start is called before the first frame update
    void Start()
    {
        HouseDistributionManage();
        if (GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Start)
        {
            InstantiateItemsTutorialPreevaluation(gc.GenerateTutorialKeyItems(), gc.GenerateTutorialDecoyItems(), null);
        }
        else if (Settings.currentDifficulty == Settings.Difficulty.Preevaluación)
        {
            InstantiateItemsTutorialPreevaluation(gc.preevaluationKeyItemList, gc.preevaluationDecoyItemList, specialItems);
        }
        else if (GameStatus.currentPhase == GameStatus.GamePhase.Waiting)
        {
            InstantiateItems(gc.GenerateKeyItems(), gc.GenerateDecoyItems());
        }

    }

    public void HouseDistributionManage()
    {
        if (Settings.currentDifficulty == Settings.Difficulty.Alto)
        {
            itemSpawnPoints_Search.RemoveAll(spawn => spawn.isLevel1);
        }
        else
        {
            itemSpawnPoints_Search.RemoveAll(spawn => spawn.isLevel2);
        }
    }

    /// <summary>
    /// Instantiate the key and decoy items in the defined spawn points.
    /// </summary>
    /// <param name="hallItemList">List of items that should be retrieved by the user.</param>
    /// <param name="livingItemList">List of items that are added in the environment.</param>
    public void InstantiateItems(Dictionary<string, GameObject> hallItemList, Dictionary<string, GameObject> livingItemList)
    {
        List<GameObject> itemsToMemorize = new();
        List<GameObject> itemsInEnvironment = new();
        // Create list of spawn points available
        List<ItemSpawn> availableSpawnPoints_Start = new(itemSpawnPoints_Start);
        List<ItemSpawn> availableSpawnPoints_Search = new(itemSpawnPoints_Search);

        // Place items of memorize phase in their positions
        List<GameObject> goList = hallItemList.Values.ToList();
        //int shelfItemNumber = goList.Count(item => !item.GetComponent<HeldItem>().validSpawnTypes[(int)SpawnType.rack].isValid);
        //Debug.Log("Shelf item number: " + shelfItemNumber);
        //itemsToMemorize.AddRange(PlaceItemsInHallSpawnpoint(goList, availableSpawnPoints_Start, spawnsEnabledInHall[shelfItemNumber]));

        foreach (string s in hallItemList.Keys)
        {
            // Add to living
            Debug.Log("FOR DE KEY: " + s);
            ItemSpawn getSpawnPoint = availableSpawnPoints_Start.FirstOrDefault(sp => sp.spawnName == s);
            if (getSpawnPoint != null)
                itemsToMemorize.Add(PlaceItemInSpecificSpawnpoint(hallItemList[s], getSpawnPoint, false, true));
        }
        foreach (string s in livingItemList.Keys)
        {
            Debug.Log("FOR DE DECOY: " + s);
            ItemSpawn getSpawnPoint = availableSpawnPoints_Search.FirstOrDefault(sp => sp.spawnName == s);
            if (getSpawnPoint != null)
                itemsInEnvironment.Add(PlaceItemInSpecificSpawnpoint(livingItemList[s], getSpawnPoint, true, false));
        }

        GameStatus.itemsInEnvironment = itemsInEnvironment;
        GameStatus.itemsToMemorize = itemsToMemorize;
    }




    /// <summary>
    /// Instantiate the key and decoy items in the defined spawn points.
    /// </summary>
    /// <param name="keyItemList">List of items that should be retrieved by the user.</param>
    /// <param name="decoyItemList">List of items that are added in the environment.</param>
    public void InstantiateItems(Dictionary<string, List<GameObject>> keyItemList, Dictionary<string, List<GameObject>> decoyItemList)
    {
        List<GameObject> itemsToMemorize = new();
        List<GameObject> itemsInEnvironment = new();
        // Create list of spawn points available
        List<ItemSpawn> availableSpawnPoints_Start = new(itemSpawnPoints_Start);
        GenerateSpawnPoints(new(itemSpawnPoints_Search));
        // Place items of memorize phase in their positions
        foreach (string s in keyItemList.Keys)
        {
            foreach (GameObject item in keyItemList[s])
            {
                GameStatus.keyItems.Add(item.GetComponent<HeldItem>());
                itemsToMemorize.Add(PlaceItemInSpawnpoint(item, availableSpawnPoints_Start, false));
                Logging.DebugLog(item.name + " colocado exitosamente en la repisa");
            }
        }

        List<ItemSpawn> largeSpawnsAvailable = largeSearchSpawns;
        Debug.Log("LSA " + largeSearchSpawns.Count);
        List<ItemSpawn> normalSpawnsAvailable = smallSearchSpawns;
        Debug.Log("SSA " + smallSearchSpawns.Count);
        foreach (string s in keyItemList.Keys)
        {
            Debug.Log(s);
            List<ItemSpawn> filteredLargeSpawnPoints = largeSpawnsAvailable.Where(sp => sp.spawnType.ToString() == s).ToList();
            List<ItemSpawn> filteredNormalSpawnPoints = normalSpawnsAvailable.Where(sp => sp.spawnType.ToString() == s).ToList();
            Debug.Log("Inicio LSA en " + s + ": " + filteredLargeSpawnPoints.Count);
            Debug.Log("Inicio NSA en " + s + ": " + filteredNormalSpawnPoints.Count);
            SpawnItemsLargeFirst(new(keyItemList[s]), ref filteredLargeSpawnPoints, ref filteredNormalSpawnPoints, itemsInEnvironment);
            Debug.Log("Tras Key LSA en " + s + ": " + filteredLargeSpawnPoints.Count);
            Debug.Log("Tras Key NSA en " + s + ": " + filteredNormalSpawnPoints.Count);
            SpawnItemsLargeFirst(new(decoyItemList[s]), ref filteredLargeSpawnPoints, ref filteredNormalSpawnPoints, itemsInEnvironment, true);
            Debug.Log("Tras Fin LSA en " + s + ": " + filteredLargeSpawnPoints.Count);
            Debug.Log("Tras Fin NSA en " + s + ": " + filteredNormalSpawnPoints.Count);
        }

        GameStatus.itemsInEnvironment = itemsInEnvironment;
        GameStatus.itemsToMemorize = itemsToMemorize;
    }






    /// <summary>
    /// Instantiate the key and decoy items in the defined spawn points.
    /// </summary>
    /// <param name="keyItemList">List of items that should be retrieved by the user.</param>
    /// <param name="decoyItemList">List of items that are added in the environment.</param>
    public void InstantiateItemsTutorialPreevaluation(
    List<GameObject> keyItemList, 
    List<GameObject> decoyItemList, 
    List<GameObject> specialBigItems // 👈 Lista de objetos especiales
)
{
    List<GameObject> itemsToMemorize = new();
    List<GameObject> itemsInEnvironment = new();

    // Spawns de inicio fijos (igual que antes)
    List<ItemSpawn> availableSpawnPoints_Start = new() 
    { 
        itemSpawnPoints_Start[2], 
        itemSpawnPoints_Start[9] 
    };

    // 1. Colocar los objetos especiales primero
    var specialSpawns = itemSpawnPoints_Preevaluation
        .Where(sp => sp.isSpecial)
        .ToList();

    if (specialSpawns.Count > 0 && specialBigItems != null && specialBigItems.Count > 0)
    {
        System.Random rng = new();

        foreach (GameObject specialBigItem in specialBigItems)
        {
            if (specialSpawns.Count == 0) break; // no hay más lugares especiales

            // Elegir un spawn especial al azar
            ItemSpawn chosenSpecialSpawn = specialSpawns[rng.Next(specialSpawns.Count)];

            // Instanciar objeto especial en el spawn elegido
            GameObject placedSpecial = PlaceItemInSpawnpoint(
                specialBigItem, 
                new List<ItemSpawn> { chosenSpecialSpawn }, 
                true
            );
            itemsInEnvironment.Add(placedSpecial);

            // Remover TODOS los spawns de ese mismo tipo de la lista general
            itemSpawnPoints_Preevaluation.RemoveAll(sp => sp.spawnType == chosenSpecialSpawn.spawnType);
            specialSpawns.RemoveAll(sp => sp.spawnType == chosenSpecialSpawn.spawnType);

            Debug.Log($"Special object {specialBigItem.name} colocado en {chosenSpecialSpawn.spawnType} - {chosenSpecialSpawn.spawnName}");
        }
    }

    // 2. Selección de los 8 sectores requeridos
    var requiredTypes = new List<SpawnType>
    {
        SpawnType.aboveEntrance,
        SpawnType.table,
        SpawnType.insideDoor,
        SpawnType.insideOppositeDrawer,
        SpawnType.aboveTVFurniture,
        SpawnType.insideTVShelf,
        SpawnType.oppositeBookshelf,
        SpawnType.aboveLongFurniture
    };

    System.Random rngNormal = new();
    var results = requiredTypes
        .Select(type =>
        {
            var options = itemSpawnPoints_Preevaluation.Where(sp => sp.spawnType == type).ToList();
            return options.Count > 0 ? options[rngNormal.Next(options.Count)] : null;
        })
        .Where(sp => sp != null)
        .ToList();

    itemSpawnPoints_Preevaluation = results;

    GenerateSpawnPoints(new(itemSpawnPoints_Preevaluation));
    Debug.Log(availableSpawnPoints_Start);
    Debug.Log(itemSpawnPoints_Preevaluation);

    // 3. Colocar los items de memorización
    foreach (GameObject item in keyItemList)
    {
        GameStatus.keyItems.Add(item.GetComponent<HeldItem>());
        itemsToMemorize.Add(PlaceItemInSpawnpoint(item, availableSpawnPoints_Start, false));
        Logging.DebugLog(item.name + " colocado exitosamente en la repisa");
    }

    // 4. Colocar key items y decoys en los spawns elegidos
    List<ItemSpawn> largeSpawnsAvailable = largeSearchSpawns;
    List<ItemSpawn> normalSpawnsAvailable = smallSearchSpawns;

    SpawnItemsLargeFirst(new(keyItemList), ref largeSpawnsAvailable, ref normalSpawnsAvailable, itemsInEnvironment);

    SpawnItemsLargeFirst(new(decoyItemList), ref largeSpawnsAvailable, ref normalSpawnsAvailable, itemsInEnvironment, true);

    // 5. Guardar referencias
    GameStatus.itemsInEnvironment = itemsInEnvironment;
    GameStatus.itemsToMemorize = itemsToMemorize;
}





    public void SpawnItemsLargeFirst(List<GameObject> itemsToSpawn, ref List<ItemSpawn> largeSpawnsAvailable, ref List<ItemSpawn> normalSpawnsAvailable, List<GameObject> itemsInEnvironment, bool expandChoice = false)
    {
        // Place large items first
        itemsToSpawn.Sort((item1, item2) => item2.GetComponent<HeldItem>().isLargeItem.CompareTo(item1.GetComponent<HeldItem>().isLargeItem));
        // Start with large spawns only at the beginning
        List<ItemSpawn> spawnsAvailable = new(largeSpawnsAvailable);
        bool smallItemsAddedToPool = false;
        foreach (GameObject item in itemsToSpawn)
        {
            Debug.Log("Leo el item " + item.name);
            // once there are no more large items, allow small spawns to be used
            if (!item.GetComponent<HeldItem>().isLargeItem && !smallItemsAddedToPool)
            {
                spawnsAvailable.AddRange(normalSpawnsAvailable);
                smallItemsAddedToPool = true;
            }
            ;
            itemsInEnvironment.Add(PlaceItemInSpawnpoint(item, spawnsAvailable, true));
        }
        if (!smallItemsAddedToPool)
        {
            spawnsAvailable.AddRange(normalSpawnsAvailable);
            smallItemsAddedToPool = true;
        }
        largeSpawnsAvailable = largeSpawnsAvailable.Intersect(spawnsAvailable).ToList();
        normalSpawnsAvailable = normalSpawnsAvailable.Intersect(spawnsAvailable).ToList();
    }

    public void GenerateSpawnPoints(List<ItemSpawn> spawns)
    {
        foreach (ItemSpawn sp in spawns)
        {
            if (sp.allowLargeItems)
            {
                largeSearchSpawns.Add(sp);
            }
            else
            {
                smallSearchSpawns.Add(sp);
            }
        }
    }

    /// <summary>
    /// Place the item in a random spawn point from those available in the list of spawn points.
    /// </summary>
    /// <param name="item">Item to instantiate.</param>
    /// <param name="availableSpawnPoints">List of spawn points where an item can be spawned.</param>
    /// <param name="isKeyItem">Whether the item should be marked as retrieved or not.</param>
    /// <param name="isEnvironmentItem">Whether the item is placed in the search environment or not.</param>
    /// <returns>The GameObject representing the item added.</returns>
    public GameObject PlaceItemInSpawnpoint(GameObject item, List<ItemSpawn> availableSpawnPoints, bool isEnvironmentItem, bool forceInstantiate = false)
    {
        try
        {
            HeldItem heldItem = item.GetComponent<HeldItem>();
            Logging.DebugLog("LISTA DE SPAWNS COMPATIBLES: " + availableSpawnPoints);
            foreach (ItemSpawn spawn in availableSpawnPoints)
            {
                Logging.DebugLog(spawn.name);
            }


            // Choose only from valid spawn points when spawning in the room, filtering the rest
            List<ItemSpawn> compatibleSpawnPoints = availableSpawnPoints;
            // if (isEnvironmentItem)
            // {
            //     compatibleSpawnPoints = availableSpawnPoints.Where(spawn => heldItem.validSpawnTypes[(int)spawn.spawnType].isValid
            //     && (!heldItem.isLargeItem || spawn.allowLargeItems)).ToList();
            // }
            // else
            // {
            //     // Separate between rack and non-rack spawns for hall               
            //     compatibleSpawnPoints = heldItem.validSpawnTypes[(int)SpawnType.rack].isValid ?
            //     availableSpawnPoints.Where(spawn => spawn.spawnType == SpawnType.rack).ToList() : availableSpawnPoints.Where(spawn => !(spawn.spawnType == SpawnType.rack)).ToList();
            // }
            Logging.DebugLog("#Spawns compatibles " + compatibleSpawnPoints.Count + " para " + item.name);
            // foreach(var spawn in compatibleSpawnPoints){
            //     Debug.Log(spawn.name + " es compatible para " + item.name);
            // }

            ItemSpawn next_position = compatibleSpawnPoints[Random.Range(0, compatibleSpawnPoints.Count)];
            Logging.DebugLog("Utilizamos " + next_position + " para " + item.name);
            heldItem.isEnvironmentItem = isEnvironmentItem;
            GameObject go = Instantiate(item, next_position.transform);
            availableSpawnPoints.Remove(next_position);
            return go;
        }
        catch (ArgumentOutOfRangeException)
        {
            Logging.DebugLog("¡No hay suficientes spawns adecuados para el objeto: " + item.name + "! Se generará un objeto sustituto.");

            // Select substitute item
            GameObject itemToAdd = gc.allItemsList[Random.Range(0, gc.allItemsList.Count)];
            gc.allItemsList.Remove(itemToAdd);
            return PlaceItemInSpawnpoint(itemToAdd, availableSpawnPoints, isEnvironmentItem);
        }
    }

    /// <summary>
    /// Place the item in a random spawn point from those available in the list of spawn points.
    /// </summary>
    /// <param name="item">Item to instantiate.</param>
    /// <param name="spawnPoint">List of spawn points where an item can be spawned.</param>
    /// <param name="isKeyItem">Whether the item should be marked as retrieved or not.</param>
    /// <param name="isEnvironmentItem">Whether the item is placed in the search environment or not.</param>
    /// <returns>The GameObject representing the item added.</returns>
    public GameObject PlaceItemInSpecificSpawnpoint(GameObject item, ItemSpawn spawnPoint, bool isEnvironmentItem, bool isHallItem)
    {
        HeldItem heldItem = item.GetComponent<HeldItem>();
        heldItem.isEnvironmentItem = isEnvironmentItem;
        GameObject go = Instantiate(item, spawnPoint.transform);
        if (isHallItem){
            GameStatus.keyItems.Add(heldItem);
        }
        Debug.Log(go.name + " fue colocado en " + spawnPoint.name);
        return go;
    }

    public List<GameObject> PlaceItemsInHallSpawnpoint(List<GameObject> itemList, List<ItemSpawn> availableSpawnPoints, List<int> positions, bool isEnvironmentItem = false)
    {
        int shelfPosition = 0;
        List<GameObject> result = new();
        foreach (GameObject item in itemList)
        {
            try
            {
                HeldItem heldItem = item.GetComponent<HeldItem>();
                GameStatus.keyItems.Add(heldItem);

                // Choose only from valid spawn points when spawning in the room, filtering the rest
                List<ItemSpawn> compatibleSpawnPoints = availableSpawnPoints;
                ItemSpawn next_position;
                // Special case for rack items
                bool isRackSpawn = heldItem.validSpawnTypes[(int)SpawnType.rack].isValid;
                if (isRackSpawn)
                {
                    compatibleSpawnPoints = availableSpawnPoints.Where(spawn => spawn.spawnType == SpawnType.rack).ToList();
                    next_position = compatibleSpawnPoints[Random.Range(0, compatibleSpawnPoints.Count)];
                    availableSpawnPoints.Remove(next_position);
                }
                else
                {
                    Debug.Log("ELSEEEEEEE");
                    Debug.Log("Next position: " + positions[shelfPosition]);
                    next_position = compatibleSpawnPoints[positions[shelfPosition]];
                    shelfPosition++;
                }
                Logging.DebugLog("Utilizamos " + next_position + " para " + item.name);
                heldItem.isEnvironmentItem = isEnvironmentItem;
                GameObject go = Instantiate(item, next_position.transform);
                result.Add(go);
            }
            catch (ArgumentOutOfRangeException)
            {
                Logging.DebugLog("¡No hay suficientes spawns adecuados para el objeto: " + item.name + "!");
                return result;
            }
        }
        return result;
    }

}
