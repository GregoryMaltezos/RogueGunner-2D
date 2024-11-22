using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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

    

    public void GenerateRoomContent(DungeonData dungeonData)
    {
        foreach (GameObject item in spawnedObjects)
        {
            DestroyImmediate(item);
        }
        spawnedObjects.Clear();

        // Spawn boss room first at the farthest position from any room
        SpawnBossRoom(dungeonData);

        // Spawn item room at a random position
        SpawnItemRoom(dungeonData);

        // Then spawn player room
        SelectPlayerSpawnPoint(dungeonData);

        // Spawn all other rooms
        SelectEnemySpawnPoints(dungeonData);

        foreach (GameObject item in spawnedObjects)
        {
            if (item != null)
                item.transform.SetParent(itemParent, false);
        }
    }

    public void SetBossRoomPosition(Vector2Int position)
    {
        bossRoomPosition = position;
    }

    public void SetItemRoomPosition(Vector2Int position)
    {
        itemRoomPosition = position;
    }

    private void SelectPlayerSpawnPoint(DungeonData dungeonData)
    {
        // Calculate the farthest position from the boss room
        Vector2Int farthestPosition = Vector2Int.zero;
        float maxDistance = float.MinValue;

        foreach (Vector2Int potentialPlayerPosition in dungeonData.roomsDictionary.Keys)
        {
            // Calculate the distance from the boss room
            float distance = Vector2Int.Distance(bossRoomPosition, potentialPlayerPosition);

            // Check if this distance is greater than the current maximum
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestPosition = potentialPlayerPosition;
            }
        }

        // Set the selected player spawn point to the farthest position found
        playerRoomPosition = farthestPosition;

        // Perform any additional logic here, if needed

        // Spawn the player room using the farthest position
        List<GameObject> placedPrefabs = playerRoom.ProcessRoom(
            playerRoomPosition,
            dungeonData.roomsDictionary[playerRoomPosition],
            dungeonData.GetRoomFloorWithoutCorridors(playerRoomPosition)
        );

        FocusCameraOnThePlayer(placedPrefabs[placedPrefabs.Count - 1].transform);

        spawnedObjects.AddRange(placedPrefabs);

        // Remove the player room position from the dictionary to avoid duplication
        dungeonData.roomsDictionary.Remove(playerRoomPosition);
    }


    private void FocusCameraOnThePlayer(Transform playerTransform)
    {
        cinemachineCamera.LookAt = playerTransform;
        cinemachineCamera.Follow = playerTransform;
    }

    private void SelectEnemySpawnPoints(DungeonData dungeonData)
    {
        // Spawn the default room for all rooms except the boss room and item room
        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> roomData in dungeonData.roomsDictionary)
        {
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

    private void SpawnBossRoom(DungeonData dungeonData)
    {
        // Debug log to see current room positions
        Debug.Log("Available room positions: " + string.Join(", ", dungeonData.roomsDictionary.Keys));

        // Set boss room position directly to (0, 0)
        Vector2Int selectedBossRoomPosition = Vector2Int.zero; // Fixed position at (0, 0)
        SetBossRoomPosition(selectedBossRoomPosition);

        // Ensure that the room exists in the dictionary or create an entry if it doesn't
        if (!dungeonData.roomsDictionary.ContainsKey(selectedBossRoomPosition))
        {
            // Add an entry for (0, 0) with an empty set if it does not exist
            dungeonData.roomsDictionary[selectedBossRoomPosition] = new HashSet<Vector2Int>();
            Debug.LogWarning("Added boss room position (0, 0) to rooms dictionary.");
        }

        // Spawn the boss room
        List<GameObject> bossRoomObjects = bossRoom.ProcessRoom(
            selectedBossRoomPosition,
            dungeonData.roomsDictionary[selectedBossRoomPosition], // Pass room floor data
            dungeonData.GetRoomFloorWithoutCorridors(selectedBossRoomPosition) // Pass room floor without corridors data
        );

        // Check if the boss room objects were successfully spawned
        if (bossRoomObjects.Count > 0)
        {
            spawnedObjects.AddRange(bossRoomObjects);
            dungeonData.roomsDictionary.Remove(selectedBossRoomPosition); // Remove after spawning
            Debug.Log("Boss room successfully spawned at (0, 0).");
        }
        else
        {
            Debug.LogWarning("No boss room objects were spawned.");
        }
    }



    private List<Vector2Int> GetAdjacentPositions(Vector2Int roomPosition)
    {
        List<Vector2Int> adjacentPositions = new List<Vector2Int>
        {
            roomPosition + Vector2Int.up,    // Above
            roomPosition + Vector2Int.down,  // Below
            roomPosition + Vector2Int.left,  // Left
            roomPosition + Vector2Int.right  // Right
        };

        return adjacentPositions;
    }

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

    private void SpawnItemRoom(DungeonData dungeonData)
    {
        // Select a random room position for the item room
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
