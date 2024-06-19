using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Subscribe to player death event
        PlayerController.OnPlayerDeath += HandlePlayerDeath;
    }

    void HandlePlayerDeath()
    {
        // Handle player death logic, like respawning or game over
        Debug.Log("Handling player death in GameManager");

        // Trigger completion of DieOnce challenge
        ChallengeManager.instance.CompleteChallenge("DieOnce");
    }

    void Update()
    {
        // Example: Check for walking distance
        float currentDistance = GetPlayerDistance(); // Replace with your actual distance calculation
        if (currentDistance >= 1000f)
        {
            // Trigger completion of WalkDistance challenge
            ChallengeManager.instance.CompleteChallenge("WalkDistance", currentDistance);
        }
    }

    float GetPlayerDistance()
    {
        // Replace with your actual distance calculation logic (e.g., using player's movement script)
        return 1500f; // Example distance for testing
    }

    void OnDestroy()
    {
        // Unsubscribe from player death event
        PlayerController.OnPlayerDeath -= HandlePlayerDeath;
    }
}
