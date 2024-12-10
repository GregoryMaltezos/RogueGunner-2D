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

    void Start()
    {
        // Assuming these buttons are children of the death menu panel
        tryAgainButton.onClick.AddListener(OnTryAgain);
        quitButton.onClick.AddListener(QuitToMainMenu);
        PlayerController.OnPlayerDeath += ShowDeathMenu;
        // Find the CorridorFirstDungeonGenerator in the scene
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
    void OnDestroy()
    {
        // Unsubscribe from the event when the object is destroyed to prevent memory leaks
        PlayerController.OnPlayerDeath -= ShowDeathMenu;
    }
    void Update()
    {
        // Check if the player's health is zero (i.e., they are dead)
        if (playerHealth != null && playerHealth.currentHealth <= 0 && !playerHealth.isDead)
        {
            ShowDeathMenu();  // Trigger the death menu if the player's health is zero
        }
    }

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

        // Start a coroutine to handle the delay before resetting the player
        StartCoroutine(ResetPlayerWithDelay());
    }

    private IEnumerator ResetPlayerWithDelay()
    {
        // Wait for a brief moment (e.g., 1.5 seconds) before resetting the player
        yield return new WaitForSeconds(1.5f); // Adjust this value as needed to match your animation or desired delay

        // Optionally, regenerate the dungeon or reset the game state
        if (dungeonGenerator != null)
        {
            dungeonGenerator.ResetForNewGame();  // Reset the dungeon to floor 1
            StartCoroutine(RegenerateDungeon());
        }

        // Reset player's health and death status for a fresh start
        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;  // Reset the player's health to max
            playerHealth.isDead = false;  // Mark the player as alive again
        }

        // Optionally, if you want to reset the player's position, you can reset it to the starting point here.
        // Example: player.transform.position = dungeonGenerator.GetStartingPosition();
    }

    private IEnumerator RegenerateDungeon()
    {
        // Wait a moment to allow for death animation to finish
        yield return new WaitForSeconds(0.6f); // Adjust this delay based on animation length

        // Clear the current dungeon
        dungeonGenerator.tilemapVisualizer.Clear();

        // Start dungeon generation using the public method
        dungeonGenerator.StartDungeonGeneration();

        // Optionally, show floor notification with effects
        StartCoroutine(dungeonGenerator.ShowFloorNotification());
        // Reset player's health and death status for a fresh start
        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;  // Reset the player's health to max
            playerHealth.isDead = false;  // Mark the player as alive again
        }

    }





    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
