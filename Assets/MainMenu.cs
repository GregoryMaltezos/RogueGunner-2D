using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject[] canvasesToEnable; // Array of canvases to enable
    public GameObject mainMenuCanvas; // Reference to the main menu canvas
    public MapRuntimeGenerator mapRuntimeGenerator; // Reference to MapRuntimeGenerator

    // Start is called before the first frame update
    void Start()
    {
        // Disable the canvases at the start of the game
        DisableCanvases();
    }

    // Call this method to start the game
    public void StartGame()
    {
        // Enable other canvases after clicking the start button
        foreach (GameObject canvas in canvasesToEnable)
        {
            canvas.SetActive(true); // Enable each specified canvas
        }

        // Optionally disable the main menu canvas if needed
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
        }

        // Directly initialize dungeon generation
        InitializeDungeon();
    }

    private void InitializeDungeon()
    {
        // Check if the mapRuntimeGenerator is set and call the StartDungeonGeneration method
        if (mapRuntimeGenerator != null)
        {
           // mapRuntimeGenerator.StartDungeonGeneration(); // Start dungeon generation
        }
        else
        {
            Debug.LogWarning("MapRuntimeGenerator is not assigned.");
        }
    }

    // Method to disable canvases at the start
    private void DisableCanvases()
    {
        foreach (GameObject canvas in canvasesToEnable)
        {
            canvas.SetActive(false); // Ensure each specified canvas is disabled at the start
        }
    }

    // Call this method to open settings
    public void OpenSettings()
    {
        // Hide the main menu and show the settings panel
        // Assuming you have a settings panel to show
    }

    // Call this method to quit the game
    public void QuitGame()
    {
        // Exit the application
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
