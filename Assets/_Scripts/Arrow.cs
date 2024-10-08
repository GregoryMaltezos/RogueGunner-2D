using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;            // Speed of the arrow
    public float lifetime = 5f;           // Time before the arrow is destroyed
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed; // Shoot straight in the direction it was instantiated
        Destroy(gameObject, lifetime);          // Destroy the arrow after its lifetime
    }

    private void Update()
    {
        // Ensure the arrow always points in the direction of its movement
        if (rb.velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle collision with other objects
        Debug.Log("Arrow stuck on: " + collision.gameObject.name);
        StickToSurface(collision);
    }

    private void StickToSurface(Collision2D collision)
    {
        rb.isKinematic = true; // Make the arrow kinematic
        rb.velocity = Vector2.zero; // Stop the arrow's movement

        // Freeze rotation
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Disable both colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        // Optionally, parent the arrow to the object it collided with
        transform.SetParent(collision.transform);
    }
}
