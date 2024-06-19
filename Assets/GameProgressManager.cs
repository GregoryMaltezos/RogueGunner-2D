using UnityEngine;
using System.Collections.Generic;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager instance;

    private List<int> unlockedWeapons = new List<int>();

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

    void Start()
    {
        InitializeUnlockedWeapons();
    }

    void InitializeUnlockedWeapons()
    {
        Debug.Log("Initializing unlocked weapons...");

        // Ensure challenges are loaded before initializing unlocked weapons
        ChallengeManager.instance.LoadChallenges();

        // Initialize unlocked weapons based on completed challenges
        foreach (ChallengeManager.Challenge challenge in ChallengeManager.instance.challenges)
        {
            Debug.Log($"Checking challenge: {challenge.challengeId}, completed: {challenge.completed}");
            if (challenge.completed)
            {
                UnlockWeapon(challenge.weaponIndexToUnlock);
            }
        }

        Debug.Log($"Total unlocked weapons: {unlockedWeapons.Count}");
    }

    public List<int> GetUnlockedWeapons()
    {
        // Return the list of unlocked weapon indices
        return new List<int>(unlockedWeapons);
    }

    public void UnlockWeapon(int weaponIndex)
    {
        if (!unlockedWeapons.Contains(weaponIndex))
        {
            unlockedWeapons.Add(weaponIndex);
            Debug.Log($"Weapon {weaponIndex} unlocked.");
        }
    }

    public void CompleteChallengeAndUnlockWeapon(string challengeId)
    {
        ChallengeManager.Challenge challengeToComplete = ChallengeManager.instance.challenges.Find(challenge => challenge.challengeId == challengeId);
        if (challengeToComplete != null && !challengeToComplete.completed)
        {
            ChallengeManager.instance.CompleteChallenge(challengeId);
            UnlockWeapon(challengeToComplete.weaponIndexToUnlock);
        }
        else
        {
            if (challengeToComplete == null)
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
