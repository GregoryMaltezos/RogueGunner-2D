using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;            // Speed of the arrow
    public float lifetime = 5f;           // Time before the arrow is destroyed
    private Rigidbody2D rb;

    /// <summary>
    /// Initializes the arrow's movement and sets up its destruction after a specified lifetime.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;  // Set the arrow's velocity to move in the direction it was instantiated
        Destroy(gameObject, lifetime);          // Destroy the arrow after its lifetime
    }

    /// <summary>
    /// Updates the arrow's rotation to ensure it faces the direction of movement.
    /// </summary>
    private void Update()
    {
        // Check if the arrow is still moving
        if (rb.velocity != Vector2.zero)
        {
            // Calculate the angle based on the arrow's velocity
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            // Apply the calculated rotation to the arrow
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    /// <summary>
    /// Handles logic when the arrow collides with another object.
    /// </summary>
    /// <param name="collision">Collision information from the physics engine.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle collision with other objects
        Debug.Log("Arrow stuck on: " + collision.gameObject.name);
        // Call a method to handle the arrow sticking to the surface
        StickToSurface(collision);
    }

    /// <summary>
    /// Stops the arrow's movement and attaches it to the object it collided with.
    /// </summary>
    /// <param name="collision">Collision information from the physics engine.</param>
    private void StickToSurface(Collision2D collision)
    {
        rb.isKinematic = true; // Make the Rigidbody2D kinematic to stop physics interactions
        rb.velocity = Vector2.zero;// Set the arrow's velocity to zero to stop its movement

       
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent any rotation of the arrow

        // Disable all colliders attached to the arrow to prevent further collisions
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        // Optionally, parent the arrow to the object it collided with
        transform.SetParent(collision.transform);
    }
}
