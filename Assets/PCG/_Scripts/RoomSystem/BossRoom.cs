using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossRoom : RoomGenerator
{
    [SerializeField]
    private PrefabPlacer prefabPlacer;

    public List<EnemyPlacementData> bossPlacementData; // Assuming bossPlacementData is separate from normal enemies

    private bool hasSpawned = false; // Flag to track if spawning has occurred

    public override List<GameObject> ProcessRoom(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        // Check if spawning has already occurred
        if (hasSpawned)
        {
            // Return an empty list as spawning should only happen once
            return new List<GameObject>();
        }

        ItemPlacementHelper itemPlacementHelper =
            new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);

        List<GameObject> placedObjects =
            prefabPlacer.PlaceAllItems(new List<ItemPlacementData>(), itemPlacementHelper); // No items in boss room

        placedObjects.AddRange(prefabPlacer.PlaceEnemies(bossPlacementData, itemPlacementHelper)); // Spawn boss only

        // Update the flag to indicate spawning has occurred
        hasSpawned = true;

        return placedObjects;
    }
}
