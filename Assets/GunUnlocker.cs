using UnityEngine;

public class GunUnlocker : MonoBehaviour
{
    public int gunIndexToUnlock; // Index of the gun to unlock in the WeaponManager's guns list
    public KeyCode interactKey = KeyCode.E; // Key to press to interact

    private WeaponManager weaponManager; // Reference to the WeaponManager script on the player
    private bool canInteract = false; // Flag to track if player can interact

    void Start()
    {
        weaponManager = FindObjectOfType<WeaponManager>(); // Find WeaponManager in the scene
        if (weaponManager == null)
        {
            Debug.LogWarning("GunUnlocker: WeaponManager not found in the scene.");
        }
    }

    void Update()
    {
        // Check for player interaction with 'E' key
        if (canInteract && Input.GetKeyDown(interactKey))
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
