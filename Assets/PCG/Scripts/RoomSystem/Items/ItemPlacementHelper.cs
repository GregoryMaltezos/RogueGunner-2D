using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemPlacementHelper
{
    // Dictionary to store tile positions categorized by PlacementType (OpenSpace, NearWall)
    Dictionary<PlacementType, HashSet<Vector2Int>> 
        tileByType = new Dictionary<PlacementType, HashSet<Vector2Int>>();

    // HashSet to store room floor positions excluding corridors
    HashSet<Vector2Int> roomFloorNoCorridor;

    /// <summary>
    /// Constructor for initializing the ItemPlacementHelper with room floor data.
    /// This constructor analyzes the room floor and categorizes positions based on 
    /// whether they are near a wall or an open space.
    /// </summary>
    /// <param name="roomFloor">Set of all positions within the room's floor.</param>
    /// <param name="roomFloorNoCorridor">Set of positions within the room floor excluding corridors.</param>
    public ItemPlacementHelper(HashSet<Vector2Int> roomFloor, 
        HashSet<Vector2Int> roomFloorNoCorridor)
    {
        // Initialize the graph to analyze room connections
        Graph graph = new Graph(roomFloor);
        this.roomFloorNoCorridor = roomFloorNoCorridor;
        // Analyze each position to classify it as either near a wall or open space
        foreach (var position in roomFloorNoCorridor)
        {
            int neighboursCount8Dir = graph.GetNeighbours8Directions(position).Count;
            PlacementType type = neighboursCount8Dir < 8 ? PlacementType.NearWall : PlacementType.OpenSpace;
            // If the dictionary does not contain the type, add it
            if (tileByType.ContainsKey(type) == false)
                tileByType[type] = new HashSet<Vector2Int>();
            // Skip positions that are near a wall and fully surrounded by 4 directions
            if (type == PlacementType.NearWall && graph.GetNeighbours4Directions(position).Count == 4)
                continue;
            // Add the position to the appropriate placement type
            tileByType[type].Add(position);
        }
    }

    /// <summary>
    /// Attempts to find a valid position to place an item of a given size and placement type.
    /// If the item is large, it checks for enough adjacent space, otherwise, it checks for a single valid position.
    /// </summary>
    /// <param name="placementType">Type of placement (OpenSpace, NearWall)</param>
    /// <param name="iterationsMax">Maximum number of attempts to find a valid position.</param>
    /// <param name="size">Size of the item to be placed (width x height).</param>
    /// <param name="addOffset">Whether to add an offset when placing large items.</param>
    /// <returns>A valid position if found, otherwise null.</returns>
    public Vector2? GetItemPlacementPosition(PlacementType placementType, int iterationsMax, Vector2Int size, bool addOffset)
    {
        int itemArea = size.x * size.y;
        // If there are not enough tiles of the required type to fit the item, return null
        if (tileByType[placementType].Count < itemArea)
            return null;

        int iteration = 0;
        while (iteration < iterationsMax)
        {
            iteration++;
            // Randomly pick a tile position from the available tiles
            int index = UnityEngine.Random.Range(0, tileByType[placementType].Count);
            Vector2Int position = tileByType[placementType].ElementAt(index);
            // If the item is larger than 1 tile, check if it can fit
            if (itemArea > 1)
            {
                var (result, placementPositions) = PlaceBigItem(position, size, addOffset);
                // If placing the item failed, continue to the next iteratio
                if (result == false)
                    continue;
                // Remove the occupied positions from available tiles
                tileByType[placementType].ExceptWith(placementPositions);
                tileByType[PlacementType.NearWall].ExceptWith(placementPositions);
            }
            else
            {
                // If the item fits in a single tile, just remove the position
                tileByType[placementType].Remove(position);
            }

            
            return position;
        }
        // Return null if no valid position was found after the specified iterations
        return null;
    }


    /// <summary>
    /// Attempts to place a large item starting from a given position.
    /// It checks if there is enough space to fit the item, considering its size.
    /// </summary>
    /// <param name="originPosition">Starting position for placement.</param>
    /// <param name="size">Size of the item to be placed (width x height).</param>
    /// <param name="addOffset">Whether to add an offset to the placement area.</param>
    /// <returns>A tuple indicating whether placement succeeded and the list of positions occupied by the item.</returns>
    private (bool, List<Vector2Int>) PlaceBigItem(
        Vector2Int originPosition, 
        Vector2Int size ,
        bool addOffset)
    {
        // List to store all positions occupied by the item
        List<Vector2Int> positions = new List<Vector2Int>() { originPosition };
        // Determine the bounds of the item with optional offset
        int maxX = addOffset ? size.x + 1 : size.x;
        int maxY = addOffset ? size.y + 1 : size.y;
        int minX = addOffset ? -1 : 0;
        int minY = addOffset ? -1 : 0;
        // Check all positions within the item's bounds
        for (int row = minX; row <= maxX; row++)
        {
            for (int col = minY; col <= maxY; col++)
            {
                // Skip the origin position itself
                if (col == 0 && row == 0)
                    continue;
                // Calculate the position to check
                Vector2Int newPosToCheck = 
                    new Vector2Int(originPosition.x + row, originPosition.y + col);
                // If the position is not valid (not part of the room floor), return failure
                if (roomFloorNoCorridor.Contains(newPosToCheck) == false)
                    return (false, positions);
                // Add the position to the list of occupied positions
                positions.Add(newPosToCheck);
            }
        }
        // Return success and the list of occupied positions
        return (true, positions);
    }


    /// <summary>
    /// Checks if the room is empty (i.e., no tiles are available for item placement).
    /// </summary>
    /// <returns>True if the room is empty, otherwise false.</returns>
    public bool IsEmptyRoom()
    {
        // Check if any tile set contains any tiles (if any tile set is not empty)
        foreach (HashSet<Vector2Int> tileSet in tileByType.Values)
        {
            if (tileSet.Any())
            {
                return false; // Room is not empty if any tile set is not empty
            }
        }
        return true; // Room is empty if all tile sets are empty
    }


}
/// <summary>
/// Enum representing different types of placements in the room.
/// </summary>
public enum PlacementType
{
    OpenSpace, // Represents a space where an item can be placed freely
    NearWall   // Represents a space near a wall
}
