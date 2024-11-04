using System.Collections.Generic;
using UnityEngine;

public class ItemRoom : RoomGenerator
{
    [SerializeField]
    private PrefabPlacer prefabPlacer;

    public List<ItemPlacementData> itemData;
    public GameObject treasurePrefab; // Add this line

    public override List<GameObject> ProcessRoom(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        ItemPlacementHelper itemPlacementHelper = new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);

        // Place all items in the room
        List<GameObject> placedObjects = prefabPlacer.PlaceAllItems(itemData, itemPlacementHelper);

        // Place the treasure in the middle of the room
        Vector3 treasurePosition = new Vector3(roomCenter.x + 0.5f, roomCenter.y + 0.5f, 0); // Centered position
        Vector3 adjustedTreasurePosition = GetAdjustedPosition(treasurePosition, roomFloorNoCorridors);

        if (adjustedTreasurePosition != Vector3.zero)
        {
            GameObject treasure = prefabPlacer.PlaceSingleItem(treasurePrefab, adjustedTreasurePosition); // Using new method to create the object
            if (treasure != null)
            {
                placedObjects.Add(treasure);
               // Debug.Log("Treasure spawned at: " + adjustedTreasurePosition);
            }
            else
            {
                Debug.LogWarning("Failed to spawn treasure at: " + adjustedTreasurePosition);
            }
        }
        else
        {
            Debug.LogWarning("No valid position found for treasure placement.");
        }

        return placedObjects;
    }

    private Vector3 GetAdjustedPosition(Vector3 originalPosition, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        // First, check if the original position and surroundings are valid
        if (!IsPositionObstructedWithSurroundings(originalPosition, roomFloorNoCorridors))
        {
            return originalPosition; // Return the original position if it's valid
        }

        // If obstructed, search for the nearest valid position
        Vector2Int nearbyPosition = FindNearbyValidPosition(originalPosition, roomFloorNoCorridors);
        if (nearbyPosition != Vector2Int.zero)
        {
            return new Vector3(nearbyPosition.x + 0.5f, nearbyPosition.y + 0.5f, 0);
        }

        // If no valid position is found, return Vector3.zero as a signal of failure
        return Vector3.zero;
    }

    private bool IsPositionObstructed(Vector3 position)
    {
        // Check if there's an obstacle at the specified position
        Collider2D hit = Physics2D.OverlapCircle(position, 0.3f); // Check with a small radius
        return hit != null;
    }

    private bool IsPositionObstructedWithSurroundings(Vector3 position, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));

        // Check if the center position is valid
        if (!roomFloorNoCorridors.Contains(gridPos) || IsPositionObstructed(position))
        {
            return true; // If the center is obstructed or outside valid room area
        }

        // Check all 8 surrounding positions (up, down, left, right, diagonals)
        Vector2Int[] directions = {
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0),   // Right
            new Vector2Int(0, -1),  // Down
            new Vector2Int(0, 1),   // Up
            new Vector2Int(-1, -1), // Bottom-left
            new Vector2Int(1, -1),  // Bottom-right
            new Vector2Int(-1, 1),  // Top-left
            new Vector2Int(1, 1)    // Top-right
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int adjacentPos = gridPos + dir;
            Vector3 adjacentWorldPos = new Vector3(adjacentPos.x + 0.5f, adjacentPos.y + 0.5f, 0);

            // If any surrounding position is obstructed or outside the room bounds
            if (!roomFloorNoCorridors.Contains(adjacentPos) || IsPositionObstructed(adjacentWorldPos))
            {
                return true; // Invalid if any surrounding tile is blocked
            }
        }

        // If the position and all surroundings are valid
        return false;
    }

    private Vector2Int FindNearbyValidPosition(Vector3 originalPosition, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        Vector2Int originalGridPos = new Vector2Int(Mathf.RoundToInt(originalPosition.x), Mathf.RoundToInt(originalPosition.y));

        // Expand search radius dynamically
        for (int radius = 1; radius <= 5; radius++) // Adjust 5 based on the room size
        {
            // Check positions around the original point in an expanding square
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2Int positionToCheck = originalGridPos + new Vector2Int(x, y);

                    // Ensure the position is part of the room, has no obstructions, and is surrounded by free space
                    if (roomFloorNoCorridors.Contains(positionToCheck) &&
                        !IsPositionObstructedWithSurroundings(new Vector3(positionToCheck.x + 0.5f, positionToCheck.y + 0.5f, 0), roomFloorNoCorridors))
                    {
                        return positionToCheck;
                    }
                }
            }
        }

        // No valid position found within a reasonable radius
        return Vector2Int.zero;
    }
}
