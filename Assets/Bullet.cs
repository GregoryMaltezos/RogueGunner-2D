using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletLifetime = 2f; // Lifetime of the bullet before it disappears
    public int damage = 20; // Damage value for the bullet

    private Rigidbody2D rb2D; // Reference to the bullet's Rigidbody2D component
    private SpriteRenderer spriteRenderer; // Reference to the bullet's SpriteRenderer component
    public bool hasCollided = false; // Flag to track whether the bullet has collided

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        Destroy(gameObject, bulletLifetime); // Destroy the bullet after the lifetime
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasCollided) return; // Prevent multiple collision handling

        // Check if the collision is with an enemy (e.g., boss)
        if (collision.gameObject.CompareTag("Enemy"))
        {
            BossHp bossHp = collision.gameObject.GetComponent<BossHp>();
            if (bossHp != null)
            {
                hasCollided = true; // Mark as collided to prevent further damage
                bossHp.TakeDamage(damage);
                Destroy(gameObject); // Destroy the bullet after applying damage
            }
        }
        else if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.GetComponent<Item>())
        {
            hasCollided = true; // Mark as collided to prevent further handling
            Destroy(gameObject); // Destroy bullet on collision with tiles or items
        }
    }

    void Update()
    {
        if (rb2D.velocity.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
}
