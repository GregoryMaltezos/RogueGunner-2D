using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMenu : MonoBehaviour
{
    public Button tryAgainButton;
    public Button quitButton;
    public Canvas canvas1; // Changed from CanvasGroup to Canvas
    public Canvas canvas2; // Changed from CanvasGroup to Canvas

    private CorridorFirstDungeonGenerator dungeonGenerator; // Reference to the dungeon generator

    void Start()
    {
        // Assuming these buttons are children of the death menu panel
        tryAgainButton.onClick.AddListener(OnTryAgain);
        quitButton.onClick.AddListener(QuitToMainMenu);

        // Disable canvases at the start
        if (canvas1 != null) canvas1.enabled = false; // Disable the first canvas
        if (canvas2 != null) canvas2.enabled = false; // Disable the second canvas

        // Find the CorridorFirstDungeonGenerator in the scene
        dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>();
        if (dungeonGenerator == null)
        {
            Debug.LogError("CorridorFirstDungeonGenerator not found in the scene.");
        }
    }

    public void ShowDeathMenu()
    {
        // Show the death menu and enable the canvases
        gameObject.SetActive(true);
        if (canvas1 != null) canvas1.enabled = true; // Enable the first canvas
        if (canvas2 != null) canvas2.enabled = true; // Enable the second canvas
    }

    public void OnTryAgain()
    {
        // Disable the canvases before regenerating the dungeon
        if (canvas1 != null) canvas1.enabled = false;
        if (canvas2 != null) canvas2.enabled = false;

        // Check if the dungeon generator is available
        if (dungeonGenerator != null)
        {
            // Optionally reset parameters for a fresh start
            // dungeonGenerator.ResetForNewGame(); // Optional: Reset game state if you have a method for it

            // Regenerate the dungeon immediately
            StartCoroutine(RegenerateDungeon());
        }

        // Enable the canvases after the dungeon is regenerated (in ShowDeathMenu or elsewhere)
    }


    private IEnumerator RegenerateDungeon()
    {
        // Wait a brief moment to allow for UI feedback
        yield return new WaitForSeconds(1f);

        // Clear the current dungeon
        dungeonGenerator.tilemapVisualizer.Clear();

        // Start dungeon generation using the public method
        dungeonGenerator.StartDungeonGeneration(); // Call the public method

        // Optionally, show floor notification with effects
        StartCoroutine(dungeonGenerator.ShowFloorNotification());
    }

    public void QuitToMainMenu()
    {
        // Load the main menu scene (replace "MainMenu" with your actual scene name)
        SceneManager.LoadScene("MainMenuScene");
    }
}
