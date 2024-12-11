using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject[] canvasesToEnable; // Array of canvases to enable
    public GameObject mainMenuCanvas; // Reference to the main menu canvas
    public MapRuntimeGenerator mapRuntimeGenerator; // Reference to MapRuntimeGenerator


    /// <summary>
    /// Disables specified canvases at the start of the game.
    /// </summary>
    void Start()
    {
        // Disable all specified canvases at the beginning
        DisableCanvases();
    }

    /// <summary>
    /// Starts the game by enabling specified canvases, hiding the main menu canvas,
    /// and initializing dungeon generation.
    /// </summary>
    public void StartGame()
    {
        // Enable the canvases needed for the game
        foreach (GameObject canvas in canvasesToEnable)
        {
            canvas.SetActive(true); // Make each specified canvas visible
        }

        // Hide the main menu canvas if it exists
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }

        // Begin dungeon generation
        InitializeDungeon();
    }

    /// <summary>
    /// Initializes dungeon generation using the MapRuntimeGenerator.
    /// </summary>
    private void InitializeDungeon()
    {
        // Check if MapRuntimeGenerator is assigned
        if (mapRuntimeGenerator != null)
        {
           // mapRuntimeGenerator.StartDungeonGeneration(); // Start dungeon generation
        }
        else
        {
            Debug.LogWarning("MapRuntimeGenerator is not assigned.");
        }
    }

    /// <summary>
    /// Disables all specified canvases at the start of the game.
    /// </summary>
    private void DisableCanvases()
    {
        foreach (GameObject canvas in canvasesToEnable) // Loop through each canvas in the array and disable it
        {
            canvas.SetActive(false); // Ensure each specified canvas is disabled at the start
        }
    }

    /// <summary>
    /// Opens the settings menu. Implementation depends on your UI setup.
    /// </summary>
    public void OpenSettings()
    {
        // Hide the main menu and show the settings panel
        // Assuming you have a settings panel to show
    }

    /// <summary>
    /// Quits the game. Exits the application and logs the action.
    /// </summary>
    public void QuitGame()
    {
        // Exit the application
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
