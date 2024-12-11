using UnityEngine;
using UnityEngine.InputSystem; // Include Input System namespace
using System.Collections.Generic;
using FMODUnity;

public class Chest : MonoBehaviour
{
    public Animator chestAnimator;
    public GameObject[] weaponPrefabs;
    public Transform spawnPoint;
    public float interactionDistance = 2.0f;
    public float hoverHeight = 0.5f;
    public float hoverSpeed = 2f;

    private bool isOpen = false;
    private GameObject spawnedWeapon;
    private Vector3 originalWeaponPosition;
    private CorridorFirstDungeonGenerator dungeonGenerator;

    private StudioEventEmitter emitter;
    private bool hasPlayedIdleSound = false;

    // Reference to the InputAction for interacting with the chest
    private InputAction interactAction;
    [SerializeField] private EventReference open;
    public List<int> alwaysUnlockedWeaponIndices = new List<int>();

    /// <summary>
    /// Initializes the interact InputAction and binds it to the Interact method.
    /// </summary>
    private void OnEnable()
    {
        // Initialize the InputAction and bind to the Interact method
        var playerInput = new NewControls(); // Assuming you created PlayerInput in the Input Action Asset
        interactAction = playerInput.PlayerInput.Interact;
        interactAction.Enable();
    }

    /// <summary>
    /// Disables the interact InputAction and stops any chest sound.
    /// </summary>
    private void OnDisable()
    {
        // Disable the input action when the object is disabled
        interactAction.Disable();
        StopChestSound();
    }
    /// <summary>
    /// Stops any chest sound when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        // Stop sound when chest is destroyed
        StopChestSound();
    }

    /// <summary>
    /// Initializes the chest sound emitter, plays idle sound, and resets chest if on the first floor.
    /// </summary>
    private void Start()
    {
        emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.chestIdle, this.gameObject);
        emitter.Play();
        dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>();

        if (dungeonGenerator != null && dungeonGenerator.CurrentFloor == 1)
        {
            ResetChest();
        }
    }

    /// <summary>
    /// Handles chest interaction and weapon spawning.
    /// </summary>
    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(PlayerController.instance.transform.position, transform.position);

        // Check if player is in interaction range and reinitialize emitter if necessary
        if (distanceToPlayer < interactionDistance && !isOpen)
        {
            if (!emitter.IsPlaying())
            {
                emitter = AudioManager.instance.InitializeEventEmitter(FMODEvents.instance.chestIdle, this.gameObject);
                emitter.Play();
            }
        }

        // Check if the player presses the interact key (E) when near the chest
        if (distanceToPlayer < interactionDistance && interactAction.triggered && !isOpen)
        {
            OpenChest();
        }

        // Apply hovering effect to the spawned weapon if it exists
        if (spawnedWeapon != null)
        {
            // Use a sine wave function to simulate hovering (up and down movement)
            float newY = originalWeaponPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            spawnedWeapon.transform.position = new Vector3(originalWeaponPosition.x, newY, originalWeaponPosition.z);
        }
    }

    /// <summary>
    /// Refreshes the list of available weapons after the chest has been opened.
    /// Spawns a random available weapon.
    /// </summary>
    public void RefreshAvailableWeapons()
    {
        // Only spawn weapons when chest is opened
        if (!isOpen) return;
        // Get the list of unlocked and picked-up weapons
        List<int> unlockedWeapons = GameProgressManager.instance.GetUnlockedWeapons();
        List<int> pickedUpWeapons = WeaponManager.instance.GetPickedUpWeapons();

        List<int> availableWeapons = new List<int>(); // List of weapons available to spawn
        foreach (int weaponIndex in unlockedWeapons) // Iterate through unlocked weapons
        {
            if (!pickedUpWeapons.Contains(weaponIndex))  // Only add weapons that haven't been picked up
            {
                availableWeapons.Add(weaponIndex);
            }
        }

        Debug.Log($"Available weapons count: {availableWeapons.Count}");

        if (availableWeapons.Count > 0)
        {
            SpawnRandomWeapon(availableWeapons);  // Spawn a random weapon from the available list
        }
        else
        {
            // If all weapons are picked up, fallback to unlocked weapons
            Debug.Log("All weapons have been picked up. Resetting weapon pool to unlocked weapons.");
            SpawnRandomWeapon(unlockedWeapons); // Spawn a random weapon from unlocked list
        }
    }


    /// <summary>
    /// Opens the chest and spawns a random weapon if conditions are met.
    /// Plays the chest opening sound and checks if challenges are completed.
    /// </summary>
    void OpenChest()
    {
        if (isOpen) return; // Prevent opening if chest is already open

        isOpen = true;
        chestAnimator.SetTrigger("Open");
        AudioManager.instance.PlayOneShot(open, this.transform.position); // Play chest open sound
        emitter.Stop();

        // Get the list of unlocked weapons and picked-up weapons
        List<int> unlockedWeapons = GameProgressManager.instance.GetUnlockedWeapons();
        List<int> pickedUpWeapons = WeaponManager.instance.GetPickedUpWeapons();

        // Create the list of available weapon indices
        List<int> availableWeapons = new List<int>();

        // Add unlocked weapons that haven't been picked up
        foreach (int weaponIndex in unlockedWeapons)
        {
            if (!pickedUpWeapons.Contains(weaponIndex)) // Only add weapons that haven't been picked up
            {
                availableWeapons.Add(weaponIndex);
            }
        }

        // If no challenges are completed and no other weapons are available, add the always unlocked weapons
        bool anyChallengeCompleted = CheckAnyChallengeCompleted();
        if (!anyChallengeCompleted && availableWeapons.Count == 0)
        {
            availableWeapons.AddRange(alwaysUnlockedWeaponIndices); // Add always unlocked weapon indices
        }

        // If all available weapons have been picked up, reset the pool to unlocked weapons
        if (availableWeapons.Count == 0)
        {
            Debug.Log("All available weapons have been picked up. Refreshing the weapon pool.");
            availableWeapons.AddRange(unlockedWeapons); // Add all unlocked weapons (resetting the pool)
        }

        // Log the available weapons count
        Debug.Log($"Available weapons count: {availableWeapons.Count}");

        // Spawn a random weapon from the available weapons pool
        if (availableWeapons.Count > 0)
        {
            SpawnRandomWeapon(availableWeapons); // Spawn a random weapon from the available weapons pool
        }
        else
        {
            Debug.Log("No available weapons to spawn.");
        }
    }


    /// <summary>
    /// Checks if any challenge has been completed.
    /// </summary>
    /// <returns>True if any challenge is completed, otherwise false.</returns>
    bool CheckAnyChallengeCompleted()
    {
        // Iterate through all challenges and check if any of them are completed
        foreach (ChallengeManager.Challenge challenge in ChallengeManager.instance.challenges)
        {
            if (challenge.completed) // If any challenge is completed, return true
            {
                return true;
            }
        }
        return false; // Return false if no challenge is completed
    }

    /// <summary>
    /// Spawns a random weapon from the list of available weapons.
    /// </summary>
    /// <param name="availableWeapons">List of weapon indices to choose from.</param>
    void SpawnRandomWeapon(List<int> availableWeapons)
    {
        int randomIndex = Random.Range(0, availableWeapons.Count); // Pick a random index from available weapons
        int weaponIndex = availableWeapons[randomIndex]; // Get the weapon index
        GameObject spawnedWeaponPrefab = weaponPrefabs[weaponIndex]; // Get the prefab for the weapon

        spawnedWeapon = Instantiate(spawnedWeaponPrefab, spawnPoint.position, spawnPoint.rotation);  // Spawn the weapon at the spawn point
        spawnedWeapon.SetActive(true); // Activate the spawned weapon

        originalWeaponPosition = spawnedWeapon.transform.position; // Store the original position for hovering effect
        NotifyWeaponPickedUp(weaponIndex); // Notify that the weapon has been picked up
    }

    /// <summary>
    /// Notifies the weapon manager that the weapon has been picked up.
    /// </summary>
    /// <param name="weaponIndex">The index of the weapon that was picked up.</param>
    void NotifyWeaponPickedUp(int weaponIndex)
    {
        WeaponManager.instance.AddPickedUpWeapon(weaponIndex); // Add the weapon to the picked-up list
        Debug.Log("Weapon picked up from chest: " + weaponPrefabs[weaponIndex].name);
    }

    /// <summary>
    /// Resets the chest's weapon pool for the first floor of the dungeon.
    /// </summary>
    private void ResetChest()
    {
        Debug.Log("Chest reset for the first floor.");
        WeaponManager.instance.ResetPickedUpWeapons(); // Reset picked-up weapons for the first floor
    }


    /// <summary>
    /// Stops any sound currently playing from the chest's emitter.
    /// </summary>
    private void StopChestSound()
    {
        // Stop any playing sound from the emitter when the chest is disabled or destroyed
        if (emitter != null && emitter.IsPlaying())
        {
            emitter.Stop();
        }
    }
}