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
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FadeToBlackAndProceed());

        }
    }

    private IEnumerator FadeToBlackAndProceed()
    {
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