using UnityEngine;
using TMPro; 
using UnityEngine.UI; 
using UnityEngine.Events; 
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using System;

public class CorridorFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    // PCG parameters
    [SerializeField]
    private int corridorLength = 14; 
    [SerializeField]
    private int corridorCount = 5;   
    [SerializeField]
    [Range(0.1f, 1)]
    private float roomPercent = 0.8f;
    public RoomContentGenerator roomContentGenerator;

    // UI
    [SerializeField]
    private TextMeshProUGUI floorNotificationText; // Updated to TextMeshProUGUI
    [SerializeField]
    private CanvasGroup floorNotificationCanvasGroup; // Reference to the CanvasGroup component

    [SerializeField]
    [Range(0, 1920)] // Adjust to screen width range
    private float finalPositionX = 100f; // Final position X

    [SerializeField]
    [Range(0, 1080)] // Adjust to screen height range
    private float finalPositionY = 100f; // Final position Y

    // Manually set start position
    [SerializeField]
    private Vector2 startNotificationPosition = new Vector2(0, 0); // Default to top middle

    private RectTransform floorNotificationRectTransform; // For UI positioning and scaling
    public int currentFloor = 1; 

    // PCG Data
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private HashSet<Vector2Int> floorPositions, corridorPositions;

    // Gizmos Data
    private List<Color> roomColors = new List<Color>();
    [SerializeField]
    private bool showRoomGizmo = false, showCorridorsGizmo;

    // Events
    public UnityEvent<DungeonData> OnDungeonFloorReady;
    public UnityEvent OnBossKilled;  // Event to trigger on boss kill

    private int bossKills = 0; // To keep track of how many times the boss was killed
    private bool isFirstFloorGenerated = false; // Flag to check if the first floor is generated
    public int CurrentFloor => currentFloor;
    public static int urrentFloor { get; set; }


    void Start()
    {
        currentFloor = 1; // Initialize starting floor to 1
    }

    /// <summary>
    /// Starts the dungeon generation process.
    /// This method can be called publicly to initiate the generation of the dungeon.
    /// </summary>
    public void StartDungeonGeneration()
    {
        Debug.Log("Starting dungeon generation...");
        // This method can be called publicly to start the dungeon generation process
        RunProceduralGeneration();

    }


    /// <summary>
    /// Runs the main procedural generation logic for the dungeon.
    /// It creates corridors, generates rooms, and handles dungeon events.
    /// </summary>
    protected override void RunProceduralGeneration()
    {
        Debug.Log("Running procedural generation...");
        CorridorFirstGeneration();
        DungeonData data = new DungeonData
        {
            roomsDictionary = this.roomsDictionary, // Save the generated rooms data
            corridorPositions = this.corridorPositions, // Save corridor positions
            floorPositions = this.floorPositions // Save floor positions
        };
        OnDungeonFloorReady?.Invoke(data); // Notify listeners that dungeon floor is ready
    }

    /// <summary>
    /// Generates the first floor of the dungeon, starting by creating corridors and then generating rooms at the ends of these corridors.
    /// It also handles resetting player preferences (related to guns) when the first floor is generated.
    /// </summary>
    private void CorridorFirstGeneration()
    {
        if (!isFirstFloorGenerated && currentFloor == 1)
        {
            ResetGunPreferences(); // Reset player prefs for guns when the first floor is generated
            isFirstFloorGenerated = true; // Ensure the reset happens only once
        }

        floorPositions = new HashSet<Vector2Int>(); // Clear the current floor positions
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>(); // Set up potential room positions
        Debug.Log("Generating corridors...");
        CreateCorridors(floorPositions, potentialRoomPositions); // Create corridors and add positions to floor
        Debug.Log($"Corridor generation completed. Floor positions: {string.Join(", ", floorPositions.Select(p => p.ToString()))}");
        Debug.Log("Generating rooms...");
        GenerateRooms(potentialRoomPositions); // Generate rooms based on corridor ends
        Debug.Log($"Room generation completed. Room positions: {string.Join(", ", potentialRoomPositions.Select(p => p.ToString()))}");
        // Show floor notification with effects
        StartCoroutine(ShowFloorNotification());
    }

    /// <summary>
    /// Handles the fade-in effect for the floor notification UI, including moving and scaling the text.
    /// </summary>
    public IEnumerator ShowFloorNotification()
    {
        // Initialize CanvasGroup and TextMeshProUGUI
        floorNotificationCanvasGroup.alpha = 0; // Start with alpha at 0 for fade-in effect
        TextMeshProUGUI tmpText = floorNotificationText; // Ensure this is assigned correctly
        tmpText.text = $"Floor {currentFloor}";
        floorNotificationText.gameObject.SetActive(true); // Ensure the text is active

        RectTransform rt = tmpText.GetComponent<RectTransform>();
        Vector2 startSize = rt.sizeDelta; // Keep the original size as the start size
        Vector2 targetSize = startSize * 0.5f; // 50% reduction for a less drastic shrinkage

        // Use the manually set start position
        Vector2 startPos = startNotificationPosition;
        Vector2 endPos = new Vector2(finalPositionX, finalPositionY); // Final position from sliders

        // Clamp end position to screen bounds
        endPos.x = Mathf.Clamp(endPos.x, startSize.x / 2, Screen.width - startSize.x / 2);
        endPos.y = Mathf.Clamp(endPos.y, startSize.y / 2, Screen.height - startSize.y / 2);

        float fadeDuration = 2f; // Duration of the fade-in effect
        float moveDuration = 2f; // Duration of the move
        float scaleDuration = 1f; // Duration for scaling down
        float elapsed = 0f;

        // Reset position to initial before starting animation
        rt.anchoredPosition = startPos;
        rt.sizeDelta = startSize;
        tmpText.fontSize = 30; // Set an initial font size, adjust if needed

        // Fade in the text
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            floorNotificationCanvasGroup.alpha = t;
            yield return null;
        }

        // Ensure fully opaque at the end of fade
        floorNotificationCanvasGroup.alpha = 1;

        // Move the text to the target position and scale it down
        Vector2 initialSize = rt.sizeDelta; // Store the initial size
        Vector2 initialPos = rt.anchoredPosition; // Store the initial position
        elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            // Interpolate position
            rt.anchoredPosition = Vector2.Lerp(initialPos, endPos, t);

            // Interpolate size and font size
            rt.sizeDelta = Vector2.Lerp(initialSize, targetSize, t);
            tmpText.fontSize = Mathf.RoundToInt(Mathf.Lerp(30, 15, t)); // Adjust starting and ending font sizes

            yield return null;
        }

        // Ensure the final position and size are set
        rt.anchoredPosition = endPos;
        rt.sizeDelta = targetSize;
        tmpText.fontSize = Mathf.RoundToInt(15); // Final font size

        // Ensure the CanvasGroup alpha remains at 1
        floorNotificationCanvasGroup.alpha = 1;

        // Ensure the text stays visible
        floorNotificationText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Generates the rooms in the dungeon by selecting potential room positions, 
    /// which are primarily located at the ends of the generated corridors.
    /// This includes room generation for regular rooms, as well as a reserved boss room.
    /// </summary>
    private void GenerateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        if (potentialRoomPositions.Count == 0)
        {
            Debug.LogError("No potential room positions available.");
            return;
        }

        // Ensure the boss room position is selected and reserved
        Vector2Int bossRoomPosition = new Vector2Int(0, 0); //Desired square room position here if not procedural
        roomContentGenerator.SetBossRoomPosition(bossRoomPosition); // Pass the boss room position to RoomContentGenerator

        // Generate a square boss room at the specified position
        HashSet<Vector2Int> bossRoomFloor = CreateSquareRoom(bossRoomPosition, 20); //5x5 square room
        floorPositions.UnionWith(bossRoomFloor); // Add boss room floor positions to overall floor positions

        // Remove boss room position from potential rooms
        potentialRoomPositions.Remove(bossRoomPosition);

        // Generate other rooms
        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);
        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);

        CreateRoomsAtDeadEnd(deadEnds, roomPositions);
        floorPositions.UnionWith(roomPositions);

        tilemapVisualizer.PaintFloorTiles(floorPositions);// Paint the floor tiles in the dungeon
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);// Create walls around the dungeon
    }

    /// <summary>
    /// Creates a square-shaped room at the specified center position and size.
    /// </summary>
    private HashSet<Vector2Int> CreateSquareRoom(Vector2Int centerPosition, int size)
    {
        // Loop through a square area around the center position
        HashSet<Vector2Int> squareRoomPositions = new HashSet<Vector2Int>();

        for (int x = -size / 2; x < size / 2; x++)
        {
            for (int y = -size / 2; y < size / 2; y++)
            {
                squareRoomPositions.Add(centerPosition + new Vector2Int(x, y)); // Add each tile to the room
            }
        }

        return squareRoomPositions;
    }


    /// <summary>
    /// Coroutine to generate rooms asynchronously.
    /// </summary>
    private IEnumerator GenerateRoomsCoroutine(HashSet<Vector2Int> potentialRoomPositions)
    {
        yield return new WaitForSeconds(2); // Wait for 2 seconds before starting room generation
        tilemapVisualizer.Clear(); // Clear the previous dungeon visualization
        GenerateRooms(potentialRoomPositions); // Generate rooms with given positions
        DungeonData data = new DungeonData
        {
            roomsDictionary = this.roomsDictionary,
            corridorPositions = this.corridorPositions,
            floorPositions = this.floorPositions
        };
        OnDungeonFloorReady?.Invoke(data); // Notify listeners that dungeon floor is ready
    }

    /// <summary>
    /// Creates rooms at dead-end points in the dungeon.
    /// This is done by randomly selecting a dead-end position and running a random walk to create the room.
    /// </summary>
    private void CreateRoomsAtDeadEnd(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
        // Iterate over each dead-end position in the dungeon
        foreach (var position in deadEnds)
        {
            // Check if the position is not already part of an existing room floor
            if (roomFloors.Contains(position) == false)
            {
                // Run gen algorithm from the dead-end position to generate a room
                var room = RunRandomWalk(randomWalkParameters, position);
                SaveRoomData(position, room);  // Save the generated room's position and floor tiles to the room data
                roomFloors.UnionWith(room); // Add the generated room's floor tiles to the overall collection of room floors
            }
        }
    }


    /// <summary>
    /// Finds all the dead-end points in the dungeon based on floor positions.
    /// Dead ends are positions surrounded by walls with only one path leading to them.
    /// </summary>
    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
         // Initialize a list to store all the dead-end positions
        List<Vector2Int> deadEnds = new List<Vector2Int>();
        // Iterate over each position on the floor
        foreach (var position in floorPositions)
        {
            int neighboursCount = 0;
            foreach (var direction in Direction2D.cardinalDirectionsList)  // Check all cardinal directions (up, down, left, right) for neighboring positions
            {
                if (floorPositions.Contains(position + direction))  // If the neighboring position exists in floorPositions, increment the neighbour count
                    neighboursCount++;
            }
            if (neighboursCount == 1)  // A position is considered a dead-end if it only has 1 neighbor
                deadEnds.Add(position);
        }
        return deadEnds;
    }


    /// <summary>
    /// Generates rooms in the dungeon using random walk at selected potential positions.
    /// This function uses a percentage to determine how many rooms to generate.
    /// </summary>
    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>(); 
        int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);  // Calculate how many rooms to generate based on the roomPercent

        List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();  // Randomly select positions for the rooms based on the calculated count
        ClearRoomData(); 
        foreach (var roomPosition in roomsToCreate)  // Iterate through the selected room positions and create a room at each position
        {
            var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);  // Use random walk to generate the room floor at the given position

            SaveRoomData(roomPosition, roomFloor);  // Save the generated room data, such as its position and floor layout
            roomPositions.UnionWith(roomFloor); // Add the generated room's floor positions to the overall room positions
        }
        return roomPositions;
    }


    /// <summary>
    /// Clears the data related to previously generated rooms (room positions and colors).
    /// </summary>
    private void ClearRoomData()
    {
        roomsDictionary.Clear();
        roomColors.Clear();
    }

    /// <summary>
    /// Saves data about a room including its position and the set of floor tiles that make up the room.
    /// </summary>
    private void SaveRoomData(Vector2Int roomPosition, HashSet<Vector2Int> roomFloor)
    {
        roomsDictionary[roomPosition] = roomFloor;
        roomColors.Add(UnityEngine.Random.ColorHSV());
    }

    /// <summary>
    /// Creates corridors in the dungeon by adding walkable paths between rooms or sections.
    /// Corridors are placed randomly between various positions to connect rooms.
    /// </summary>
    private void CreateCorridors(HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> potentialRoomPositions)
    {
        var currentPosition = startPosition;
        potentialRoomPositions.Add(currentPosition);

        for (int i = 0; i < corridorCount; i++)
        {
            var corridor = ProceduralGenerationAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);
            currentPosition = corridor[corridor.Count - 1];
            potentialRoomPositions.Add(currentPosition);
            floorPositions.UnionWith(corridor);
        }
        corridorPositions = new HashSet<Vector2Int>(floorPositions);
    }

    /// <summary>
    /// Called when the boss is defeated. It increases the difficulty of the dungeon by adding more corridors.
    /// </summary>
    public void OnBossDefeated()
    {
        bossKills++; // Increment boss kill count
        corridorCount += 6; // Increase corridor count by 4 each time

        // Increment the floor number
        currentFloor++;

        // Trigger the OnBossKilled event if needed
        OnBossKilled?.Invoke();

        // Save gun ammo data before regenerating the dungeon
        WeaponManager.instance.SaveAllGunAmmoData();

        // Regenerate the dungeon
        StartCoroutine(RegenerateDungeon());
    }


    /// <summary>
    /// Resets gun preferences to their default state, clearing stored data in PlayerPrefs.
    /// </summary>
    private void ResetGunPreferences()
    {
        PlayerPrefs.DeleteKey(WeaponManager.UnlockedGunsKey); // Remove the unlocked guns key
        PlayerPrefs.DeleteKey(WeaponManager.EquippedGunIndicesKey); // Remove the equipped guns key
        PlayerPrefs.DeleteKey(WeaponManager.PickedUpWeaponsKey); // Remove the picked-up weapons key
        PlayerPrefs.DeleteKey(WeaponManager.GunAmmoKey); // Remove gun ammo key
        PlayerPrefs.DeleteKey(WeaponManager.GunClipsKey); // Remove gun clips key
        PlayerPrefs.DeleteKey(WeaponManager.GunClipAmmoKey); // Remove gun clip ammo key
        PlayerPrefs.Save(); // Save changes
        Debug.Log("Player gun preferences reset.");
    }


    /// <summary>
    /// Regenerates the dungeon after a boss kill or other event. This clears the current dungeon and creates a new one.
    /// </summary>
    public IEnumerator RegenerateDungeon()
    {
        yield return new WaitForSeconds(1); // A brief delay to simulate transition

        // Clear current dungeon
        tilemapVisualizer.Clear();

        // Run procedural generation with updated corridor count
        RunProceduralGeneration();

        // Restore guns for the new dungeon (this currently restores all ammo)
        WeaponManager.instance.RestoreSomeGunAmmoData();

        // Show floor notification with effects
        StartCoroutine(ShowFloorNotification());
    }


    /// <summary>
    /// Visualizes rooms and corridors using Gizmos, which helps with debugging and visualization in the editor.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (showRoomGizmo)
        {
            int i = 0;
            foreach (var roomData in roomsDictionary)
            {
                if (i >= roomColors.Count)
                    break;

                Color color = roomColors[i];
                color.a = 0.5f;
                Gizmos.color = color;
                Gizmos.DrawSphere((Vector2)roomData.Key, 0.5f);
                foreach (var position in roomData.Value)
                {
                    Gizmos.DrawCube((Vector2)position + new Vector2(0.5f, 0.5f), Vector3.one);
                }
                i++;
            }
        }
        if (showCorridorsGizmo && corridorPositions != null)
        {
            Gizmos.color = Color.magenta;
            foreach (var corridorTile in corridorPositions)
            {
                Gizmos.DrawCube((Vector2)corridorTile + new Vector2(0.5f, 0.5f), Vector3.one);
            }
        }
    }


    /// <summary>
    /// Resets the dungeon for a new game by clearing relevant data and resetting parameters.
    /// </summary>
    public void ResetForNewGame()
{
    // Reset dungeon parameters for a new game
    currentFloor = 1; // Reset the floor number
    corridorCount = 5; // Reset corridor count or adjust as needed
    isFirstFloorGenerated = false; // Allow first floor to regenerate
    roomsDictionary.Clear(); // Clear previous room data if necessary
    floorPositions = new HashSet<Vector2Int>(); // Clear floor positions
    corridorPositions = new HashSet<Vector2Int>(); // Clear corridor positions
}


}