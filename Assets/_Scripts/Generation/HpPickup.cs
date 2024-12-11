using UnityEngine;
using UnityEngine.InputSystem; // For the new Input System
using FMODUnity; // For FMOD audio

public class HpPickup : MonoBehaviour
{
    public float healthRestorePercentage = 0.15f; // 15% health restore
    private bool isPlayerInRange = false; // To track if the player is in range to pick up

    // Reference to the InputAction for interacting
    private InputAction interactAction;

    [SerializeField] private EventReference pickupSound; // Reference to the pickup sound event

    /// <summary>
    /// Initializes the InputAction for interacting with the health pickup.
    /// </summary>
    private void Awake()
    {
        // Initialize the InputAction and bind to the interact method
        var playerInput = new NewControls(); // Replace with your actual input setup
        interactAction = playerInput.PlayerInput.Interact; // Replace with your actual action reference
    }

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
    /// Ensures that the InputAction is set up and enabled.
    /// </summary>
    private void OnEnable()
    {
        // Check if interactAction is null and initialize if necessary
        if (interactAction == null)
        {
            var playerInput = new NewControls(); // Replace with your actual input setup
            interactAction = playerInput.PlayerInput.Interact; // Replace with your actual action reference
        }

        // Enable the input action
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
    /// Heals the player and destroys the health pickup if conditions are met.
    /// </summary>
    private void Update()
    {
        // Check if the player presses the interact button and is in range
        if (isPlayerInRange && interactAction != null && interactAction.triggered)
        {
            // Find the player's health component and heal the player
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                // Restore 15% of the player's max health
                float healAmount = playerHealth.maxHealth * healthRestorePercentage;
                playerHealth.Heal(healAmount);

                // Update the health bar UI after healing
                playerHealth.UpdateHealthBar();

                // Play the sound effect when the pickup is collected
                AudioManager.instance.PlayOneShot(pickupSound, this.transform.position);

                // Destroy the health pickup object after it has been collected
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("PlayerHealth component not found in the scene.");
            }
        }
    }

    /// <summary>
    /// Sets the flag to indicate the player is in range to pick up the health item.
    /// </summary>
    /// <param name="other">The collider that entered the trigger area.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player entered the pickup range
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player is in range of the HP pickup. Press the interact button to pick up.");
        }
    }


    /// <summary>
    /// Sets the flag to indicate the player is no longer in range to pick up the health item.
    /// </summary>
    /// <param name="other">The collider that exited the trigger area.</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the player left the pickup range
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player left the range of the HP pickup.");
        }
    }
}