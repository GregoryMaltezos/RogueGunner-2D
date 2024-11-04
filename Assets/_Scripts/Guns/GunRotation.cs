using UnityEngine;

public class GunRotation : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        playerController = PlayerController.instance; // Get reference to PlayerController
        UpdateGunRotation(); // Set the initial rotation based on player facing direction
    }

    void Update()
    {
        if (playerController == null) return;

        // Continuously update the gun rotation based on the player's facing direction
        UpdateGunRotation();
    }

    // Method to update the gun's rotation based on player's facing direction
    public void UpdateGunRotation()
    {
        // Check if the player is facing right and set rotation accordingly
        if (playerController.IsFacingRight())
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f); // Normal rotation for right facing
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 180f); // Flip the gun for left facing
        }
    }
}
