using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentacleAttack : MonoBehaviour
{
    /// <summary>
    /// Called when another collider enters the trigger collider attached to the tentacle.
    /// Checks if the player is hit and applies damage if applicable.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Check if the collider belongs to the player
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Get the damage source from the boss
                DamageSource damageSource = GetComponent<DamageSource>();
                float damage = damageSource != null ? damageSource.GetDamage() : 10f; // Default damage if not set

                // Apply damage to the player
                playerHealth.TakeDamage(damage);
                Debug.Log("Player hit by boss attack! Damage applied: " + damage);
            }
        }
    }
}