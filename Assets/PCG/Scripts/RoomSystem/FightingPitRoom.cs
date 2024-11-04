using System.Collections.Generic;
using UnityEngine;

public class FightingPitRoom : RoomGenerator
{
    [SerializeField]
    private PrefabPlacer prefabPlacer;

    public List<EnemyPlacementData> enemyPlacementData;
    public List<ItemPlacementData> itemData;

    public override List<GameObject> ProcessRoom(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        ItemPlacementHelper itemPlacementHelper = new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);

        List<GameObject> placedObjects = prefabPlacer.PlaceAllItems(itemData, itemPlacementHelper);

        // Dynamically find the CorridorFirstDungeonGenerator instance
        CorridorFirstDungeonGenerator dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>();

        if (dungeonGenerator != null)
        {
            // Access the currentFloor from the found dungeonGenerator instance
            placedObjects.AddRange(prefabPlacer.PlaceEnemies(enemyPlacementData, itemPlacementHelper, dungeonGenerator.currentFloor));
        }
        else
        {
            Debug.LogWarning("CorridorFirstDungeonGenerator not found in the scene!");
        }

        return placedObjects;
    }
}
