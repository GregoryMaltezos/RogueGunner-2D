using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    private bool isPlayerInRange = false; // To track if the player is in range to pick up

    private void Update()
    {
        // Check if the player presses the "E" key and is in range of the ammo pickup
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Find the player's gun component
            Gun playerGun = FindObjectOfType<Gun>();
            if (playerGun != null)
            {
                // Restore ammo to the current gun
                playerGun.RestoreAmmoFromPickup();

                // Optionally, play a sound effect or animation here

                // Destroy the ammo pickup object after it has been collected
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("Gun component not found in the scene.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player entered the pickup range
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player is in range of the Ammo pickup. Press 'E' to pick up.");
        }
    }

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
