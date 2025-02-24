using UnityEngine;
using UnityEngine.InputSystem; // For the new Input System
using FMODUnity; // For FMOD audio

public class AmmoPickup : MonoBehaviour
{
    private bool isPlayerInRange = false; // To track if the player is in range to pick up

    // Reference to the InputAction for interacting
    private InputAction interactAction;

    [SerializeField] private EventReference pickupSound; // Reference to the pickup sound event

    /// <summary>
    /// Enables the InputAction for interaction.
    /// </summary>
    private void Start()
    {
        // Initialize the InputAction and bind to the interact method
        var playerInput = new NewControls(); // Assuming NewControls is your input action asset
        interactAction = playerInput.PlayerInput.Interact; // Assuming 'Interact' is the action name
        interactAction.Enable();
    }

    /// <summary>
    /// Ensures that the InputAction is enabled.
    /// </summary>
    private void OnEnable()
    {
        // Enable the input action when the object is enabled
        interactAction.Enable();
    }

    /// <summary>
    /// Disables the InputAction to prevent unnecessary updates.
    /// </summary>
    private void OnDisable()
    {
        // Disable the input action when the object is disabled
        interactAction.Disable();
    }

    /// <summary>
    /// Restores ammo to the player and plays the pickup sound when conditions are met.
    /// </summary>
    private void Update()
    {
        // Check if the player presses the interact button and is in range
        if (isPlayerInRange && interactAction.triggered)
        {
            // Restore ammo to the player's gun
            Gun playerGun = FindObjectOfType<Gun>();
            if (playerGun != null)
            {
                playerGun.RestoreAmmoFromPickup();
            }
            else
            {
                Debug.LogError("Gun component not found in the scene.");
            }

            // Restore 1 grenade to the player
            PlayerGrenade playerGrenade = FindObjectOfType<PlayerGrenade>();
            if (playerGrenade != null)
            {
                playerGrenade.AddGrenade();
            }
            else
            {
                Debug.LogError("PlayerGrenade component not found in the scene.");
            }

            // Play the sound effect when the pickup is collected
            AudioManager.instance.PlayOneShot(pickupSound, this.transform.position);

            // Destroy the ammo pickup object after it has been collected
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the flag to indicate the player is in range to pick up the ammo item.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player entered the pickup range
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player is in range of the Ammo pickup. Press the interact button to pick up.");
        }
    }

    /// <summary>
    /// Sets the flag to indicate the player is no longer in range to pick up the ammo item.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the player left the pickup range
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player left the range of the Ammo pickup.");
        }
    }
}