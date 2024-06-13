using UnityEngine;
using UnityEngine.Tilemaps;

public class Bullet : MonoBehaviour
{
    public float bulletLifetime = 2f; // Lifetime of the bullet before it disappears

    private Rigidbody2D rb2D; // Reference to the bullet's Rigidbody2D component
    private SpriteRenderer spriteRenderer; // Reference to the bullet's SpriteRenderer component
    private bool hasCollided = false; // Flag to track whether the bullet has collided

    // Called when the bullet is instantiated
    void Start()
    {
        // Get references to Rigidbody2D and SpriteRenderer components
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Destroy the bullet after the specified lifetime
        Destroy(gameObject, bulletLifetime);
    }

    // Called when the bullet collides with another collider
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with a GameObject containing a TilemapCollider2D component
        if (collision.gameObject.GetComponent<TilemapCollider2D>())
        {
            // Destroy the bullet
            Destroy(gameObject);
        }
        // Check if the collision is with a GameObject containing an Item component
        if (collision.gameObject.GetComponent<Item>())
        {
            // Destroy the bullet
            Destroy(gameObject);
        }
        // Check if the collision is with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Do nothing if the collision is with the player
            return;
        }

        // Set the flag to true to indicate that the bullet has collided
        hasCollided = true;
    }

    // Called every frame
    void Update()
    {
        // Check if the bullet's velocity is negative on the x-axis (left) and it hasn't collided yet
        if (rb2D.velocity.x < 0 && !hasCollided)
        {
            // Flip the bullet sprite horizontally
            spriteRenderer.flipX = true;
        }
        else
        {
            // Reset the bullet sprite's rotation if it's traveling right or has collided
            spriteRenderer.flipX = false;
        }
    }
}