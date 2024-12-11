using UnityEngine;
using UnityEngine.InputSystem; // Include the new Input System namespace
using FMODUnity;

public class GunUnlocker : MonoBehaviour
{
    public int gunIndexToUnlock; // Index of the gun to unlock in the WeaponManager's guns list
    public string interactActionName = "Interact"; // Action name in the Input Action Asset (for example, "Interact" bound to 'E')

    private WeaponManager weaponManager; // Reference to the WeaponManager script on the player
    private bool canInteract = false; // Flag to track if player can interact

    private InputAction interactAction; // Input action for interacting with the gun unlocker
    [SerializeField] private EventReference pickup;

    /// <summary>
    /// Finds the WeaponManager and sets up the input action.
    /// </summary>
    void Start()
    {
        weaponManager = FindObjectOfType<WeaponManager>(); // Find WeaponManager in the scene
        if (weaponManager == null)
        {
            Debug.LogWarning("GunUnlocker: WeaponManager not found in the scene.");
        }

        // Get the PlayerInput component and the interact action from it
        var playerInput = new NewControls(); //action asset
        interactAction = playerInput.PlayerInput.Interact; // Access the action by its name
        interactAction.Enable(); // Enable the action
    }

    /// <summary>
    /// Disables the input action.
    /// </summary>
    void OnDisable()
    {
        interactAction.Disable(); // Disable the action when the object is disabled
    }

    /// <summary>
    /// Checks if the player can interact and triggers the unlock action.
    /// </summary>
    void Update()
    {
        // Check if the player is in range and presses the interact button
        if (canInteract && interactAction.triggered)
        {
            UnlockGun(); // Unlock the gun if the player interacts
        }
    }

    /// <summary>
    /// Sets canInteract flag to true when the player enters.
    /// </summary>
    /// <param name="other">Collider that entered the trigger area.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player enters the trigger collider
        if (other.CompareTag("Player"))
        {
            canInteract = true; // Player can interact when entering trigger collider
        }
    }

    /// <summary>
    /// Resets canInteract flag when the player exits.
    /// </summary>
    /// <param name="other">Collider that exited the trigger area.</param>
    void OnTriggerExit2D(Collider2D other)
    {
        // Reset interaction flag when player exits trigger collider
        if (other.CompareTag("Player"))
        {
            canInteract = false;
        }
    }

    /// <summary>
    /// Unlocks the gun and restores ammo if the gun is not already unlocked. Plays the pickup sound.
    /// </summary>
    void UnlockGun()
    {
        // Play the pickup sound
        AudioManager.instance.PlayOneShot(pickup, this.transform.position);

        // Unlock the gun
        weaponManager.UnlockGun(gunIndexToUnlock);

        // Check if the gun is already unlocked
        if (!weaponManager.guns[gunIndexToUnlock].locked)
        {
            // If unlocked, restore ammo
            Gun gunComponent = weaponManager.guns[gunIndexToUnlock].gunObject.GetComponent<Gun>();
            if (gunComponent != null)
            {
                gunComponent.RestoreAmmo(); // Restore all ammo including clips
                Debug.Log("Ammo restored for unlocked gun!");
            }
        }
        else
        {
            Debug.Log("Gun unlocked!");
        }

        Destroy(gameObject); // Destroy the pickup item after unlocking the gun
    }
}