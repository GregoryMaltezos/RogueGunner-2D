using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
   // [SerializeField]
   // private GameObject mainMenu; // Reference to the main menu UI
    [SerializeField]
    private CorridorFirstDungeonGenerator dungeonGenerator; // Reference to the dungeon generator
    private Vector3 startPosition; // To store the player's initial spawn position
    private int killsInCurrentWindow = 0;  // Count kills within the time window
    private float killWindowTime = 3f;     // Time limit for the challenge (in seconds)
    private float killWindowTimer = 0f;    // Timer to track time
    private bool isKillWindowActive = false;  // Flag to check if the timer is running
    private Vector3 lastPosition; // To store the player's last position for tracking distance
    private float totalDistanceTraveled = 0f;  // Total distance traveled by the player


    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Called when the script instance is being loaded. Subscribes to player death event.
    /// </summary>
    void Start()
    {
        // Subscribe to player death event
        PlayerController.OnPlayerDeath += HandlePlayerDeath;
        PlayerController.OnPlayerWalkDistance += HandlePlayerWalkDistance;
        // ShowMainMenu(); // Show the main menu when the game starts
    }
    void HandlePlayerWalkDistance(float distance)
    {
        totalDistanceTraveled += distance;
        Debug.Log($"Distance Traveled: {totalDistanceTraveled}");

        if (totalDistanceTraveled >= 100f)
        {
            ChallengeManager.instance.CompleteChallenge("WalkDistance", totalDistanceTraveled);
        }
    }

    /// <summary>
    /// Starts the game by generating the dungeon.
    /// </summary>
    public void StartGame()
    {
      
        dungeonGenerator.GenerateDungeon(); // Start dungeon generation
    }

    /// <summary>
    /// Displays the main menu UI (currently commented out).
    /// </summary>
    private void ShowMainMenu()
    {
       // mainMenu.SetActive(true);
    }

    /// <summary>
    /// Hides the main menu UI (currently commented out).
    /// </summary>
    private void HideMainMenu()
    {
       // mainMenu.SetActive(false);
    }

    /// <summary>
    /// Handles logic when the player dies, including logging and challenge completion.
    /// </summary>
    void HandlePlayerDeath()
    {
        Debug.Log("Handling player death in GameManager");
        // Handle player death logic, like respawning or game over

        // Trigger completion of DieOnce challenge
        ChallengeManager.instance.CompleteChallenge("DieOnce");
    }

    /// <summary>
    /// Called once per frame. Monitors player distance and checks for challenge completion.
    /// </summary>
    void Update()
    {
        // Track the distance traveled by the player in the procedural world
        float currentDistance = GetPlayerDistance();

        // If the player has traveled enough distance for a challenge, complete the challenge
        if (currentDistance >= 10f)
        {
            // Trigger completion of WalkDistance challenge
            ChallengeManager.instance.CompleteChallenge("WalkDistance", currentDistance);
        }

        // Update the last position for the next frame
        lastPosition = transform.position;
    }

      /// <summary>
    /// Calculates the total distance traveled by the player.
    /// </summary>
    /// <returns>Distance traveled by the player.</returns>
    float GetPlayerDistance()
    {
        // Calculate the distance from the last position to the current position
        float distanceTraveled = Vector3.Distance(lastPosition, transform.position);
        totalDistanceTraveled += distanceTraveled;

        return totalDistanceTraveled;
    }


    /// <summary>
    /// Returns the player's spawn position at the start of the game session.
    /// </summary>
    /// <returns>The spawn position of the player.</returns>
    Vector3 GetPlayerSpawnPosition()
    {
       
        return transform.position;  // Use the current position as the spawn position at the start of the game.
    }

    /// <summary>
    /// Called when the script is destroyed. Unsubscribes from player death event.
    /// </summary>
    void OnDestroy()
    {
        // Unsubscribe from player death event
        PlayerController.OnPlayerDeath -= HandlePlayerDeath;
    }

    /// <summary>
    /// Called when an enemy is killed. Tracks kills and starts a kill window for challenge completion.
    /// </summary>
    public void OnEnemyKilled()
    {
        if (!isKillWindowActive)
        {
            // Start the timer window if not active
            isKillWindowActive = true;
            killsInCurrentWindow = 1;  // First kill starts the window
            StartCoroutine(KillWindowTimer());  // Start the timer coroutine
        }
        else
        {
            // Increment kill count within the window
            killsInCurrentWindow++;
        }

        // Check if the challenge should be completed
        if (killsInCurrentWindow >= 3)
        {
            CompleteChallenge();
        }
    }

    /// <summary>
    /// Timer coroutine that runs for the duration of the kill window.
    /// </summary>
    /// <returns>Waits until the timer runs out.</returns>
    private IEnumerator KillWindowTimer()
    {
        killWindowTimer = 0f;
        while (killWindowTimer < killWindowTime)
        {
            killWindowTimer += Time.deltaTime;  // Increase the timer every frame
            yield return null;
        }

        // Once time runs out, finalize the window
        EndKillWindow();
    }

    /// <summary>
    /// Ends the kill window after the specified time has elapsed, resetting necessary states.
    /// </summary>
    private void EndKillWindow()
    {
        isKillWindowActive = false;

        // If kills were not completed within the window, reset the state
        if (killsInCurrentWindow < 3)
        {
            killsInCurrentWindow = 0;
            Debug.Log("Failed to complete the challenge within the time window.");
        }
    }

    /// <summary>
    /// Completes the 'Defeat Enemies Quickly' challenge if conditions are met.
    /// </summary>
    private void CompleteChallenge()
    {
        // Ensure the kill window hasn't expired
        if (!isKillWindowActive)
        {
            Debug.Log("Kill window expired. Challenge not completed.");
            return;
        }

        // Call the ChallengeManager to complete the challenge
        ChallengeManager.instance.CompleteChallenge("DefeatEnemiesQuickly", 3f, killsInCurrentWindow, killWindowTimer);

        // Reset the window state after completing the challenge
        killsInCurrentWindow = 0;
        isKillWindowActive = false;
        Debug.Log("Challenge 'Defeat Enemies Quickly' completed!");
    }


}