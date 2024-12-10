using UnityEngine;

public class GameManager : MonoBehaviour
{
   // [SerializeField]
   // private GameObject mainMenu; // Reference to the main menu UI
    [SerializeField]
    private CorridorFirstDungeonGenerator dungeonGenerator; // Reference to the dungeon generator
    private Vector3 startPosition; // To store the player's initial spawn position

    /// <summary>
    /// Called when the script instance is being loaded. Subscribes to player death event.
    /// </summary>
    void Start()
    {
        // Subscribe to player death event
        PlayerController.OnPlayerDeath += HandlePlayerDeath;
        startPosition = GetPlayerSpawnPosition();
        // ShowMainMenu(); // Show the main menu when the game starts
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
        // Check for conditions like player walking distance and update challenges if conditions are met.
        float currentDistance = GetPlayerDistance(); // Calculate the current distance traveled by the player.
        if (currentDistance >= 1000f)
        {
            // Trigger completion of WalkDistance challenge
            ChallengeManager.instance.CompleteChallenge("WalkDistance", currentDistance);
        }
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
    /// Calculates the player's distance (placeholder logic).
    /// </summary>
    /// <returns>Distance traveled by the player.</returns>
    float GetPlayerDistance()
    {
        // Calculate the distance from the player's current position to the initial spawn position.
        return Vector3.Distance(startPosition, transform.position);
    }
    /// <summary>
    /// Called when the script is destroyed. Unsubscribes from player death event.
    /// </summary>
    void OnDestroy()
    {
        // Unsubscribe from player death event
        PlayerController.OnPlayerDeath -= HandlePlayerDeath;
    }
}