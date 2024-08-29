using System;
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

        // Initialize the list of placed objects
        List<GameObject> placedObjects = new List<GameObject>();

        // Ensure we have at least one boss to place
        if (bossPlacementData != null && bossPlacementData.Count > 0)
        {
            // Get the boss data (assuming the first entry is the boss)
            EnemyPlacementData bossData = bossPlacementData[0];

            // Attempt to place the boss
            Vector2Int positionToPlace = FindValidPositionNearCenter(roomCenter, roomFloor);
            if (positionToPlace != null)
            {
                // Use the PlaceSingleItem method to place the boss
                GameObject boss = prefabPlacer.PlaceSingleItem(bossData.enemyPrefab, positionToPlace);
                if (boss != null)
                {
                    placedObjects.Add(boss);
                }
            }
        }

        return placedObjects;
    }

    private Vector2Int FindValidPositionNearCenter(Vector2Int center, HashSet<Vector2Int> validPositions)
    {
        // Search for a valid position near the center
        Vector2Int nearestPosition = center;

        // Check if the center position is valid
        if (validPositions.Contains(center))
        {
            return center;
        }

        // If the center is not valid, find the nearest valid position
        int searchRadius = 1;
        while (searchRadius < 100) // Arbitrary large number to avoid infinite loop
        {
            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int y = -searchRadius; y <= searchRadius; y++)
                {
                    Vector2Int potentialPosition = center + new Vector2Int(x, y);
                    if (validPositions.Contains(potentialPosition))
                    {
                        nearestPosition = potentialPosition;
                        return nearestPosition;
                    }
                }
            }
            searchRadius++;
        }

        return nearestPosition;
    }
}
