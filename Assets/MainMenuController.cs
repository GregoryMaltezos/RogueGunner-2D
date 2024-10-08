using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // References to the main menu and settings panels
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    // Function to start the game (as in your original code)
    public void StartGame()
    {
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
}
