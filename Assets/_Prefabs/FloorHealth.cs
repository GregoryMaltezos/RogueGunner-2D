using UnityEngine;

[System.Serializable]
public class FloorHealth
{
    public int floorNumber; // The floor number
    public int health;      // The health for this floor

    public FloorHealth(int floorNumber, int health)
    {
        this.floorNumber = floorNumber;
        this.health = health;
    }
}
