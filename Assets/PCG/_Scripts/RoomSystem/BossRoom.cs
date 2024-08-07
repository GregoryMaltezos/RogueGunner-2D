using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoom : RoomGenerator
{
    [SerializeField]
    private PrefabPlacer prefabPlacer;

    public List<EnemyPlacementData> bossPlacementData; // List should contain only the boss data

    public override List<GameObject> ProcessRoom(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        ItemPlacementHelper itemPlacementHelper =
            new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);

        // No items to place in the boss room, so just initialize the list
        List<GameObject> placedObjects = new List<GameObject>();

        // Ensure we have at least one boss to place
        if (bossPlacementData != null && bossPlacementData.Count > 0)
        {
            // Only place the first boss (assuming the first entry in the list is the boss data)
            placedObjects.AddRange(prefabPlacer.PlaceEnemies(new List<EnemyPlacementData> { bossPlacementData[0] }, itemPlacementHelper));
        }

        return placedObjects;
    }
}
