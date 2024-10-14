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
        ItemPlacementHelper itemPlacementHelper = new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);
        List<GameObject> placedObjects = new List<GameObject>();

        if (bossPlacementData != null && bossPlacementData.Count > 0)
        {
            EnemyPlacementData bossData = ChooseBoss();

            if (bossData != null)
            {
                // Set boss's position directly to (0, 0)
                Vector2Int bossPosition = Vector2Int.zero; // (0, 0)

                // Remove the check for placement restrictions
                GameObject boss = prefabPlacer.PlaceSingleItem(bossData.enemyPrefab, bossPosition);
                if (boss != null)
                {
                    placedObjects.Add(boss);
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

    // New method to retry finding a valid position if the initial one fails
    private Vector2Int RetryFindValidPosition(Vector2Int center, HashSet<Vector2Int> validPositions)
    {
        int retries = 0;
        Vector2Int validPosition = center; // Start checking from the center

        while (retries < 5) // Limit the number of retries to avoid potential infinite loops
        {
            validPosition = FindValidPositionNearCenter(center + new Vector2Int(UnityEngine.Random.Range(-3, 3), UnityEngine.Random.Range(-3, 3)), validPositions);
            if (validPositions.Contains(validPosition))
            {
                return validPosition;
            }
            retries++;
        }

        return validPosition; // Return the last attempted position (which may not be valid)
    }

    private bool CanPlaceBoss(Vector2Int position, HashSet<Vector2Int> validPositions)
    {
        int emptySpaces = 0;
        float checkRadius = 1.0f; // Define a small radius to check for obstacles

        // Check all 8 surrounding positions
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Skip the center position (the boss's intended position)

                Vector2Int adjacentPosition = position + new Vector2Int(x, y);

                // Check if the adjacent position is within valid positions (room floor)
                if (validPositions.Contains(adjacentPosition))
                {
                    emptySpaces++;
                }
            }
        }

        // Perform a physics check for obstacles around the boss's intended position
        if (!IsAreaFreeOfObstacles(position, checkRadius))
        {
            return false; // If any obstacles are found, the boss cannot be placed
        }

        // Boss can be placed if there are enough empty spaces and no obstacles
        return emptySpaces >= 2;
    }

    private bool IsAreaFreeOfObstacles(Vector2Int position, float radius)
    {
        // Convert the Vector2Int position to a Vector3 or Vector2, depending on your setup
        Vector2 positionInWorld = new Vector2(position.x, position.y);

        // Check for colliders within the specified radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(positionInWorld, radius);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Obstacle"))
            {
                return false; // An obstacle was found
            }
        }

        return true; // No obstacles found
    }
}
