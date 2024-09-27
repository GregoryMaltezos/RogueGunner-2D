using System;
using System.Collections.Generic;
using UnityEngine;

public class BossRoom : RoomGenerator
{
    [SerializeField]
    private PrefabPlacer prefabPlacer;

    public List<EnemyPlacementData> bossPlacementData; // List should contain only the boss data

    private List<EnemyPlacementData> unusedBosses = new List<EnemyPlacementData>(); // Bosses yet to spawn

    public override List<GameObject> ProcessRoom(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        ItemPlacementHelper itemPlacementHelper =
            new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);

        // Initialize the list of placed objects
        List<GameObject> placedObjects = new List<GameObject>();

        // Ensure we have at least one boss to place
        if (bossPlacementData != null && bossPlacementData.Count > 0)
        {
            // Choose a boss that hasn't been recently spawned
            EnemyPlacementData bossData = ChooseBoss();

            if (bossData != null)
            {
                // Attempt to place the boss
                Vector2Int positionToPlace = FindValidPositionNearCenter(roomCenter, roomFloor);
                if (positionToPlace != null && CanPlaceBoss(positionToPlace, roomFloor)) // Add the check here
                {
                    // Use the PlaceSingleItem method to place the boss
                    GameObject boss = prefabPlacer.PlaceSingleItem(bossData.enemyPrefab, positionToPlace);
                    if (boss != null)
                    {
                        placedObjects.Add(boss);
                    }
                }
            }
        }

        return placedObjects;
    }

    private EnemyPlacementData ChooseBoss()
    {
        // Initialize the unusedBosses list if it's empty
        if (unusedBosses.Count == 0)
        {
            unusedBosses = new List<EnemyPlacementData>(bossPlacementData);
        }

        // Choose a random boss from the unused ones
        EnemyPlacementData chosenBoss = unusedBosses[UnityEngine.Random.Range(0, unusedBosses.Count)];

        // Remove the chosen boss from the unused list so it doesn't spawn again
        unusedBosses.Remove(chosenBoss);

        return chosenBoss;
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

    // New method to check if the boss can be placed
    private bool CanPlaceBoss(Vector2Int position, HashSet<Vector2Int> validPositions)
    {
        int emptySpaces = 0;

        // Check all 8 surrounding positions
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Skip the center position
                Vector2Int adjacentPosition = position + new Vector2Int(x, y);
                if (validPositions.Contains(adjacentPosition))
                {
                    emptySpaces++;
                }

                // If we already found 2 empty spaces, we can return true
                if (emptySpaces >= 2)
                {
                    return true;
                }
            }
        }

        return false; // Not enough empty spaces
    }
}
