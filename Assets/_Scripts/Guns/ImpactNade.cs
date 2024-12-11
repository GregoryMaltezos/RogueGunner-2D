using System.Collections;
using UnityEngine;
using FMODUnity;
using UnityEngine.InputSystem;

public class ImpactNade : MonoBehaviour
{
    public float throwForce = 10f;
    public float spinForce = 100f; // Adjust this value to control the spin speed
    public GameObject explosionEffectPrefab;
    public LayerMask explosionLayers; // Layers to detect collision with
    public float explosionRadius = 5f; // The radius of the explosion
    public int explosionDamage = 50; // Damage dealt by the explosion

    private Rigidbody2D rb;
    private bool exploded = false;
    [SerializeField] private EventReference gunFired;

    private Vector2 throwDirection;


    /// <summary>
    /// Initializes grenade properties, calculates throw direction, and sets velocity and spin.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Calculate direction from grenade to mouse position
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0));
        throwDirection = (mousePosition - (Vector2)transform.position).normalized;

        // Set initial velocity of the grenade
        rb.velocity = throwDirection * throwForce;

        // Apply torque for spinning effect
        rb.angularVelocity = spinForce; // Set angular velocity for immediate spinning
    }

    /// <summary>
    /// Detects collision and triggers explosion when the grenade collides with any object.
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!exploded)
        {
            Explode();
        }
    }


    /// <summary>
    /// Handles the grenade explosion, plays sound, instantiates effects, and applies damage to nearby enemies or the player.
    /// </summary>
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
            // Check if the object has a tag of "Enemy"
            if (obj.CompareTag("Enemy"))
            {
                // Try to get the Health component first
                Health targetHealth = obj.GetComponent<Health>();

                if (targetHealth != null)
                {
                    // Apply full damage to the enemy
                    targetHealth.GetHit(explosionDamage, gameObject);
                }
                else
                {
                    // If Health component is not found, try to get the BossHp component
                    BossHp bossHealth = obj.GetComponent<BossHp>();

                    if (bossHealth != null)
                    {
                        // Apply full damage to the boss
                        bossHealth.TakeDamage(explosionDamage);
                    }
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

    /// <summary>
    /// Draws the explosion radius in the editor for visual debugging.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
