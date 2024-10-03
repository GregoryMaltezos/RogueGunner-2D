using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenu; // Reference to the main menu UI
    [SerializeField]
    private CorridorFirstDungeonGenerator dungeonGenerator; // Reference to the dungeon generator

    void Start()
    {
        // Subscribe to player death event
        PlayerController.OnPlayerDeath += HandlePlayerDeath;

        ShowMainMenu(); // Show the main menu when the game starts
    }

    // Method to start the game
    public void StartGame()
    {
        HideMainMenu(); // Hide the main menu
        dungeonGenerator.GenerateDungeon(); // Start dungeon generation
    }

    // Show the main menu UI
    private void ShowMainMenu()
    {
        mainMenu.SetActive(true);
    }

    // Hide the main menu UI
    private void HideMainMenu()
    {
        mainMenu.SetActive(false);
    }

    // Handle player death logic
    void HandlePlayerDeath()
    {
        Debug.Log("Handling player death in GameManager");
        // Handle player death logic, like respawning or game over

        // Trigger completion of DieOnce challenge
        ChallengeManager.instance.CompleteChallenge("DieOnce");
    }

    void Update()
    {
        // Example: Check for walking distance
        float currentDistance = GetPlayerDistance(); // Replace with your actual distance calculation
        if (currentDistance >= 1000f)
        {
            // Trigger completion of WalkDistance challenge
            ChallengeManager.instance.CompleteChallenge("WalkDistance", currentDistance);
        }
    }

    float GetPlayerDistance()
    {
        // Replace with your actual distance calculation logic (e.g., using player's movement script)
        return 1500f; // Example distance for testing
    }

    void OnDestroy()
    {
        // Unsubscribe from player death event
        PlayerController.OnPlayerDeath -= HandlePlayerDeath;
    }
}
