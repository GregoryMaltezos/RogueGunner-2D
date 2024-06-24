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
        public float requiredDistance;
        public int requiredKills;
        public float requiredTime;
        public bool completed;
        public int weaponIndexToUnlock;

        public Challenge(string id, float distance, int kills, float time, int weaponIndex, bool isCompleted)
        {
            challengeId = id;
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

    void Start()
    {
        InitializeChallenges();
    }

    void InitializeChallenges()
    {
        LoadChallenges();

        if (challenges.Count == 0) // If challenges haven't been initialized yet
        {
            // Initialize challenges
            challenges.Add(new Challenge("DieOnce", 0f, 0, 0f, 0, false)); // No distance or kills requirement
            challenges.Add(new Challenge("WalkDistance", 1000f, 0, 0f, 1, false)); // Requires walking 1000 units
            challenges.Add(new Challenge("DefeatEnemiesQuickly", 0f, 3, 3f, 2, false)); // Defeat 3 enemies within 3 seconds
            challenges.Add(new Challenge("GrenadeKills", 0f, 15, 0f, 3, false)); // Get 15 grenade kills

            // Save initialized challenges
            SaveChallenges();
        }
    }

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
        }
        else
        {
            Debug.LogWarning("Challenge not found or already completed: " + challengeId);
        }
    }

    void CompleteDieOnceChallenge(Challenge challenge)
    {
        challenge.completed = true;
        SaveChallenges();
        Debug.Log("Challenge completed: " + challenge.challengeId);
        Debug.Log("Weapon unlocked: " + challenge.weaponIndexToUnlock);
    }

    void CompleteWalkDistanceChallenge(Challenge challenge)
    {
        challenge.completed = true;
        SaveChallenges();
        Debug.Log("Challenge completed: " + challenge.challengeId);
        Debug.Log("Weapon unlocked: " + challenge.weaponIndexToUnlock);
    }

    void CompleteDefeatEnemiesQuicklyChallenge(Challenge challenge)
    {
        challenge.completed = true;
        SaveChallenges();
        Debug.Log("Challenge completed: " + challenge.challengeId);
        Debug.Log("Weapon unlocked: " + challenge.weaponIndexToUnlock);
    }

    void CompleteGrenadeKillsChallenge(Challenge challenge)
    {
        challenge.completed = true;
        SaveChallenges();
        Debug.Log("Challenge completed: " + challenge.challengeId);
        Debug.Log("Weapon unlocked: " + challenge.weaponIndexToUnlock);
    }

    void SaveChallenges()
    {
        ChallengeList challengeList = new ChallengeList();
        challengeList.challenges = challenges;

        string json = JsonUtility.ToJson(challengeList, true);
        File.WriteAllText(saveFilePath, json);
    }

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

    public void IncrementGrenadeKills()
    {
        grenadeKills++;
        CheckGrenadeKillsChallenge();
    }

    void CheckGrenadeKillsChallenge()
    {
        Challenge grenadeKillsChallenge = challenges.Find(challenge => challenge.challengeId == "GrenadeKills");
        if (grenadeKillsChallenge != null && !grenadeKillsChallenge.completed && grenadeKills >= grenadeKillsChallenge.requiredKills)
        {
            CompleteGrenadeKillsChallenge(grenadeKillsChallenge);
        }
    }
}
