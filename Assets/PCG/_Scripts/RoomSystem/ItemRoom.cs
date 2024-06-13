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
                Debug.Log("Treasure spawned at: " + adjustedTreasurePosition);
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
        Vector3 adjustedPosition = originalPosition;
        RaycastHit2D hit = Physics2D.Raycast(originalPosition, Vector2.zero);

        if (hit.collider != null)
        {
            // If the original position is obstructed, find a nearby valid position
            Vector2Int nearbyPosition = FindNearbyValidPosition(originalPosition, roomFloorNoCorridors);
            if (nearbyPosition != Vector2Int.zero)
            {
                adjustedPosition = new Vector3(nearbyPosition.x + 0.5f, nearbyPosition.y + 0.5f, 0);
            }
        }

        return adjustedPosition;
    }

    private Vector2Int FindNearbyValidPosition(Vector3 originalPosition, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        Vector2Int nearbyPosition = Vector2Int.zero;
        float maxDistance = 1f; // Maximum distance to search for a nearby valid position

        for (float x = -maxDistance; x <= maxDistance; x++)
        {
            for (float y = -maxDistance; y <= maxDistance; y++)
            {
                Vector2Int positionToCheck = new Vector2Int(Mathf.RoundToInt(originalPosition.x + x), Mathf.RoundToInt(originalPosition.y + y));
                if (roomFloorNoCorridors.Contains(positionToCheck))
                {
                    nearbyPosition = positionToCheck;
                    break;
                }
            }
        }

        return nearbyPosition;
    }
}
