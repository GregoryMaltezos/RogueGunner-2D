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
            // Check if the enemy is about to push the player away
            // You could reset the enemy's velocity when colliding with the player.
            rb2d.velocity = Vector2.zero; // Stops the enemy's movement when colliding with the player
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