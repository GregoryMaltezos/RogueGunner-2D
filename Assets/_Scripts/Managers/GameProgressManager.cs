using UnityEngine;
using System.Collections.Generic;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager instance;

    private List<int> unlockedWeapons = new List<int>();
    /// <summary>
    /// Initializes the singleton instance of GameProgressManager and ensures the object persists across scenes.
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Initializes unlocked weapons on game start.
    /// </summary>
    void Start()
    {
        InitializeUnlockedWeapons();
    }
    /// <summary>
    /// Initializes unlocked weapons by checking completed challenges and unlocking corresponding weapons.
    /// </summary>
    void InitializeUnlockedWeapons()
    {
        Debug.Log("Initializing unlocked weapons...");

        // Ensure challenges are loaded before initializing unlocked weapons
        ChallengeManager.instance.LoadChallenges();

        // Go through the list of challenges and unlock weapons for completed challenges
        foreach (ChallengeManager.Challenge challenge in ChallengeManager.instance.challenges)
        {
            Debug.Log($"Checking challenge: {challenge.challengeId}, completed: {challenge.completed}");
            if (challenge.completed)
            {
                UnlockWeapon(challenge.weaponIndexToUnlock);// Unlock weapon if the challenge is completed
            }
        }

        Debug.Log($"Total unlocked weapons: {unlockedWeapons.Count}");
    }
    /// <summary>
    /// Retrieves the list of unlocked weapons.
    /// </summary>
    /// <returns>A list of unlocked weapon indices.</returns>
    public List<int> GetUnlockedWeapons()
    {
        // Return the list of unlocked weapon indices
        return new List<int>(unlockedWeapons);
    }
    /// <summary>
    /// Unlocks a weapon by its index if it's not already unlocked.
    /// </summary>
    /// <param name="weaponIndex">The index of the weapon to unlock.</param>
    public void UnlockWeapon(int weaponIndex)
    {
        if (!unlockedWeapons.Contains(weaponIndex)) // Check if the weapon is already unlocked
        {
            unlockedWeapons.Add(weaponIndex); // Add weapon to the unlocked list
            Debug.Log($"Weapon {weaponIndex} unlocked.");

            // Notify the chest to refresh the available weapons list
            Chest chest = FindObjectOfType<Chest>();
            if (chest != null)
            {
                chest.RefreshAvailableWeapons();// Update chest's available weapons list
            }
        }
    }

    /// <summary>
    /// Completes a challenge and unlocks the corresponding weapon if the challenge was not previously completed.
    /// </summary>
    /// <param name="challengeId">The ID of the challenge to complete.</param>
    public void CompleteChallengeAndUnlockWeapon(string challengeId) 
    {
        // Find the challenge by its ID
        ChallengeManager.Challenge challengeToComplete = ChallengeManager.instance.challenges.Find(challenge => challenge.challengeId == challengeId);
        if (challengeToComplete != null && !challengeToComplete.completed) // Check if the challenge exists and is not already completed
        {
            ChallengeManager.instance.CompleteChallenge(challengeId); // Complete the challenge and unlock the weapon
            UnlockWeapon(challengeToComplete.weaponIndexToUnlock);
        }
        else 
        {
            if (challengeToComplete == null) // Handle cases where the challenge is either not found or already completed
            {
                Debug.LogWarning("Challenge not found: " + challengeId);
            }
            else if (challengeToComplete.completed)
            {
                Debug.LogWarning("Challenge already completed: " + challengeId);
            }
        }
    }
}
