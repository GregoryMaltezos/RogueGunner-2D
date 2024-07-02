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

    void Update()
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
        isOpen = true;
        chestAnimator.SetTrigger("Open"); // Play the opening animation

        // Check if any challenge has been completed
        bool anyChallengeCompleted = CheckAnyChallengeCompleted();

        if (!anyChallengeCompleted)
        {
            Debug.LogWarning("Cannot spawn weapon: No challenges completed.");
            return; // Exit without spawning anything
        }

        // Check if there are unlocked weapons available
        List<int> unlockedWeapons = GameProgressManager.instance.GetUnlockedWeapons();
        Debug.Log($"Unlocked weapons count: {unlockedWeapons.Count}");
        Debug.Log("Unlocked weapons: " + string.Join(", ", unlockedWeapons));

        if (unlockedWeapons.Count > 0)
        {
            SpawnRandomWeapon(); // Spawn a random weapon
        }
        else
        {
            Debug.LogWarning("No unlocked weapons available to spawn.");
            // Optionally, you can play a different animation or effect for an empty chest
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

    void SpawnRandomWeapon()
    {
        List<int> unlockedWeapons = GameProgressManager.instance.GetUnlockedWeapons();
        List<GameObject> availableWeapons = new List<GameObject>();

        // Collect unlocked weapon prefabs
        foreach (int weaponIndex in unlockedWeapons)
        {
            if (weaponIndex < weaponPrefabs.Length)
            {
                availableWeapons.Add(weaponPrefabs[weaponIndex]);
            }
            else
            {
                Debug.LogWarning("Weapon index " + weaponIndex + " exceeds the number of weapon prefabs.");
            }
        }

        if (availableWeapons.Count > 0)
        {
            int randomIndex = Random.Range(0, availableWeapons.Count);
            GameObject spawnedWeaponPrefab = availableWeapons[randomIndex];

            // Spawn the weapon at the specified spawn point
            spawnedWeapon = Instantiate(spawnedWeaponPrefab, spawnPoint.position, spawnPoint.rotation);

            // Ensure the weapon is initially active when spawned
            spawnedWeapon.SetActive(true);

            // Store the original position for hovering effect
            originalWeaponPosition = spawnedWeapon.transform.position;

            // Notify the WeaponManager or other relevant systems about the spawned weapon
            NotifyWeaponPickedUp(spawnedWeapon);
        }
        else
        {
            Debug.LogWarning("No unlocked weapons available to spawn.");
        }
    }

    // Method to handle when a weapon is picked up from the chest
    void NotifyWeaponPickedUp(GameObject weapon)
    {
        // Implement any specific logic here if needed
        Debug.Log("Weapon picked up from chest: " + weapon.name);
    }
}
