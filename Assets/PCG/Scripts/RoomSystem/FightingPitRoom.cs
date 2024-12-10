using System.Collections.Generic;
using UnityEngine;

public class FightingPitRoom : RoomGenerator
{
    [SerializeField]
    private PrefabPlacer prefabPlacer; // Handles placing items and enemies in the room

    public List<EnemyPlacementData> enemyPlacementData;  // List of enemy placement configurations
    public List<ItemPlacementData> itemData; // List of item placement configurations


    /// <summary>
    /// Processes the Fighting Pit room by spawning items and enemies based on the current floor.
    /// </summary>
    /// <param name="roomCenter">The center position of the room (not used directly).</param>
    /// <param name="roomFloor">The complete floor area of the room.</param>
    /// <param name="roomFloorNoCorridors">The floor area excluding corridors.</param>
    /// <returns>A list of all game objects (items and enemies) placed in the room.</returns>
    public override List<GameObject> ProcessRoom(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        // Helper to categorize and place items or enemies based on the room's structure
        ItemPlacementHelper itemPlacementHelper = new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);
        // Place all predefined items in the room
        List<GameObject> placedObjects = prefabPlacer.PlaceAllItems(itemData, itemPlacementHelper);

        // Dynamically find the CorridorFirstDungeonGenerator instance
        CorridorFirstDungeonGenerator dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>();

        if (dungeonGenerator != null)
        {
            // Place enemies based on the current dungeon floor
            placedObjects.AddRange(prefabPlacer.PlaceEnemies(enemyPlacementData, itemPlacementHelper, dungeonGenerator.currentFloor));
        }
        else
        {
            Debug.LogWarning("CorridorFirstDungeonGenerator not found in the scene!");
        }

        return placedObjects;
    }
}
