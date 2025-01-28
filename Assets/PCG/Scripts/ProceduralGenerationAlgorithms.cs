using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ProceduralGenerationAlgorithms
{
    /// <summary>
    /// Generates a path of random walk based on cardinal directions.
    /// </summary>
    /// <param name="startPosition">The starting position of the walk.</param>
    /// <param name="walkLength">The number of steps in the walk.</param>
    /// <returns>A set of unique positions visited during the walk.</returns>
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();

        path.Add(startPosition); // Add the starting position to the path
        var previousPosition = startPosition;

        for (int i = 0; i < walkLength; i++)
        {
            // Move in a random cardinal direction and add the new position to the path
            var newPosition = previousPosition + Direction2D.GetRandomCardinalDirection();
            path.Add(newPosition);
            previousPosition = newPosition;
        }
        return path;
    }

    /// <summary>
    /// Creates a corridor path of a given length starting from a specified position.
    /// </summary>
    /// <param name="startPosition">The starting position of the corridor.</param>
    /// <param name="corridorLength">The length of the corridor to generate.</param>
    /// <returns>A list of positions forming the corridor.</returns>
    public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int corridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var direction = Direction2D.GetRandomCardinalDirection(); // Choose a random direction for the corridor
        var currentPosition = startPosition;
        corridor.Add(currentPosition);
        //corridor.Add(CalculateAdditionalCorridorTile(currentPosition, direction));

        for (int i = 0; i < corridorLength; i++)
        {
            // Move in the chosen direction and add the new position to the corridor
            currentPosition += direction;
            corridor.Add(currentPosition);
            //corridor.Add(CalculateAdditionalCorridorTile(currentPosition, direction));
        }
        return corridor;
    }

    /// <summary>
    /// Calculates an additional tile offset for a corridor based on its direction.
    /// </summary>
    /// <param name="currentPosition">The current position in the corridor.</param>
    /// <param name="direction">The direction of the corridor.</param>
    /// <returns>A new position for the additional tile.</returns>
    private static Vector2Int CalculateAdditionalCorridorTile(Vector2Int currentPosition, Vector2Int direction)
    {
        // Adjust the offset based on the direction of the corridor
        Vector2Int offset = Vector2Int.zero;
        if (direction.y > 0)
            offset.x = 1;
        else if (direction.y < 0)
            offset.x = -1;
        else if (direction.x > 0)
            offset.y = -1;
        else
            offset.y = 1;
        return currentPosition + offset;
    }

    /// <summary>
    /// Performs binary space partitioning on a given space to create smaller rooms.
    /// </summary>
    /// <param name="spaceToSplit">The space to be partitioned.</param>
    /// <param name="minWidth">The minimum allowable width for a partition.</param>
    /// <param name="minHeight">The minimum allowable height for a partition.</param>
    /// <returns>A list of smaller rooms created from the partitioning.</returns>
    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit); // Initialize the queue with the full space to split
        while (roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue();
            // Ensure the room meets the minimum size requirements
            if (room.size.y >= minHeight && room.size.x >= minWidth)
            {
                if(Random.value < 0.5f)
                {
                    // Attempt to split the room horizontally if possible
                    if (room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }else if(room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }else if(room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);// Add the room if it cannot be split further
                    }
                }
                else
                {
                    // Attempt to split the room vertically if possible
                    if (room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    else if (room.size.y >= minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);  // Add the room if it cannot be split further
                    }
                }
            }
        }
        return roomsList;
    }

    /// <summary>
    /// Splits a room vertically and adds the resulting partitions to the queue.
    /// </summary>
    /// <param name="minWidth">The minimum allowable width for a partition.</param>
    /// <param name="roomsQueue">The queue to store the resulting partitions.</param>
    /// <param name="room">The room to split.</param>
    private static void SplitVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        var xSplit = Random.Range(1, room.size.x); // Random vertical split position
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
            new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
        roomsQueue.Enqueue(room1); // Add the left partition to the queue
        roomsQueue.Enqueue(room2); // Add the right partition to the queue
    }

    /// <summary>
    /// Splits a room horizontally and adds the resulting partitions to the queue.
    /// </summary>
    /// <param name="minHeight">The minimum allowable height for a partition.</param>
    /// <param name="roomsQueue">The queue to store the resulting partitions.</param>
    /// <param name="room">The room to split.</param>
    private static void SplitHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        var ySplit = Random.Range(1, room.size.y); // Random horizontal split position
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
            new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
        roomsQueue.Enqueue(room1); // Add the top partition to the queue
        roomsQueue.Enqueue(room2); // Add the bottom partition to the queue
    }
}

public static class Direction2D
{
    /// <summary>
    /// List of cardinal directions in 2D space.
    /// </summary>
    public static List<Vector2Int> cardinalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0,1), //UP
        new Vector2Int(1,0), //RIGHT
        new Vector2Int(0, -1), // DOWN
        new Vector2Int(-1, 0) //LEFT
    };
    /// <summary>
    /// List of diagonal directions in 2D space.
    /// </summary>
    public static List<Vector2Int> diagonalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(1,1), //UP-RIGHT
        new Vector2Int(1,-1), //RIGHT-DOWN
        new Vector2Int(-1, -1), // DOWN-LEFT
        new Vector2Int(-1, 1) //LEFT-UP
    };
    /// <summary>
    /// List of all eight possible directions in 2D space.
    /// </summary>
    public static List<Vector2Int> eightDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0,1), //UP
        new Vector2Int(1,1), //UP-RIGHT
        new Vector2Int(1,0), //RIGHT
        new Vector2Int(1,-1), //RIGHT-DOWN
        new Vector2Int(0, -1), // DOWN
        new Vector2Int(-1, -1), // DOWN-LEFT
        new Vector2Int(-1, 0), //LEFT
        new Vector2Int(-1, 1) //LEFT-UP

    };

    /// <summary>
    /// Retrieves a random cardinal direction from the list.
    /// </summary>
    /// <returns>A random cardinal direction as a Vector2Int.</returns>
    public static Vector2Int GetRandomCardinalDirection()
    {
        return cardinalDirectionsList[UnityEngine.Random.Range(0, cardinalDirectionsList.Count)];
    }
}