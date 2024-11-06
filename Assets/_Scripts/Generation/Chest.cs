using UnityEngine;
using System.Collections.Generic;

public class Chest : MonoBehaviour
{
    public Animator chestAnimator; // Reference to the Animator component
    public GameObject[] weaponPrefabs; // Array of weapon prefabs
    public Transform spawnPoint; // Point where the weapon will be spawned
    public float interactionDistance = 2.0f; // Distance within which the player can interact
    public float hoverHeight = 0.5f; // Height of the hovering effect
    public float hoverSpeed = 2f; // Speed of the hovering effect

    private bool isOpen = false; // Track whether the chest is open
    private GameObject spawnedWeapon; // Reference to the spawned weapon
    private Vector3 originalWeaponPosition; // Original position of the spawned weapon

    private CorridorFirstDungeonGenerator dungeonGenerator;

    private void Start()
    {
        // Find the CorridorFirstDungeonGenerator in the scene
        dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>();

        if (dungeonGenerator != null && dungeonGenerator.CurrentFloor == 1)
        {
            ResetChest(); // Reset the chest contents if on the first floor
        }
    }

    private void Update()
    {
        // Check if the player is near and presses 'E' to open the chest
        if (Vector3.Distance(PlayerController.instance.transform.position, transform.position) < interactionDistance && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            OpenChest();
        }

        // Apply hovering effect to the spawned weapon if it exists
        if (spawnedWeapon != null)
        {
            float newY = originalWeaponPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            spawnedWeapon.transform.position = new Vector3(originalWeaponPosition.x, newY, originalWeaponPosition.z);
        }
    }

    void OpenChest()
    {
        if (isOpen) return; // If already open, do nothing

        isOpen = true;
        chestAnimator.SetTrigger("Open"); // Play the opening animation

        // Check if any challenge has been completed
        bool anyChallengeCompleted = CheckAnyChallengeCompleted();

        if (!anyChallengeCompleted)
        {
            Debug.LogWarning("Cannot spawn weapon: No challenges completed.");
            return; // Exit without spawning anything
        }

        // Get the list of unlocked weapons and picked-up weapons
        List<int> unlockedWeapons = GameProgressManager.instance.GetUnlockedWeapons();
        List<int> pickedUpWeapons = WeaponManager.instance.GetPickedUpWeapons();

        // Filter out weapons that the player has already picked up
        List<int> availableWeapons = new List<int>();
        foreach (int weaponIndex in unlockedWeapons)
        {
            if (!pickedUpWeapons.Contains(weaponIndex)) // Only add weapons that have not been picked up yet
            {
                availableWeapons.Add(weaponIndex);
            }
        }

        Debug.Log($"Available weapons count: {availableWeapons.Count}");

        if (availableWeapons.Count > 0)
        {
            // Spawn a random weapon that hasn't been picked up yet
            SpawnRandomWeapon(availableWeapons);
        }
        else
        {
            // If all weapons have been picked up, spawn from the full pool of unlocked weapons
            Debug.Log("All weapons have been picked up. Resetting weapon pool to unlocked weapons.");
            SpawnRandomWeapon(unlockedWeapons); // Spawn from all unlocked weapons
        }
    }

    bool CheckAnyChallengeCompleted()
    {
        // Implement logic to check if any challenge is completed
        foreach (ChallengeManager.Challenge challenge in ChallengeManager.instance.challenges)
        {
            if (challenge.completed)
            {
                return true; // Return true if any challenge is completed
            }
        }
        return false; // Return false if no challenges are completed
    }

    void SpawnRandomWeapon(List<int> availableWeapons)
    {
        // Select a random weapon from the available weapons
        int randomIndex = Random.Range(0, availableWeapons.Count);
        int weaponIndex = availableWeapons[randomIndex];
        GameObject spawnedWeaponPrefab = weaponPrefabs[weaponIndex];

        // Spawn the weapon at the specified spawn point
        spawnedWeapon = Instantiate(spawnedWeaponPrefab, spawnPoint.position, spawnPoint.rotation);

        // Ensure the weapon is initially active when spawned
        spawnedWeapon.SetActive(true);

        // Store the original position for hovering effect
        originalWeaponPosition = spawnedWeapon.transform.position;

        // Notify the WeaponManager or other relevant systems about the spawned weapon
        NotifyWeaponPickedUp(weaponIndex);
    }

    // Method to handle when a weapon is picked up from the chest
    void NotifyWeaponPickedUp(int weaponIndex)
    {
        WeaponManager.instance.AddPickedUpWeapon(weaponIndex); // Add the weapon to the picked-up list
        Debug.Log("Weapon picked up from chest: " + weaponPrefabs[weaponIndex].name);
    }

    // Method to reset chest contents for the first floor
    private void ResetChest()
    {
        Debug.Log("Chest reset for the first floor.");
        WeaponManager.instance.ResetPickedUpWeapons(); // Reset the list of picked-up weapons
    }
}