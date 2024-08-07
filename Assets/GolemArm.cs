using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemArm : MonoBehaviour
{
    public int damage = 10; // Amount of damage the projectile deals
    public LayerMask obstacleLayer; // Layer mask to specify which layers are obstacles

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the projectile hits an obstacle or the player
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0 || other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.CompareTag("Player"))
            {
                // Apply damage to the player
               
            }

            // Destroy the projectile
            Destroy(gameObject);
        }
    }
}
