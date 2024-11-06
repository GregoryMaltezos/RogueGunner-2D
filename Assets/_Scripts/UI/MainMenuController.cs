using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // References to the main menu and settings panels
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    // Reference to GunUIManager and the PlayerAmmo UI element
    private GunUIManager gunUIManager;  // Reference to GunUIManager
    private GameObject playerAmmo;      // Reference to PlayerAmmo

    void Start()
    {
        // Automatically find GunUIManager and its PlayerAmmo child if they exist
        FindGunUIManager();

        // Adjust visibility based on current scene
        AdjustPlayerAmmoVisibility();
    }

    // Function to start the game (as in your original code)
    public void StartGame()
    {
        // Ensure PlayerAmmo is enabled before starting the game scene
        EnablePlayerAmmo();

        // Load the game scene (replace "RoomContent" with your game scene)
        Debug.Log("Start Game");
        SceneManager.LoadScene("RoomContent");
    }

    // Function to open the settings menu
    public void OpenSettings()
    {
        // Disable the main menu panel and enable the settings panel
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // Function to go back to the main menu from the settings menu
    public void BackToMainMenu()
    {
        // Disable the settings panel and enable the main menu panel
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Function to quit the game (as in your original code)
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    // Function to find the GunUIManager and its PlayerAmmo child
    private void FindGunUIManager()
    {
        // Look for GunUIManager in the scene (it persists across scenes)
        gunUIManager = FindObjectOfType<GunUIManager>();
        if (gunUIManager != null)
        {
            // If found, get the reference to PlayerAmmo (child of GunUIManager)
            playerAmmo = gunUIManager.transform.Find("PlayerAmmo")?.gameObject;

            if (playerAmmo != null)
            {
                Debug.Log("PlayerAmmo found and will be managed based on scene.");
            }
            else
            {
                Debug.LogError("PlayerAmmo not found as a child of GunUIManager.");
            }
        }
        else
        {
            Debug.LogError("GunUIManager not found in the scene.");
        }
    }

    // Function to enable PlayerAmmo in the game scene
    private void EnablePlayerAmmo()
    {
        if (playerAmmo != null)
        {
            // Enable PlayerAmmo if it exists and is not already active
            if (!playerAmmo.activeSelf)
            {
                playerAmmo.SetActive(true);
                Debug.Log("PlayerAmmo enabled in the game scene.");
            }
        }
        else
        {
            Debug.LogWarning("PlayerAmmo is null, cannot enable it.");
        }
    }

    // Function to disable PlayerAmmo when in the Main Menu
    private void DisablePlayerAmmo()
    {
        if (playerAmmo != null)
        {
            // Disable PlayerAmmo if it exists
            if (playerAmmo.activeSelf)
            {
                playerAmmo.SetActive(false);
                Debug.Log("PlayerAmmo disabled in the Main Menu.");
            }
        }
        else
        {
            Debug.LogWarning("PlayerAmmo is null, cannot disable it.");
        }
    }

    // Function to adjust PlayerAmmo visibility based on the current scene
    private void AdjustPlayerAmmoVisibility()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "MainMenuSceene")
        {
            // If we're in the Main Menu, disable PlayerAmmo
            DisablePlayerAmmo();
        }
        else
        {
            // In any other scene (e.g., RoomContent), enable PlayerAmmo
            EnablePlayerAmmo();
        }
    }

    // Listener for scene load events to handle visibility when the scene changes
    [RuntimeInitializeOnLoadMethod]
    static void OnSceneLoad()
    {
        // This will run whenever a scene is loaded to adjust visibility
        var controller = FindObjectOfType<MainMenuController>();
        if (controller != null)
        {
            controller.AdjustPlayerAmmoVisibility();
        }
    }
}
