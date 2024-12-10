using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PrefabPlacer : MonoBehaviour
{
    [SerializeField]
    private GameObject itemPrefab;

    /// <summary>
    /// Places enemies in the dungeon based on the provided enemy placement data.
    /// Each enemy is placed in an open space, and the number of enemies placed is determined by
    /// the specified floor and spawn range.
    /// </summary>
    /// <param name="enemyPlacementData">The data for the enemies to place.</param>
    /// <param name="itemPlacementHelper">Helper to get valid placement positions.</param>
    /// <param name="currentFloor">The current floor of the dungeon.</param>
    /// <returns>A list of the placed enemy GameObjects.</returns>
    public List<GameObject> PlaceEnemies(List<EnemyPlacementData> enemyPlacementData, ItemPlacementHelper itemPlacementHelper, int currentFloor)
    {
        List<GameObject> placedObjects = new List<GameObject>();

        foreach (var placementData in enemyPlacementData)
        {
            // Skip if the current floor is not in the list of allowed floors for this enemy.
            if (!placementData.allowedFloors.Contains(currentFloor))
                continue;

            // Check if there is a floor-specific spawn range for the current floor.
            FloorSpawnRange floorRange = placementData.floorSpecificSpawnRanges
                .Find(range => range.floorNumber == currentFloor);
            // Use floor-specific spawn range or fallback to the default spawn range.
            int minSpawn = floorRange != null ? floorRange.minQuantity : placementData.minQuantity;
            int maxSpawn = floorRange != null ? floorRange.maxQuantity : placementData.maxQuantity;

            // Randomly determine the number of enemies to place based on the specified range.
            int enemyCount = Random.Range(minSpawn, maxSpawn + 1); // +1 because the upper bound is exclusive

            for (int i = 0; i < enemyCount; i++)
            {
                // Try to find an open space to place the enemy.
                Vector2? possiblePlacementSpot = itemPlacementHelper.GetItemPlacementPosition(
                    PlacementType.OpenSpace,
                    100,
                    placementData.enemySize,
                    false
                );

                if (possiblePlacementSpot.HasValue) // If a valid spot is found, place the enemy
                {
                    Vector2Int placementPosition = Vector2Int.RoundToInt(possiblePlacementSpot.Value + new Vector2(0.5f, 0.5f));
                    GameObject newObject = CreateObject(placementData.enemyPrefab, new Vector3(placementPosition.x, placementPosition.y, 0f));
                    TagAsEnemy(newObject);
                    placedObjects.Add(newObject);
                }
            }
        }
        return placedObjects;
    }

    /// <summary>
    /// Places all items in the dungeon based on the provided item placement data.
    /// Items are sorted by size, and each item is placed in the appropriate position.
    /// </summary>
    /// <param name="itemPlacementData">The data for the items to place.</param>
    /// <param name="itemPlacementHelper">Helper to get valid placement positions.</param>
    /// <returns>A list of the placed item GameObjects.</returns>
    public List<GameObject> PlaceAllItems(List<ItemPlacementData> itemPlacementData, ItemPlacementHelper itemPlacementHelper)
    {
        List<GameObject> placedObjects = new List<GameObject>();
        // Sort items by size in descending order
        IEnumerable<ItemPlacementData> sortedList = new List<ItemPlacementData>(itemPlacementData).OrderByDescending(placementData => placementData.itemData.size.x * placementData.itemData.size.y);

        foreach (var placementData in sortedList)
        {
            // Place each item according to its quantity.
            for (int i = 0; i < placementData.Quantity; i++)
            {
                // Try to find a valid spot for the item.
                Vector2? possiblePlacementSpot = itemPlacementHelper.GetItemPlacementPosition(
                    placementData.itemData.placementType,
                    100,
                    placementData.itemData.size,
                    placementData.itemData.addOffset);
                // If a valid spot is found, place the item.
                if (possiblePlacementSpot.HasValue)
                {
                    GameObject newItem = PlaceItem(placementData.itemData, possiblePlacementSpot.Value);
                    TagAsObstacle(newItem);
                    placedObjects.Add(newItem);
                }
            }
        }
        return placedObjects;
    }

    /// <summary>
    /// Places a single item in the dungeon at a specific location.
    /// </summary>
    /// <param name="item">The item data to be placed.</param>
    /// <param name="placementPosition">The position where the item will be placed.</param>
    /// <returns>The GameObject of the placed item.</returns>
    private GameObject PlaceItem(ItemData item, Vector2 placementPosition)
    {
        // Create and initialize the item at the specified position.
        GameObject newItem = CreateObject(itemPrefab, placementPosition);// Initialize the item with the given data.
        newItem.GetComponent<Item>().Initialize(item);
        TagAsObstacle(newItem);
        return newItem;
    }


    /// <summary>
    /// Places the boss in the dungeon at a valid position.
    /// </summary>
    /// <param name="bossPlacementData">The data for the boss to be placed.</param>
    /// <param name="itemPlacementHelper">Helper to get valid placement positions.</param>
    /// <returns>A list of the placed boss GameObjects.</returns>
    public List<GameObject> PlaceBoss(BossPlacementData bossPlacementData, ItemPlacementHelper itemPlacementHelper)
    {
        List<GameObject> placedObjects = new List<GameObject>();
        // Attempt to place the boss as per the specified quantity.
        for (int i = 0; i < bossPlacementData.Quantity; i++)
        {
            // Find a valid placement spot for the boss.
            Vector2? possiblePlacementSpot = itemPlacementHelper.GetItemPlacementPosition(
                PlacementType.OpenSpace,
                100,
                bossPlacementData.bossSize,
                false
            );
            // If a valid spot is found, place the boss and tag it accordingly.
            if (possiblePlacementSpot.HasValue)
            {
                Debug.Log("Boss placement spot found at: " + possiblePlacementSpot.Value);
                GameObject newObject = CreateObject(bossPlacementData.bossPrefab, possiblePlacementSpot.Value + new Vector2(0.5f, 0.5f));

                // Tag the boss as "Enemy" and do NOT tag as "Obstacle"
                TagAsEnemy(newObject);
                placedObjects.Add(newObject);
            }
            else
            {
                Debug.LogWarning("Boss placement spot not found!");
            }
        }

        return placedObjects;
    }

    /// <summary>
    /// Creates a new GameObject instance of a given prefab at the specified position.
    /// Handles both runtime and editor object creation.
    /// </summary>
    /// <param name="prefab">The prefab to instantiate.</param>
    /// <param name="placementPosition">The position where the new object will be placed.</param>
    /// <returns>The newly created GameObject.</returns>
    public GameObject CreateObject(GameObject prefab, Vector3 placementPosition)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab is null. Cannot place item.");
            return null;
        }

        GameObject newItem;
        if (Application.isPlaying) // Check if the game is running or in editor mode.
        {
            newItem = Instantiate(prefab, placementPosition, Quaternion.identity);
        }
        else
        {
#if UNITY_EDITOR
            newItem = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            newItem.transform.position = placementPosition;
            newItem.transform.rotation = Quaternion.identity;
#else
        Debug.LogWarning("PrefabUtility cannot be used outside of the editor.");
        newItem = null;
#endif
        }

        if (newItem == null)
        {
            Debug.LogWarning("Failed to instantiate item.");
        }

        return newItem;
    }

    /// <summary>
    /// Places a single item in the dungeon at a specified position.
    /// Optionally tags the item as a boss, in which case it is not tagged as an obstacle.
    /// </summary>
    /// <param name="prefab">The prefab of the item to be placed.</param>
    /// <param name="placementPosition">The position where the item will be placed.</param>
    /// <param name="isBoss">If true, the item is tagged as a boss and not as an obstacle.</param>
    /// <returns>The GameObject of the placed item.</returns>
    public GameObject PlaceSingleItem(GameObject prefab, Vector2 placementPosition, bool isBoss = false)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab is null. Cannot place item.");
            return null;
        }

        Debug.Log("Placing item at: " + placementPosition);
        GameObject newItem = CreateObject(prefab, placementPosition); // Create the item at the specified position
        if (newItem == null)
        {
            Debug.LogWarning("Failed to instantiate item.");
        }
        else
        {
            Debug.Log("Item instantiated successfully at: " + placementPosition);
            // Only tag as an obstacle if it is not a boss
            if (!isBoss)
            {
                TagAsObstacle(newItem);
            }
        }
        return newItem;
    }


    /// <summary>
    /// Tags a GameObject as an "Enemy" by setting its tag to "Enemy".
    /// </summary>
    /// <param name="gameObject">The GameObject to tag as an enemy.</param>
    private void TagAsEnemy(GameObject gameObject)
    {
        if (gameObject != null)
        {
            gameObject.tag = "Enemy";  // Set tag to "Enemy"
        }
    }
    /// <summary>
    /// Tags a GameObject as an "Obstacle" by setting its tag to "Obstacle".
    /// </summary>
    /// <param name="gameObject">The GameObject to tag as an obstacle.</param>
    private void TagAsObstacle(GameObject gameObject)
    {
        if (gameObject != null)
        {
            gameObject.tag = "Obstacle";  // Existing method to set tag to "Obstacle"
        }
    }
}
