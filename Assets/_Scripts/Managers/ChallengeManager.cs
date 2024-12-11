using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class ChallengeManager : MonoBehaviour
{
    [System.Serializable]
    public class Challenge
    {
        public string challengeId;
        public string description; // Added description
        public float requiredDistance;
        public int requiredKills;
        public float requiredTime;
        public bool completed;
        public int weaponIndexToUnlock;

        public Challenge(string id, string desc, float distance, int kills, float time, int weaponIndex, bool isCompleted)
        {
            challengeId = id;
            description = desc; // Set description
            requiredDistance = distance;
            requiredKills = kills;
            requiredTime = time;
            weaponIndexToUnlock = weaponIndex;
            completed = isCompleted;
        }
    }


    [System.Serializable]
    public class ChallengeList
    {
        public List<Challenge> challenges;
    }

    public static ChallengeManager instance;

    public List<Challenge> challenges = new List<Challenge>();
    private string saveFilePath;
    private int grenadeKills = 0;

    /// <summary>
    /// Initializes the singleton instance of ChallengeManager and sets the file path for saving challenges.
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "challenges.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Called on start to initialize challenges.
    /// </summary>
    void Start()
    {
        InitializeChallenges();
    }

    /// <summary>
    /// Initializes challenges by loading from a file or creating default challenges.
    /// </summary>
    void InitializeChallenges()
    {
        if (!File.Exists(saveFilePath))
        {
            // Load default challenges from Resources
            TextAsset defaultChallenges = Resources.Load<TextAsset>("challenges");
            if (defaultChallenges != null)
            {
                File.WriteAllText(saveFilePath, defaultChallenges.text);
                Debug.Log("Default challenges file copied to persistent data path.");
            }
            else
            {
                Debug.LogError("Default challenges file not found in Resources.");
            }
        }

        LoadChallenges();

        if (challenges.Count == 0) // If challenges haven't been initialized yet
        {
            // Initialize challenges
            challenges.Add(new Challenge("DieOnce","Die for the first time", 0f, 0, 0f, 1, false)); // No distance or kills requirement
            challenges.Add(new Challenge("WalkDistance","Walk for 1000 meters", 100f, 0, 0f, 2, false)); // Requires walking 1000 units
            challenges.Add(new Challenge("DefeatEnemiesQuickly", "Defeat 3 enemies within 3 seconds", 0f, 3, 3f, 3, false)); // Defeat 3 enemies within 3 seconds
            challenges.Add(new Challenge("GrenadeKills", "Get 15 grenade kills", 0f, 15, 0f, 4, false)); // Get 15 grenade kills
            challenges.Add(new Challenge("DefeatGolem","Defeat the Stone Golem Boss", 0f, 1, 0f, 5, false)); // New challenge to defeat boss with ID 1

            // Save initialized challenges
            SaveChallenges();
        }
    }

    /// <summary>
    /// Marks a specific challenge as completed if its conditions are met.
    /// </summary>
    /// <param name="challengeId">ID of the challenge to complete.</param>
    /// <param name="distance">Distance covered (if applicable).</param>
    /// <param name="kills">Number of kills (if applicable).</param>
    /// <param name="time">Time taken (if applicable).</param>
    public void CompleteChallenge(string challengeId, float distance = 0f, int kills = 0, float time = 0f)
    {
        Challenge challengeToComplete = challenges.Find(challenge => challenge.challengeId == challengeId);
        if (challengeToComplete != null && !challengeToComplete.completed)
        {
            if (challengeId == "DieOnce")
            {
                CompleteDieOnceChallenge(challengeToComplete);
            }
            else if (challengeId == "WalkDistance" && distance >= challengeToComplete.requiredDistance)
            {
                CompleteWalkDistanceChallenge(challengeToComplete);
            }
            else if (challengeId == "DefeatEnemiesQuickly" && kills >= challengeToComplete.requiredKills && time <= challengeToComplete.requiredTime)
            {
                CompleteDefeatEnemiesQuicklyChallenge(challengeToComplete);
            }
            else if (challengeId == "GrenadeKills" && kills >= challengeToComplete.requiredKills)
            {
                CompleteGrenadeKillsChallenge(challengeToComplete);
            }
            else if (challengeId == "DefeatGolem") 
            {
                CompleteDefeatBoss1Challenge(challengeToComplete);
            }
        }
        else
        {
           // Debug.LogWarning("Challenge not found or already completed: " + challengeId);
        }
    }
    /// <summary>
    /// Completes the "Defeat Golem" boss challenge and unlocks a weapon.
    /// </summary>
    void CompleteDefeatBoss1Challenge(Challenge challenge)
    {
        challenge.completed = true;
        SaveChallenges();
        Debug.Log("Challenge completed: " + challenge.challengeId);
        Debug.Log("Weapon unlocked: " + challenge.weaponIndexToUnlock);
        GameProgressManager.instance.UnlockWeapon(challenge.weaponIndexToUnlock); // Unlock the weapon
        Chest chest = FindObjectOfType<Chest>();
        if (chest != null)
        {
            chest.RefreshAvailableWeapons(); // Ensure the chest refreshes its available weapons
        }
    }
    /// <summary>
    /// Completes the "Die Once" challenge and unlocks a weapon.
    /// </summary>
    void CompleteDieOnceChallenge(Challenge challenge)
    {
        challenge.completed = true;
        SaveChallenges();
        Debug.Log("Challenge completed: " + challenge.challengeId);
        Debug.Log("Weapon unlocked: " + challenge.weaponIndexToUnlock);
        GameProgressManager.instance.UnlockWeapon(challenge.weaponIndexToUnlock);
        Chest chest = FindObjectOfType<Chest>();
        if (chest != null)
        {
            chest.RefreshAvailableWeapons(); // Ensure the chest refreshes its available weapons
        }
    }
    /// <summary>
    /// Completes the "Walk Distance" challenge and unlocks a weapon.
    /// </summary>
    void CompleteWalkDistanceChallenge(Challenge challenge)
    {
        challenge.completed = true;
        SaveChallenges();
        Debug.Log("Challenge completed: " + challenge.challengeId);
        Debug.Log("Weapon unlocked: " + challenge.weaponIndexToUnlock);
        GameProgressManager.instance.UnlockWeapon(challenge.weaponIndexToUnlock);
        Chest chest = FindObjectOfType<Chest>();
        if (chest != null)
        {
            chest.RefreshAvailableWeapons(); // Ensure the chest refreshes its available weapons
        }
    }
    /// <summary>
    /// Completes the "Defeat Enemies Quickly" challenge and unlocks a weapon.
    /// </summary>
    void CompleteDefeatEnemiesQuicklyChallenge(Challenge challenge)
    {
        challenge.completed = true;
        SaveChallenges();
        Debug.Log("Challenge completed: " + challenge.challengeId);
        Debug.Log("Weapon unlocked: " + challenge.weaponIndexToUnlock);
        GameProgressManager.instance.UnlockWeapon(challenge.weaponIndexToUnlock);
        Chest chest = FindObjectOfType<Chest>();
        if (chest != null)
        {
            chest.RefreshAvailableWeapons(); // Ensure the chest refreshes its available weapons
        }
    }


    /// <summary>
    /// Completes the "Grenade Kills" challenge and unlocks a weapon.
    /// </summary>
    void CompleteGrenadeKillsChallenge(Challenge challenge)
    {
        challenge.completed = true;
        SaveChallenges();
        Debug.Log("Challenge completed: " + challenge.challengeId);
        Debug.Log("Weapon unlocked: " + challenge.weaponIndexToUnlock);
        GameProgressManager.instance.UnlockWeapon(challenge.weaponIndexToUnlock);
        Chest chest = FindObjectOfType<Chest>();
        if (chest != null)
        {
            chest.RefreshAvailableWeapons(); // Ensure the chest refreshes its available weapons
        }
    }
    /// <summary>
    /// Saves the current state of challenges to a file.
    /// </summary>
    void SaveChallenges()
    {
        ChallengeList challengeList = new ChallengeList();
        challengeList.challenges = challenges;

        string json = JsonUtility.ToJson(challengeList, true);
        File.WriteAllText(saveFilePath, json);
    }

    /// <summary>
    /// Loads the challenges from a file into memory.
    /// </summary>
    public void LoadChallenges()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            ChallengeList challengeList = JsonUtility.FromJson<ChallengeList>(json);
            challenges = challengeList.challenges;
            Debug.Log("Challenges loaded from file.");
        }
        else
        {
            challenges = new List<Challenge>();
        }
    }

    /// <summary>
    /// Increments the grenade kill count and checks if the related challenge is completed.
    /// </summary>
    public void IncrementGrenadeKills()
    {
        grenadeKills++;
        CheckGrenadeKillsChallenge();
    }
    /// <summary>
    /// Checks if the "Grenade Kills" challenge is completed based on the current grenade kill count.
    /// </summary>
    void CheckGrenadeKillsChallenge()
    {
        Challenge grenadeKillsChallenge = challenges.Find(challenge => challenge.challengeId == "GrenadeKills");
        if (grenadeKillsChallenge != null && !grenadeKillsChallenge.completed && grenadeKills >= grenadeKillsChallenge.requiredKills)
        {
            CompleteGrenadeKillsChallenge(grenadeKillsChallenge);
        }
    }
}