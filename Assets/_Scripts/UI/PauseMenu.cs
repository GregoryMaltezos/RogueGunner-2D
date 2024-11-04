using UnityEngine;
using UnityEngine.SceneManagement; // Make sure to include this namespace

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // Reference to the pause menu UI
    public GameObject settingsPanel; // Reference to the settings panel UI
    public GameObject otherCanvas; // Reference to the gameplay canvas
    private bool isPaused = false; // Track the pause state
    private GunController gunController; // Reference to the GunController
    private PlayerController playerController; // Reference to the PlayerController

    public bool IsPaused // Public property to get the pause state
    {
        get { return isPaused; }
    }

    void Start()
    {
        // Initial fetch can be done here, but will be updated in Pause()
        gunController = FindObjectOfType<GunController>();
        playerController = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        // Check if the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume(); // Resume the game
            }
            else
            {
                Pause(); // Pause the game
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu
        settingsPanel.SetActive(false); // Hide the settings panel
        Time.timeScale = 1f; // Resume the game time
        isPaused = false; // Update pause state
        SetCursorState(false); // Hide cursor in gameplay
        EnableGunController(true); // Enable GunController when resuming
        EnablePlayerController(true); // Enable PlayerController when resuming
        if (otherCanvas != null) otherCanvas.SetActive(true); // Enable the gameplay canvas
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true); // Show the pause menu
        settingsPanel.SetActive(false); // Ensure settings panel is hidden
        Time.timeScale = 0f; // Freeze the game time
        isPaused = true; // Update pause state
        SetCursorState(true); // Show cursor in pause menu
        EnableGunController(false); // Disable GunController when paused
        EnablePlayerController(false); // Disable PlayerController when paused
        if (otherCanvas != null) otherCanvas.SetActive(false); // Disable the gameplay canvas
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu UI
        settingsPanel.SetActive(true); // Show the settings panel
    }

    public void BackToPauseMenu()
    {
        settingsPanel.SetActive(false); // Hide the settings panel
        pauseMenuUI.SetActive(true); // Show the pause menu UI
    }

    public void Restart()
    {
        Time.timeScale = 1f; // Reset time scale
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f; // Reset time scale to normal in case the game is paused

        // Load the main menu scene
        SceneManager.LoadScene("MainMenuScene"); // Replace "MainMenuScene" with the name of your main menu scene
    }

    private void SetCursorState(bool isVisible)
    {
        Cursor.visible = isVisible; // Set cursor visibility
    }

    private void EnableGunController(bool enable)
    {
        if (gunController != null)
        {
            gunController.enabled = enable; // Enable or disable the GunController script
        }
    }

    private void EnablePlayerController(bool enable)
    {
        if (playerController != null)
        {
            playerController.enabled = enable; // Enable or disable the PlayerController script
        }
    }
}