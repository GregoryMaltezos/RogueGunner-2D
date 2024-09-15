using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PrefabPlacer : MonoBehaviour
{
    [SerializeField]
    private GameObject itemPrefab;

    public List<GameObject> PlaceEnemies(List<EnemyPlacementData> enemyPlacementData, ItemPlacementHelper itemPlacementHelper)
    {
        List<GameObject> placedObjects = new List<GameObject>();

        foreach (var placementData in enemyPlacementData)
        {
            for (int i = 0; i < placementData.Quantity; i++)
            {
                Vector2? possiblePlacementSpot = itemPlacementHelper.GetItemPlacementPosition(
                    PlacementType.OpenSpace,
                    100,
                    placementData.enemySize,
                    false
                    );
                if (possiblePlacementSpot.HasValue)
                {
                    GameObject newObject = CreateObject(placementData.enemyPrefab, possiblePlacementSpot.Value + new Vector2(0.5f, 0.5f));
                    TagAsObstacle(newObject);
                    placedObjects.Add(newObject);
                }
            }
        }
        return placedObjects;
    }

    public List<GameObject> PlaceAllItems(List<ItemPlacementData> itemPlacementData, ItemPlacementHelper itemPlacementHelper)
    {
        List<GameObject> placedObjects = new List<GameObject>();

        IEnumerable<ItemPlacementData> sortedList = new List<ItemPlacementData>(itemPlacementData).OrderByDescending(placementData => placementData.itemData.size.x * placementData.itemData.size.y);

        foreach (var placementData in sortedList)
        {
            for (int i = 0; i < placementData.Quantity; i++)
            {
                Vector2? possiblePlacementSpot = itemPlacementHelper.GetItemPlacementPosition(
                    placementData.itemData.placementType,
                    100,
                    placementData.itemData.size,
                    placementData.itemData.addOffset);

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

    private GameObject PlaceItem(ItemData item, Vector2 placementPosition)
    {
        GameObject newItem = CreateObject(itemPrefab, placementPosition);
        newItem.GetComponent<Item>().Initialize(item);
        TagAsObstacle(newItem);
        return newItem;
    }

    public List<GameObject> PlaceBoss(BossPlacementData bossPlacementData, ItemPlacementHelper itemPlacementHelper)
    {
        List<GameObject> placedObjects = new List<GameObject>();

        for (int i = 0; i < bossPlacementData.Quantity; i++)
        {
            Vector2? possiblePlacementSpot = itemPlacementHelper.GetItemPlacementPosition(
                PlacementType.OpenSpace,
                100,
                bossPlacementData.bossSize,
                false
            );

            if (possiblePlacementSpot.HasValue)
            {
                Debug.Log("Boss placement spot found at: " + possiblePlacementSpot.Value);
                GameObject newObject = CreateObject(bossPlacementData.bossPrefab, possiblePlacementSpot.Value + new Vector2(0.5f, 0.5f));
                TagAsObstacle(newObject);
                placedObjects.Add(newObject);
            }
            else
            {
                Debug.LogWarning("Boss placement spot not found!");
            }
        }

        return placedObjects;
    }

    public GameObject CreateObject(GameObject prefab, Vector3 placementPosition)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab is null. Cannot place item.");
            return null;
        }

        GameObject newItem;
        if (Application.isPlaying)
        {
            newItem = Instantiate(prefab, placementPosition, Quaternion.identity);
        }
        else
        {
            newItem = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            newItem.transform.position = placementPosition;
            newItem.transform.rotation = Quaternion.identity;
        }

        if (newItem == null)
        {
            Debug.LogWarning("Failed to instantiate item.");
        }
        else
        {
    //        Debug.Log("Item instantiated successfully at: " + placementPosition);
        }

        return newItem;
    }

    public GameObject PlaceSingleItem(GameObject prefab, Vector2 placementPosition)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab is null. Cannot place item.");
            return null;
        }

        Debug.Log("Placing item at: " + placementPosition);
        GameObject newItem = CreateObject(prefab, placementPosition);
        if (newItem == null)
        {
            Debug.LogWarning("Failed to instantiate item.");
        }
        else
        {
            Debug.Log("Item instantiated successfully at: " + placementPosition);
        }
        TagAsObstacle(newItem);
        return newItem;
    }

    private void TagAsObstacle(GameObject gameObject)
    {
        if (gameObject != null)
        {
            gameObject.tag = "Obstacle";
        }
    }
}
