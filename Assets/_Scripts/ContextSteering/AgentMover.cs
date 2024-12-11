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
    private bool isMoving = false;
    // Adjustable knockback force in the inspector
    [SerializeField]
    private float bulletKnockbackForce = 5f; // Default value, adjustable in inspector

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// It updates the agent's velocity based on input and movement speed.
    /// </summary>
    private void FixedUpdate()
    {
        // Only process movement if the agent is allowed to move
        if (canMove)
        {
            // Check if the agent has movement input and is not at zero speed
            if (MovementInput.magnitude > 0 && currentSpeed >= 0)
            {
                oldMovementInput = MovementInput; // Store the current movement input
                currentSpeed += acceleration * maxSpeed * Time.deltaTime; // Increase speed based on acceleration
                isMoving = true; // Set moving flag to true
            }
            else
            {
                // Decrease speed when there's no input (or movement input magnitude is zero)
                currentSpeed -= deacceleration * maxSpeed * Time.deltaTime;
                isMoving = false; // Set moving flag to false
            }
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed); // Clamp current speed to be between 0 and maxSpeed
            rb2d.velocity = oldMovementInput * currentSpeed;  // Set the Rigidbody's velocity based on the movement input and current speed
        }
        else
        {
            // If the agent can't move, stop all movement
            rb2d.velocity = Vector2.zero;
            isMoving = false;
        }
    }

    /// <summary>
    /// Checks if the agent is currently moving.
    /// </summary>
    /// <returns>True if the agent is moving, false otherwise.</returns>
    public bool IsMoving()
    {
        return isMoving;  // Return if the agent is moving
    }


    /// <summary>
    /// Handles collision events with other objects. If the agent collides with a player or a bullet,
    /// appropriate knockback is applied to the agent.
    /// </summary>
    /// <param name="collision">The collision object containing collision data.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Handle collision with the player
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            float knockbackForce = 5f; 
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

    /// <summary>
    /// Enables or disables movement for the agent. If movement is disabled, the agent's velocity is set to zero.
    /// </summary>
    /// <param name="enabled">If true, movement is enabled; if false, movement is disabled.</param>
    public void SetMovement(bool enabled)
    {
        canMove = enabled;

        // If movement is disabled, immediately stop the Rigidbody2D's velocity
        if (!enabled)
        {
            rb2d.velocity = Vector2.zero;
            isMoving = false;
        }
    }
}
