using UnityEngine;
using UnityEngine.InputSystem; // For the new Input System

public class HpPickup : MonoBehaviour
{
    public float healthRestorePercentage = 0.15f; // 15% health restore
    private bool isPlayerInRange = false; // To track if the player is in range to pick up

    // Reference to the InputAction for interacting
    private InputAction interactAction;

    private void Start()
    {
        // Initialize the InputAction and bind to the interact method
        var playerInput = new NewControls(); // Assuming NewControls is your input action asset
        interactAction = playerInput.PlayerInput.Interact; // Assuming 'Interact' is the action name
        interactAction.Enable();
    }

    private void OnEnable()
    {
        // Enable the input action when the object is enabled
        interactAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the input action when the object is disabled
        interactAction.Disable();
    }

    private void Update()
    {
        // Check if the player presses the interact button and is in range
        if (isPlayerInRange && interactAction.triggered)
        {
            // Find the player’s health component and heal the player
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                // Restore 15% of the player's max health
                float healAmount = playerHealth.maxHealth * healthRestorePercentage;
                playerHealth.Heal(healAmount);

                // Update the health bar UI after healing
                playerHealth.UpdateHealthBar();

                // Optionally, play a sound effect or animation here

                // Destroy the health pickup object after it has been collected
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("PlayerHealth component not found in the scene.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player entered the pickup range
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player is in range of the HP pickup. Press the interact button to pick up.");
        }
    }

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
