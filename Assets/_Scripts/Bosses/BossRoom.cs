using System;
using System.Collections.Generic;
using UnityEngine;

public class BossRoom : RoomGenerator
{
    [SerializeField]
    private PrefabPlacer prefabPlacer;

    public List<EnemyPlacementData> bossPlacementData; // List for general boss data
    public EnemyPlacementData fourthFloorBossData; // Special boss data for the 4th floor

    private List<EnemyPlacementData> unusedBosses = new List<EnemyPlacementData>(); // Bosses yet to spawn

    // Reference to CorridorFirstDungeonGenerator to track the current floor
    private CorridorFirstDungeonGenerator dungeonGenerator;


    /// <summary>
    /// Processes the boss room by spawning a boss. The boss type depends on the floor level.
    /// If it's the 4th floor, a special boss is placed; otherwise, a random boss is selected.
    /// </summary>
    /// <param name="roomCenter">The center position of the room (not used for boss placement).</param>
    /// <param name="roomFloor">The full floor area of the room.</param>
    /// <param name="roomFloorNoCorridors">The floor area excluding corridors (not directly used here).</param>
    /// <returns>A list of game objects placed in the room, primarily the boss.</returns>
    public override List<GameObject> ProcessRoom(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        ItemPlacementHelper itemPlacementHelper = new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);
        List<GameObject> placedObjects = new List<GameObject>();
        // Find the dungeon generator if not already cached
        if (dungeonGenerator == null)
        {
            dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>(); // Get reference to the dungeon generator
        }
        // Ensure boss data exists and there is at least one boss to place
        if (dungeonGenerator != null && bossPlacementData != null && bossPlacementData.Count > 0)
        {
            EnemyPlacementData bossData = null;

            // Check if the current floor is 4 to spawn the 4th-floor boss
            if (dungeonGenerator.currentFloor == 4)
            {
                bossData = fourthFloorBossData; // Use the special 4th-floor boss data
            }
            else
            {
                bossData = ChooseBoss(); // Use general boss data for other floors
            }

            if (bossData != null)
            {
                // Set boss's position directly to (0, 0)
                Vector2Int bossPosition = Vector2Int.zero; // Boss appears at the center (0, 0)

                // Spawn the boss without placement restrictions
                GameObject boss = prefabPlacer.PlaceSingleItem(bossData.enemyPrefab, bossPosition, true);

                if (boss != null)
                {
                    placedObjects.Add(boss); // Add the boss to the list of placed objects
                }
            }
        }

        return placedObjects;
    }

    /// <summary>
    /// Selects a boss randomly from the list of unused bosses. If all bosses have been used,
    /// it resets the unused list to allow repetition.
    /// </summary>
    /// <returns>The data of the chosen boss.</returns>
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
}
