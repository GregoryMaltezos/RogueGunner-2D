using System.Collections.Generic;
using UnityEngine;

public abstract class RoomGenerator : MonoBehaviour
{
    public abstract List<GameObject> ProcessRoom(
        Vector2Int roomCenter,
        HashSet<Vector2Int> roomFloor,
        HashSet<Vector2Int> corridors);

    // Method to check if the room is empty
    protected bool IsRoomEmpty(HashSet<Vector2Int> roomFloor)
    {
        return roomFloor.Count == 0;
    }
}
