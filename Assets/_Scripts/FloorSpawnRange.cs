using UnityEngine;

[System.Serializable]
public class FloorSpawnRange
{
    public int floorNumber;        // The specific floor this range applies to
    public int minQuantity;        // Minimum number of enemies for this floor
    public int maxQuantity;        // Maximum number of enemies for this floor
}
