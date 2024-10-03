using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;  // The maximum health of the player
    public float currentHealth;     // The current health of the player

    [Header("Health Feedback")]
    public float respawnTime = 5f;  // Time before respawning the player

    private bool isDead = false;
    private HealthBarUI healthBarUI;  // Reference to the HealthBarUI script

    private void Start()
    {
        currentHealth = maxHealth;
        healthBarUI = FindObjectOfType<HealthBarUI>(); // Find HealthBarUI once and cache the reference

        if (healthBarUI != null)
        {
            StartCoroutine(healthBarUI.UpdateHealthBarSmoothly());  // Start the animation coroutine
        }
        else
        {
            Debug.LogError("HealthBarUI component not found in the scene.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player is invincible in PlayerController before applying damage
        if (PlayerController.instance != null && PlayerController.instance.isInvincible)
        {
            // Player is invincible, so don't apply damage
            return;
        }

        DamageSource damageSource = other.GetComponent<DamageSource>();
        if (damageSource != null)
        {
            TakeDamage(damageSource.GetDamage());

            // Check the tag to decide whether to destroy the GameObject
            if (other.CompareTag("Arrow"))
            {
                Destroy(other.gameObject);  // Destroy the bullet or arrow
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        if (healthBarUI != null)
        {
            StartCoroutine(healthBarUI.UpdateHealthBarSmoothly());  // Trigger the health bar update
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthBar();  // Update health bar
    }

    private void Die()
    {
        isDead = true;
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);

        currentHealth = maxHealth;
        isDead = false;
        transform.position = Vector3.zero; // Move to the respawn point

        UpdateHealthBar();  // Update health bar
    }

    private void UpdateHealthBar()
    {
        if (healthBarUI != null)
        {
            Debug.Log("Updating Health Bar");
            healthBarUI.UpdateHealthBarSmoothly();
        }
    }
}