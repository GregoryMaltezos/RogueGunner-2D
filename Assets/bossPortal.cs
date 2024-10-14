using System.Collections;
using UnityEngine;

public class bossPortal : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private CorridorFirstDungeonGenerator dungeonGenerator;
    private FadeManager fadeManager; // Reference to the Fade Manager

    private void Start()
    {
        // Automatically find the player by tag
        if (GameObject.FindWithTag("Player") == null)
        {
            Debug.LogError("Player not found. Make sure the Player has the 'Player' tag.");
        }

        // Find the dungeon generator in the scene (if it exists)
        dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>();
        if (dungeonGenerator == null)
        {
            Debug.LogError("Dungeon Generator not found in the scene.");
        }

        // Find the FadeManager in the scene
        fadeManager = FindObjectOfType<FadeManager>();
        if (fadeManager == null)
        {
            Debug.LogError("FadeManager not found in the scene.");
        }
    }

    private void Update()
    {
        // Check if player is near and presses the "E" key
        if (isPlayerNearby)
        {
         //   Debug.Log("Player is nearby and can interact.");
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("E key pressed. Starting fade process.");
                StartCoroutine(FadeToBlackAndProceed());
            }
        }
    }

    private IEnumerator FadeToBlackAndProceed()
    {
        Debug.Log("FadeToBlackAndProceed called"); // Debugging line

        // Ensure the transition canvas is active
        if (fadeManager != null && fadeManager.transitionCanvas != null)
        {
            Debug.Log("Activating transition canvas");
            fadeManager.transitionCanvas.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("FadeManager or transitionCanvas is null");
        }

        // Fade to black
        if (fadeManager != null)
        {
            yield return fadeManager.FadeToBlack();
        }

        // Call the function to go to the next floor
        GoToNextFloor();

        // Wait for a moment before fading back in
        yield return new WaitForSeconds(1f); // Optional wait time

        // Fade back to normal
        if (fadeManager != null)
        {
            yield return fadeManager.FadeToClear();
            Destroy(gameObject);
        }
    }


    private void GoToNextFloor()
    {
        Debug.Log("Player interacted with portal, generating the next floor!");

        if (dungeonGenerator != null)
        {
            dungeonGenerator.OnBossDefeated();
        }
        else
        {
            Debug.LogError("Dungeon Generator reference is missing!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player is near the portal.");
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player left the portal area.");
            isPlayerNearby = false;
        }
    }
}
