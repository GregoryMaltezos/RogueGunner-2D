using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMover : MonoBehaviour
{
    private Rigidbody2D rb2d;

    [SerializeField]
    private float maxSpeed = 2, acceleration = 50, deacceleration = 100;
    [SerializeField]
    private float currentSpeed = 0;
    private Vector2 oldMovementInput;
    public Vector2 MovementInput { get; set; }

    // Variable to control movement
    private bool canMove = true;

    // Adjustable knockback force in the inspector
    [SerializeField]
    private float bulletKnockbackForce = 5f; // Default value, adjustable in inspector

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Only process movement if the agent is allowed to move
        if (canMove)
        {
            if (MovementInput.magnitude > 0 && currentSpeed >= 0)
            {
                oldMovementInput = MovementInput;
                currentSpeed += acceleration * maxSpeed * Time.deltaTime;
            }
            else
            {
                currentSpeed -= deacceleration * maxSpeed * Time.deltaTime;
            }
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
            rb2d.velocity = oldMovementInput * currentSpeed;
        }
        else
        {
            // If the agent can't move, stop all movement
            rb2d.velocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Handle collision with the player
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            float knockbackForce = 5f; // Adjust as necessary
            rb2d.velocity = Vector2.zero; // Reset velocity
            rb2d.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
        else if (collision.gameObject.CompareTag("FrBullet"))
        {
            // Handle collision with the bullet
            Vector2 bulletVelocity = collision.relativeVelocity; // Get the bullet's velocity
            float knockbackMagnitude = bulletVelocity.magnitude * 0.5f; // Adjust multiplier as necessary

            // Calculate the knockback direction as the opposite of the bullet's velocity
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized; // Push away from the bullet
            rb2d.velocity = Vector2.zero; // Reset enemy velocity before applying knockback

            // Use the adjustable knockback force from the inspector
            rb2d.AddForce(knockbackDirection * (bulletKnockbackForce + knockbackMagnitude), ForceMode2D.Impulse);

            // Optional: Destroy the bullet upon impact
            Destroy(collision.gameObject);
        }
    }

    // Method to enable or disable movement
    public void SetMovement(bool enabled)
    {
        canMove = enabled;

        // If movement is disabled, immediately stop the Rigidbody2D's velocity
        if (!enabled)
        {
            rb2d.velocity = Vector2.zero;
        }
    }
}
