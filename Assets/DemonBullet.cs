using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonBullet : MonoBehaviour
{
    public float lifetime = 5f; // Time before the projectile is destroyed
    public float speed = 5f; // Speed at which the projectile moves

    private Vector2 moveDirection; // The direction in which the projectile will move
    private Rigidbody2D rb;

    private void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();

        // Set the projectile to move in a straight line
        if (rb != null)
        {
            rb.velocity = moveDirection * speed;

            // Rotate the projectile to face the direction of movement
            RotateProjectile(moveDirection);
        }

        // Destroy the projectile after a certain lifetime
        Destroy(gameObject, lifetime);
    }

    // This function is called to set the direction from the outside when the projectile is spawned
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized; // Ensure the direction is normalized (unit vector)
    }

    private void RotateProjectile(Vector2 direction)
    {
        // Calculate the angle in degrees for rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply the rotation to the transform
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

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
