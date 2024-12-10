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
    private void OnEnable()
    {
        // Initialize the InputAction and bind to the Interact method
        var playerInput = new NewControls(); // Assuming you created PlayerInput in the Input Action Asset
        interactAction = playerInput.PlayerInput.Interact;
        interactAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the input action when the object is disabled
        interactAction.Disable();
        StopChestSound();
    }

    private void OnDestroy()
    {
        // Stop sound when chest is destroyed
        StopChestSound();
    }

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

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(PlayerController.instance.transform.position, transform.position);

        // Check if player is in interaction range and reinitialize emitter
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

        // Apply hovering effect to the spawned weapon
        if (spawnedWeapon != null)
        {
            float newY = originalWeaponPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            spawnedWeapon.transform.position = new Vector3(originalWeaponPosition.x, newY, originalWeaponPosition.z);
        }
    }
    public void RefreshAvailableWeapons()
    {
        List<int> unlockedWeapons = GameProgressManager.instance.GetUnlockedWeapons();
        List<int> pickedUpWeapons = WeaponManager.instance.GetPickedUpWeapons();

        List<int> availableWeapons = new List<int>();
        foreach (int weaponIndex in unlockedWeapons)
        {
            if (!pickedUpWeapons.Contains(weaponIndex))
            {
                availableWeapons.Add(weaponIndex);
            }
        }

        Debug.Log($"Available weapons count: {availableWeapons.Count}");

        if (availableWeapons.Count > 0)
        {
            SpawnRandomWeapon(availableWeapons);
        }
        else
        {
            Debug.Log("All weapons have been picked up. Resetting weapon pool to unlocked weapons.");
            SpawnRandomWeapon(unlockedWeapons);
        }
    }

    void OpenChest()
    {
        if (isOpen) return;

        isOpen = true;
        chestAnimator.SetTrigger("Open");
        AudioManager.instance.PlayOneShot(open, this.transform.position);
        emitter.Stop();

        bool anyChallengeCompleted = CheckAnyChallengeCompleted();

        if (!anyChallengeCompleted)
        {
            Debug.LogWarning("Cannot spawn weapon: No challenges completed.");
            return;
        }

        List<int> unlockedWeapons = GameProgressManager.instance.GetUnlockedWeapons();
        List<int> pickedUpWeapons = WeaponManager.instance.GetPickedUpWeapons();

        List<int> availableWeapons = new List<int>();
        foreach (int weaponIndex in unlockedWeapons)
        {
            if (!pickedUpWeapons.Contains(weaponIndex))
            {
                availableWeapons.Add(weaponIndex);
            }
        }

        Debug.Log($"Available weapons count: {availableWeapons.Count}");

        if (availableWeapons.Count > 0)
        {
            SpawnRandomWeapon(availableWeapons);
        }
        else
        {
            Debug.Log("All weapons have been picked up. Resetting weapon pool to unlocked weapons.");
            SpawnRandomWeapon(unlockedWeapons);
        }
    }

    bool CheckAnyChallengeCompleted()
    {
        foreach (ChallengeManager.Challenge challenge in ChallengeManager.instance.challenges)
        {
            if (challenge.completed)
            {
                return true;
            }
        }
        return false;
    }

    void SpawnRandomWeapon(List<int> availableWeapons)
    {
        int randomIndex = Random.Range(0, availableWeapons.Count);
        int weaponIndex = availableWeapons[randomIndex];
        GameObject spawnedWeaponPrefab = weaponPrefabs[weaponIndex];

        spawnedWeapon = Instantiate(spawnedWeaponPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedWeapon.SetActive(true);

        originalWeaponPosition = spawnedWeapon.transform.position;
        NotifyWeaponPickedUp(weaponIndex);
    }

    void NotifyWeaponPickedUp(int weaponIndex)
    {
        WeaponManager.instance.AddPickedUpWeapon(weaponIndex);
        Debug.Log("Weapon picked up from chest: " + weaponPrefabs[weaponIndex].name);
    }

    private void ResetChest()
    {
        Debug.Log("Chest reset for the first floor.");
        WeaponManager.instance.ResetPickedUpWeapons();
    }

    private void StopChestSound()
    {
        // Stop any playing sound from the emitter when the chest is disabled or destroyed
        if (emitter != null && emitter.IsPlaying())
        {
            emitter.Stop();
        }
    }
}