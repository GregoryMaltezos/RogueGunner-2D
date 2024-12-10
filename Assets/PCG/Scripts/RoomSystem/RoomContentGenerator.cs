using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Handles the generation of content for rooms in a dungeon, including boss rooms, item rooms,
/// player rooms, and default rooms. Also manages spawned objects and sets up the camera focus.
/// </summary>
public class RoomContentGenerator : MonoBehaviour
{
    [SerializeField]
    private RoomGenerator playerRoom, defaultRoom, bossRoom, itemRoom; // Add itemRoom
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Vector2Int bossRoomPosition; // Store boss room position
    private Vector2Int playerRoomPosition; // Store player room position
    private Vector2Int itemRoomPosition; // Store item room position

    [SerializeField]
    private CinemachineVirtualCamera cinemachineCamera;

    public Transform itemParent;

    public UnityEvent RegenerateDungeon;


    /// <summary>
    /// Generates content for all rooms in the dungeon, including player, boss, item, and default rooms.
    /// Cleans up previously spawned objects before generating new content.
    /// </summary>
    /// <param name="dungeonData">Data structure containing information about the dungeon layout.</param>
    public void GenerateRoomContent(DungeonData dungeonData)
    {
        foreach (GameObject item in spawnedObjects) // Clean up any previously spawned objects to ensure the dungeon is recreated fresh
        {
            DestroyImmediate(item);
        }
        spawnedObjects.Clear();

        SpawnBossRoom(dungeonData);  // Spawn the boss room first at a fixed position (0, 0)

        
        SpawnItemRoom(dungeonData);  // Spawn the item room at a random valid position

        
        SelectPlayerSpawnPoint(dungeonData); // Select and spawn the player room at the farthest position from the boss room

       
        SelectEnemySpawnPoints(dungeonData);  // Spawn all other enemy rooms, avoiding boss and item rooms

        foreach (GameObject item in spawnedObjects) // Organize all spawned objects under the itemParent transform
        {
            if (item != null)
                item.transform.SetParent(itemParent, false);
        }
    }
    /// <summary>
    /// Sets the position of the boss room.
    /// </summary>
    /// <param name="position">Position to set for the boss room.</param>
    public void SetBossRoomPosition(Vector2Int position)
    {
        bossRoomPosition = position;
    }

    /// <summary>
    /// Sets the position of the item room.
    /// </summary>
    /// <param name="position">Position to set for the item room.</param>
    public void SetItemRoomPosition(Vector2Int position)
    {
        itemRoomPosition = position;
    }

    /// <summary>
    /// Selects the player spawn point, which is the farthest position from the boss room.
    /// </summary>
    /// <param name="dungeonData">Data structure containing information about the dungeon layout.</param>
    private void SelectPlayerSpawnPoint(DungeonData dungeonData)
    {
        
        Vector2Int farthestPosition = Vector2Int.zero;  // Find the farthest position from the boss room to place the player room
        float maxDistance = float.MinValue; 

        foreach (Vector2Int potentialPlayerPosition in dungeonData.roomsDictionary.Keys)
        {
            
            float distance = Vector2Int.Distance(bossRoomPosition, potentialPlayerPosition); // Calculate the distance from the boss room for each potential player spawn position

            
            if (distance > maxDistance) // Track the farthest position
            {
                maxDistance = distance;
                farthestPosition = potentialPlayerPosition;
            }
        }

        playerRoomPosition = farthestPosition;  // Set the player room position to the farthest valid position

        

        // Spawn the player room at the farthest position found
        List<GameObject> placedPrefabs = playerRoom.ProcessRoom( 
            playerRoomPosition,
            dungeonData.roomsDictionary[playerRoomPosition],
            dungeonData.GetRoomFloorWithoutCorridors(playerRoomPosition)
        );

        FocusCameraOnThePlayer(placedPrefabs[placedPrefabs.Count - 1].transform); // Set the camera to focus on the player

        spawnedObjects.AddRange(placedPrefabs); // Add the spawned player room objects to the list of spawned objects

        // Remove the player room position from the dictionary to avoid duplication
        dungeonData.roomsDictionary.Remove(playerRoomPosition); 
    }

    /// <summary>
    /// Focuses the camera on the player's position.
    /// </summary>
    /// <param name="playerTransform">Transform of the player to focus the camera on.</param>
    private void FocusCameraOnThePlayer(Transform playerTransform)
    {
        cinemachineCamera.LookAt = playerTransform;
        cinemachineCamera.Follow = playerTransform;
    }

    /// <summary>
    /// Selects and spawns enemy rooms for all rooms except the boss and item rooms.
    /// </summary>
    /// <param name="dungeonData">Data structure containing information about the dungeon layout.</param>
    private void SelectEnemySpawnPoints(DungeonData dungeonData)
    {
        // Spawn the default room for all rooms except the boss and item rooms
        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> roomData in dungeonData.roomsDictionary)
        {
            // Skip the boss and item room positions
            if (bossRoomPosition != roomData.Key && itemRoomPosition != roomData.Key)
            {
                spawnedObjects.AddRange(
                    defaultRoom.ProcessRoom(
                        roomData.Key,
                        roomData.Value,
                        dungeonData.GetRoomFloorWithoutCorridors(roomData.Key)
                    )
                );
            }
        }
    }

    /// <summary>
    /// Spawns the boss room at a fixed position (0, 0) in the dungeon.
    /// </summary>
    /// <param name="dungeonData">Data structure containing information about the dungeon layout.</param>
    private void SpawnBossRoom(DungeonData dungeonData)
    {
        // Debug log to see current room positions
        Debug.Log("Available room positions: " + string.Join(", ", dungeonData.roomsDictionary.Keys));

       
        Vector2Int selectedBossRoomPosition = Vector2Int.zero; // Fixed position at (0, 0)
        SetBossRoomPosition(selectedBossRoomPosition);

        // Ensure the boss room position exists in the dungeon data dictionary
        if (!dungeonData.roomsDictionary.ContainsKey(selectedBossRoomPosition))
        {
            // If not, create an entry for (0, 0)
            dungeonData.roomsDictionary[selectedBossRoomPosition] = new HashSet<Vector2Int>();
            Debug.LogWarning("Added boss room position (0, 0) to rooms dictionary.");
        }

        // Spawn the boss room at the fixed position (0, 0)
        List<GameObject> bossRoomObjects = bossRoom.ProcessRoom(
            selectedBossRoomPosition,
            dungeonData.roomsDictionary[selectedBossRoomPosition], // Pass room floor data
            dungeonData.GetRoomFloorWithoutCorridors(selectedBossRoomPosition) // Pass room floor without corridors data
        );

        // If the boss room objects are successfully spawned, add them to the list
        if (bossRoomObjects.Count > 0)
        {
            spawnedObjects.AddRange(bossRoomObjects);
            dungeonData.roomsDictionary.Remove(selectedBossRoomPosition);  // Remove after spawning to prevent re-spawning
            Debug.Log("Boss room successfully spawned at (0, 0).");
        }
        else
        {
            Debug.LogWarning("No boss room objects were spawned.");
        }
    }


    /// <summary>
    /// Returns a list of adjacent room positions around the given room position.
    /// </summary>
    /// <param name="roomPosition">Position of the room to check for adjacent rooms.</param>
    /// <returns>A list of adjacent room positions.</returns>
    private List<Vector2Int> GetAdjacentPositions(Vector2Int roomPosition)
    {
        List<Vector2Int> adjacentPositions = new List<Vector2Int> // Return positions above, below, left, and right of the current room
        {
            roomPosition + Vector2Int.up,    // Above
            roomPosition + Vector2Int.down,  // Below
            roomPosition + Vector2Int.left,  // Left
            roomPosition + Vector2Int.right  // Right
        };

        return adjacentPositions;
    }

    /// <summary>
    /// Returns a list of invalid positions for the boss room based on the player's room position.
    /// </summary>
    /// <param name="playerRoomPosition">Position of the player room to avoid placing the boss room nearby.</param>
    /// <returns>A list of invalid positions for the boss room.</returns>
    private List<Vector2Int> GetInvalidBossRoomPositions(Vector2Int playerRoomPosition)
    {
        List<Vector2Int> invalidPositions = new List<Vector2Int>();

        // Loop through a square area around the player's spawn room (2 rooms in all directions)
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                Vector2Int position = playerRoomPosition + new Vector2Int(x, y);
                invalidPositions.Add(position); // Add all positions within 2 rooms from the player room
            }
        }

        return invalidPositions;
    }

    /// <summary>
    /// Spawns the item room at a random position in the dungeon.
    /// </summary>
    /// <param name="dungeonData">Data structure containing information about the dungeon layout.</param>
    private void SpawnItemRoom(DungeonData dungeonData)
    {
        // Select a random position for the item room
        int randomRoomIndex = Random.Range(0, dungeonData.roomsDictionary.Count);
        Vector2Int selectedItemRoomPosition = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);

        // Set the item room position
        SetItemRoomPosition(selectedItemRoomPosition);

        // Check if the item room position exists in the dictionary
        if (dungeonData.roomsDictionary.ContainsKey(itemRoomPosition))
        {
            // Spawn the item room
            List<GameObject> itemRoomObjects = itemRoom.ProcessRoom(
                itemRoomPosition,
                dungeonData.roomsDictionary[itemRoomPosition], // Pass room floor data
                dungeonData.GetRoomFloorWithoutCorridors(itemRoomPosition) // Pass room floor without corridors data
            );

            // Add spawned item room objects to the list of spawned objects
            spawnedObjects.AddRange(itemRoomObjects);

            // Remove the item room from the dictionary to avoid spawning it again
            dungeonData.roomsDictionary.Remove(itemRoomPosition);
        }
        else
        {
            Debug.LogWarning("Item room position not found in dungeon data.");
        }
    }
}
