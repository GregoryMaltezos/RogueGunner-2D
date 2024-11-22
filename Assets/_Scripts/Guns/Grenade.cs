using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grenade : MonoBehaviour
{
    public float throwForce = 10f;
    public float explosionDelay = 4f;
    public float spinForce = 100f; // Adjust this value to control the spin speed
    public GameObject explosionEffectPrefab;
    public float explosionRadius = 5f; // The radius of the explosion
    public int explosionDamage = 50; // Damage dealt by the explosion
    public float playerIgnoreDuration = 1f; // Time to ignore collision with player

    private Rigidbody2D rb;
    private Collider2D grenadeCollider;
    private GameObject player;

    // Speed reduction settings
    [Header("Speed Reduction Settings")]
    [Range(0.01f, 1f)]
    public float speedReductionRate = 0.1f; // Rate of speed reduction
    [Range(0.01f, 1f)]
    public float bounceSpeedReductionFactor = 0.5f; // Factor by which speed reduces on bounce
    [SerializeField] private EventReference gunFired;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        grenadeCollider = GetComponent<Collider2D>();

        // Find the player
        player = GameObject.FindGameObjectWithTag("Player");

        // If player exists, throw grenade towards cursor
        if (player != null)
        {
            ThrowGrenadeTowardsCursor();
            // Ignore collision with player for a duration
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(grenadeCollider, playerCollider, true);
                Invoke("EnablePlayerCollision", playerIgnoreDuration);
            }
        }

        Invoke("Explode", explosionDelay); // Explode after the set delay
    }

    private void Update()
    {
        // Gradually reduce speed over time
        if (rb.velocity.magnitude > 0.1f) // Check to avoid reducing speed if it's almost stopped
        {
            // Directly apply the reduction to velocity
            rb.velocity -= rb.velocity.normalized * speedReductionRate * Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Reduce speed more on bounce
        rb.velocity *= bounceSpeedReductionFactor;
    }

    private void ThrowGrenadeTowardsCursor()
    {
        // Get the mouse position using the new Input System
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Convert to world coordinates
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mouseWorldPosition.z = 0; // Set z to 0 for 2D

        // Calculate direction from grenade to mouse position
        Vector2 throwDirection = (mouseWorldPosition - transform.position).normalized;

        // Set initial velocity of the grenade
        rb.velocity = throwDirection * throwForce;

        // Apply torque for spinning effect
        rb.AddTorque(spinForce);
    }

    void EnablePlayerCollision()
    {
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(grenadeCollider, playerCollider, false);
            }
        }
    }

    void Explode()
    {
        AudioManager.instance.PlayOneShot(gunFired, this.transform.position);
        // Instantiate explosion effect
        GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        Destroy(explosionEffect, 0.6f); // Destroy the explosion effect after 0.6 seconds

        // Detect objects in the explosion radius
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D obj in objectsInRange)
        {
            // Check if the object has a tag of "Enemy" or "Player"
            if (obj.CompareTag("Enemy"))
            {
                // Get the object's health script (assuming it has a GetHit method)
                Health targetHealth = obj.GetComponent<Health>();

                if (targetHealth != null)
                {
                    // Apply full damage to the enemy
                    targetHealth.GetHit(explosionDamage, gameObject);
                }
            }
            else if (obj.CompareTag("Player"))
            {
                // Get the player's health script
                PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    // Apply half damage to the player
                    playerHealth.TakeDamage(explosionDamage / 2f);
                }
            }
        }

        Destroy(gameObject); // Destroy the grenade
    }

    // Draw the explosion radius in the editor for visual debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}