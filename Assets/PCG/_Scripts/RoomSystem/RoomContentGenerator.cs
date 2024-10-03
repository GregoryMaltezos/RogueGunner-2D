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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var item in spawnedObjects)
            {
                Destroy(item);
            }
            RegenerateDungeon?.Invoke();
        }
    }

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
        //  Debug.Log("Boss room position set to: " + position);
    }

    public void SetItemRoomPosition(Vector2Int position)
    {
        itemRoomPosition = position;
        // Debug.Log("Item room position set to: " + position);
    }

    private void SelectPlayerSpawnPoint(DungeonData dungeonData)
    {
        int randomRoomIndex = Random.Range(0, dungeonData.roomsDictionary.Count);
        Vector2Int playerSpawnPoint = dungeonData.roomsDictionary.Keys.ElementAt(randomRoomIndex);

        // Store player room position
        playerRoomPosition = playerSpawnPoint;

        // Perform any additional logic here, if needed

        List<GameObject> placedPrefabs = playerRoom.ProcessRoom(
            playerSpawnPoint,
            dungeonData.roomsDictionary.Values.ElementAt(randomRoomIndex),
            dungeonData.GetRoomFloorWithoutCorridors(playerSpawnPoint)
        );

        FocusCameraOnThePlayer(placedPrefabs[placedPrefabs.Count - 1].transform);

        spawnedObjects.AddRange(placedPrefabs);

        dungeonData.roomsDictionary.Remove(playerSpawnPoint);
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
        float maxDistance = float.MinValue;
        Vector2Int selectedBossRoomPosition = new Vector2Int();

        // Get all positions within 2 rooms (in all directions) from the player room
        List<Vector2Int> invalidBossRoomPositions = GetInvalidBossRoomPositions(playerRoomPosition);

        foreach (var roomData in dungeonData.roomsDictionary)
        {
            // Skip if the room is within the invalid boss room positions (2 rooms in all directions)
            if (invalidBossRoomPositions.Contains(roomData.Key))
            {
                continue;
            }

            // Find the farthest room from any other room
            foreach (var otherRoomData in dungeonData.roomsDictionary)
            {
                if (roomData.Key != otherRoomData.Key)
                {
                    float distance = Vector2Int.Distance(roomData.Key, otherRoomData.Key);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        selectedBossRoomPosition = roomData.Key;
                    }
                }
            }
        }

        // Set the boss room position
        SetBossRoomPosition(selectedBossRoomPosition);

        // Spawn the boss room (existing logic)
        if (dungeonData.roomsDictionary.ContainsKey(bossRoomPosition))
        {
            List<GameObject> bossRoomObjects = bossRoom.ProcessRoom(
                bossRoomPosition,
                dungeonData.roomsDictionary[bossRoomPosition], // Pass room floor data
                dungeonData.GetRoomFloorWithoutCorridors(bossRoomPosition) // Pass room floor without corridors data
            );

            spawnedObjects.AddRange(bossRoomObjects);
            dungeonData.roomsDictionary.Remove(bossRoomPosition);
        }
        else
        {
            Debug.LogWarning("Boss room position not found in dungeon data.");
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
            // Debugging: Log item room position found in dungeon data
            // Debug.Log("Item room position found in dungeon data.");

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
            //  Debug.LogWarning("Item room position not found in dungeon data.");
        }
    }
}