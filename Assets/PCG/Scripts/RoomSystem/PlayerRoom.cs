using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerRoom : RoomGenerator
{
    public GameObject player;

    public List<ItemPlacementData> itemData;

    [SerializeField]
    private PrefabPlacer prefabPlacer;

    public override List<GameObject> ProcessRoom(
        Vector2Int roomCenter, 
        HashSet<Vector2Int> roomFloor, 
        HashSet<Vector2Int> roomFloorNoCorridors)
    {

        ItemPlacementHelper itemPlacementHelper = 
            new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);

        List<GameObject> placedObjects = 
            prefabPlacer.PlaceAllItems(itemData, itemPlacementHelper);

        Vector2Int playerSpawnPoint = roomCenter;

        GameObject playerObject 
            = prefabPlacer.CreateObject(player, playerSpawnPoint + new Vector2(0.5f, 0.5f));
 
        placedObjects.Add(playerObject);

        return placedObjects;
    }
}

public abstract class PlacementData
{
    [Min(0)]
    public int minQuantity = 0;
    [Min(0)]
    [Tooltip("Max is inclusive")]
    public int maxQuantity = 0;
    public int Quantity
        => UnityEngine.Random.Range(minQuantity, maxQuantity + 1);
}

[Serializable]
public class ItemPlacementData : PlacementData
{
    public ItemData itemData;
}

[System.Serializable]
public class EnemyPlacementData
{
    public GameObject enemyPrefab;              // Prefab for the enemy
    public Vector2Int enemySize;                // Size of the enemy
    public List<int> allowedFloors;             // Floors where this enemy type can spawn

    public int minQuantity;                      // General minimum number of enemies if no floor-specific range
    public int maxQuantity;                      // General maximum number of enemies if no floor-specific range

    // List of floor-specific spawn ranges for this enemy type
    public List<FloorSpawnRange> floorSpecificSpawnRanges = new List<FloorSpawnRange>();
}

[Serializable]
public class BossPlacementData : PlacementData
{
    public GameObject bossPrefab;
    public Vector2Int bossSize = Vector2Int.one;
}
