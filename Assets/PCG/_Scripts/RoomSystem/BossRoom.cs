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

    public override List<GameObject> ProcessRoom(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor, HashSet<Vector2Int> roomFloorNoCorridors)
    {
        ItemPlacementHelper itemPlacementHelper = new ItemPlacementHelper(roomFloor, roomFloorNoCorridors);
        List<GameObject> placedObjects = new List<GameObject>();

        if (dungeonGenerator == null)
        {
            dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>(); // Get reference to the dungeon generator
        }

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

                // No placement restrictions
                GameObject boss = prefabPlacer.PlaceSingleItem(bossData.enemyPrefab, bossPosition, true);

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
}
