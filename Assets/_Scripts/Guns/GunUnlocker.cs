using UnityEngine;
using UnityEngine.InputSystem; // Include the new Input System namespace

public class GunUnlocker : MonoBehaviour
{
    public int gunIndexToUnlock; // Index of the gun to unlock in the WeaponManager's guns list
    public string interactActionName = "Interact"; // Action name in the Input Action Asset (for example, "Interact" bound to 'E')

    private WeaponManager weaponManager; // Reference to the WeaponManager script on the player
    private bool canInteract = false; // Flag to track if player can interact

    private InputAction interactAction; // Input action for interacting with the gun unlocker

    void Start()
    {
        weaponManager = FindObjectOfType<WeaponManager>(); // Find WeaponManager in the scene
        if (weaponManager == null)
        {
            Debug.LogWarning("GunUnlocker: WeaponManager not found in the scene.");
        }

        // Get the PlayerInput component and the interact action from it
        var playerInput = new NewControls(); // Assuming you have a PlayerInput action asset
        interactAction = playerInput.PlayerInput.Interact; // Access the action by its name
        interactAction.Enable(); // Enable the action
    }

    void OnDisable()
    {
        interactAction.Disable(); // Disable the action when the object is disabled
    }

    void Update()
    {
        // Check if the player is in range and presses the interact button
        if (canInteract && interactAction.triggered)
        {
            UnlockGun();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player enters the trigger collider
        if (other.CompareTag("Player"))
        {
            canInteract = true; // Player can interact when entering trigger collider
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Reset interaction flag when player exits trigger collider
        if (other.CompareTag("Player"))
        {
            canInteract = false;
        }
    }

    void UnlockGun()
    {
        weaponManager.UnlockGun(gunIndexToUnlock); // Unlock the gun

        // Check if the gun is already unlocked
        if (!weaponManager.guns[gunIndexToUnlock].locked)
        {
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

        Destroy(gameObject); // Destroy the pickup item after unlocking the gun (optional)
    }
}
