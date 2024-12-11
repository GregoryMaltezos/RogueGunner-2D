using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMenu : MonoBehaviour
{
    public Button tryAgainButton;
    public Button quitButton;
    public Canvas deathCanvas; // Reference to the Canvas
    public GameObject deathPanel; // Reference to the Panel inside the Canvas

    private CorridorFirstDungeonGenerator dungeonGenerator; // Reference to the dungeon generator
    private PlayerHealth playerHealth;  // Reference to the player's health system

    /// <summary>
    /// Initializes the Death Menu. Sets up button listeners and subscribes to events.
    /// Finds the necessary dungeon generator and player health components.
    /// </summary>
    void Start()
    {
        // Attach the button click event listeners to their respective methods
        tryAgainButton.onClick.AddListener(OnTryAgain);
        quitButton.onClick.AddListener(QuitToMainMenu);
        PlayerController.OnPlayerDeath += ShowDeathMenu; // Subscribe to the event that triggers when the player dies
         // Attempt to find the dungeon generator and player health components in the scene
        dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>();
        if (dungeonGenerator == null)
        {
            Debug.LogError("CorridorFirstDungeonGenerator not found in the scene.");
        }

        // Find the PlayerHealth script in the scene
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth not found in the scene.");
        }

        // Ensure the death menu is hidden when the game starts
        if (deathCanvas != null)
        {
            deathCanvas.enabled = false;  // Disable the death canvas initially
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(false); // Disable the death panel initially
        }
    }
    /// <summary>
    /// Unsubscribes from the player death event to prevent memory leaks when this object is destroyed.
    /// </summary>
    void OnDestroy()
    {
       
        PlayerController.OnPlayerDeath -= ShowDeathMenu;
    }

    /// <summary>
    /// Checks if the player has died by inspecting their health. If dead, shows the death menu.
    /// </summary>
    void Update()
    {
        // Check if the player's health is zero (i.e., they are dead)
        if (playerHealth != null && playerHealth.currentHealth <= 0 && !playerHealth.isDead)
        {
            ShowDeathMenu();  // Trigger the death menu if the player's health is zero
        }
    }

    /// <summary>
    /// Activates the death menu UI, indicating that the player has died.
    /// </summary>
    public void ShowDeathMenu()
    {
        // Log message to confirm this method is called
        Debug.Log("Death Menu is being shown!");

        // Show the death menu (Canvas and Panel should be enabled)
        if (deathCanvas != null)
        {
            deathCanvas.enabled = true; // Enable the Canvas
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(true); // Activate the Panel
        }
    }

    /// <summary>
    /// Called when the player clicks the "Try Again" button. Resets the player's state and dungeon.
    /// </summary>
    public void OnTryAgain()
    {
        // Hide the death menu before restarting
        if (deathCanvas != null)
        {
            deathCanvas.enabled = false;
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        // Start the reset process with a brief delay
        StartCoroutine(ResetPlayerWithDelay());
    }

    /// <summary>
    /// Resets the player's health and state with a brief delay after clicking "Try Again".
    /// </summary>
    private IEnumerator ResetPlayerWithDelay()
    {
        // Wait for a short delay (e.g., 1.5 seconds) to give time for any animation
        yield return new WaitForSeconds(1.5f);

        // Optionally reset the dungeon or game state
        if (dungeonGenerator != null)
        {
            dungeonGenerator.ResetForNewGame();  // Reset the dungeon to the starting floor
            StartCoroutine(RegenerateDungeon()); // Start the regeneration of the dungeon
        }

        // Reset player's health and death status for the fresh start
        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth; // Set the player's health back to max
            playerHealth.isDead = false;  // Mark the player as alive
        }  
    }

    /// <summary>
    /// Regenerates the dungeon after the player chooses to try again.
    /// </summary>
    private IEnumerator RegenerateDungeon()
    {
        // Wait briefly to allow death animation or effects to finish
        yield return new WaitForSeconds(0.6f); // Adjust this delay based on animation length

        // Clear the current dungeon
        dungeonGenerator.tilemapVisualizer.Clear(); // Clear the dungeon's visual representation

        // Regenerate the dungeon from scratch
        dungeonGenerator.StartDungeonGeneration();

        //show floor notification with effects
        StartCoroutine(dungeonGenerator.ShowFloorNotification());
        // Reset player's health and death status
        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;  // Reset the player's health to max
            playerHealth.isDead = false;  // Mark the player as alive again
        }

    }




    /// <summary>
    /// Quits the game and returns to the main menu.
    /// </summary>
    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene"); // Load the main menu scene
    }
}
