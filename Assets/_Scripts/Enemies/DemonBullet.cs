using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonBullet : MonoBehaviour
{
    public float lifetime = 5f; // Time before the projectile is destroyed
    public float speed = 5f; // Speed at which the projectile moves

    private Vector2 moveDirection; // The direction in which the projectile will move
    private Rigidbody2D rb;

    /// <summary>
    /// Initializes the projectile's movement and sets it to self-destruct after its lifetime expires.
    /// </summary>
    private void Start()
    {
        // Get the Rigidbody2D component attached to the projectile
        rb = GetComponent<Rigidbody2D>();

        // Set the projectile's velocity if the Rigidbody2D exists
        if (rb != null)
        {
            rb.velocity = moveDirection * speed; // Move the projectile in the specified direction

            // Rotate the projectile to face its movement direction
            RotateProjectile(moveDirection);
        }

        // Destroy the projectile after a certain lifetime
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Sets the direction of the projectile's movement.
    /// This method is called externally when the projectile is spawned.
    /// </summary>
    /// <param name="direction">The direction vector for the projectile.</param>
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized; // Normalize the direction to ensure consistent movement speed
    }


    /// <summary>
    /// Rotates the projectile to face the specified movement direction.
    /// </summary>
    /// <param name="direction">The direction vector for rotation.</param>
    private void RotateProjectile(Vector2 direction)
    {
        // Calculate the angle in degrees based on the direction vector
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply the calculated angle to the projectile's rotation
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Handles collision events for the projectile.
    /// </summary>
    /// <param name="collision">Collision data provided by the physics engine.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the bullet hit the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get the DamageSource component from the bullet
            DamageSource damageSource = GetComponent<DamageSource>();
            if (damageSource != null)
            {
                // Apply damage to the player
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageSource.GetDamage());
                }
            }
        }

        // Destroy the projectile upon hitting something (could be expanded to check for specific targets)
        Destroy(gameObject);
    }
}
