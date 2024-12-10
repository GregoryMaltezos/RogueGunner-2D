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


    /// <summary>
    /// Processes the Player Room by placing items and spawning the player at the center.
    /// </summary>
    /// <param name="roomCenter">The center position of the room where the player will spawn.</param>
    /// <param name="roomFloor">The complete floor area of the room.</param>
    /// <param name="roomFloorNoCorridors">The floor area excluding corridors.</param>
    /// <returns>A list of all game objects (items and player) placed in the room.</returns>
    public override List<GameObject> ProcessRoom(
        Vector2Int roomCenter, 
        HashSet<Vector2Int> roomFloor, 
        HashSet<Vector2Int> roomFloorNoCorridors)
    {
        // Helper for item placement logic based on room structure
        ItemPlacementHelper itemPlacementHelper = 
            new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);
        // Place all predefined items in the room
        List<GameObject> placedObjects = 
            prefabPlacer.PlaceAllItems(itemData, itemPlacementHelper);
        // Determine the player's spawn point (center of the room)
        Vector2Int playerSpawnPoint = roomCenter;
        // Create the player object slightly offset to align with the grid
        GameObject playerObject 
            = prefabPlacer.CreateObject(player, playerSpawnPoint + new Vector2(0.5f, 0.5f));
        // Add the player object to the list of placed objects
        placedObjects.Add(playerObject);

        return placedObjects;
    }
}

public abstract class PlacementData
{
    [Min(0)]
    public int minQuantity = 0; // Minimum quantity of objects to place
    [Min(0)]
    [Tooltip("Max is inclusive")]
    public int maxQuantity = 0; // Maximum quantity of objects to place


    /// <summary>
    /// Randomly determines the quantity of objects to place within the defined range.
    /// </summary>
    public int Quantity
        => UnityEngine.Random.Range(minQuantity, maxQuantity + 1);
}

[Serializable]
public class ItemPlacementData : PlacementData
{
    public ItemData itemData; // Data describing the item to place
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
