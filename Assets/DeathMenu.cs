using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMenu : MonoBehaviour
{
    public Button tryAgainButton;
    public Button quitButton;
    public Canvas canvas1; // Reference to the first canvas
    public Canvas canvas2;
    public Canvas canvas3; // Reference to the second canvas

    private CorridorFirstDungeonGenerator dungeonGenerator; // Reference to the dungeon generator

    void Start()
    {
        // Assuming these buttons are children of the death menu panel
        tryAgainButton.onClick.AddListener(OnTryAgain);
        quitButton.onClick.AddListener(QuitToMainMenu);
        if (canvas3 != null) canvas3.enabled = true;
        // Find the CorridorFirstDungeonGenerator in the scene
        dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>();
        if (dungeonGenerator == null)
        {
            Debug.LogError("CorridorFirstDungeonGenerator not found in the scene.");
        }
    }

    public void ShowDeathMenu()
    {
        // Show the death menu and disable other canvases only when the death menu appears
        gameObject.SetActive(true);

        // Disable canvas1 and canvas2 when the death menu appears
        if (canvas1 != null) canvas1.enabled = false; // Disable the first canvas
        if (canvas2 != null) canvas2.enabled = false; // Disable the second canvas
        if (canvas3 != null) canvas3.enabled = false;
    }

    public void OnTryAgain()
    {
        // Re-enable the canvases before starting a new game attempt
        if (canvas1 != null) canvas1.enabled = true;  // Re-enable the first canvas
        if (canvas2 != null) canvas2.enabled = true;  // Re-enable the second canvas
        if (canvas3 != null) canvas3.enabled = true;


        // Check if the dungeon generator is available
        if (dungeonGenerator != null)
        {
            // Optionally reset parameters for a fresh start
            // dungeonGenerator.ResetForNewGame(); // Optional: Reset game state if you have a method for it

            // Regenerate the dungeon immediately
            StartCoroutine(RegenerateDungeon());
        }
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
        if (canvas3 != null) canvas3.enabled = false;
        SceneManager.LoadScene("MainMenuScene");
    }
}
