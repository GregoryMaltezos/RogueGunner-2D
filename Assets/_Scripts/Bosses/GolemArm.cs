using UnityEngine;

public class GolemArm : MonoBehaviour
{
    public LayerMask obstacleLayer; // Layer mask to specify which layers are obstacles

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the projectile hits an obstacle or the player
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0 || other.CompareTag("Player"))
        {
            // If the collided object is the player
            if (other.CompareTag("Player"))
            {
                // Optionally, notify or handle specific logic here if needed
            }

            // Destroy the projectile or similar object
            Destroy(gameObject);
        }
    }
}
