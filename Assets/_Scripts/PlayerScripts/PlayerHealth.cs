using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;  // The maximum health of the player
    public float currentHealth;     // The current health of the player

    [Header("Health Feedback")]
    public float respawnTime = 5f;  // Time before respawning the player

    private bool isDead = false;
    private HealthBarUI healthBarUI;  // Reference to the HealthBarUI script
    public const string PlayerHealthKey = "PlayerHealth";
    private Animator animator;
    private void Start()
    {
        currentHealth = maxHealth;
        healthBarUI = FindObjectOfType<HealthBarUI>(); // Find HealthBarUI once and cache the reference
        animator = GetComponent<Animator>();
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
        else
        {
            // Trigger hurt animation
            PlayHurtAnimation();
        }

        if (healthBarUI != null)
        {
            StartCoroutine(healthBarUI.UpdateHealthBarSmoothly());  // Trigger the health bar update
        }
    }
    private void PlayHurtAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Hurt"); // Set the hurt trigger
            StartCoroutine(ResetHurtTrigger(0.2f)); // Start coroutine to reset the trigger after 0.2 seconds
        }
    }

    private IEnumerator ResetHurtTrigger(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        if (animator != null)
        {
            animator.ResetTrigger("Hurt"); // Reset the trigger
            animator.SetTrigger("Idle");
        }
    }

    private void Die()
    {
        Debug.Log("Player has died.");
        isDead = true; // Set the dead flag to true

        // Notify the PlayerController about the death
        PlayerController.instance.Die(); // Call the Die method in PlayerController to show the death menu

    }


    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // Call the coroutine to update the health bar smoothly
        if (healthBarUI != null)
        {
            StartCoroutine(healthBarUI.UpdateHealthBarSmoothly());  // Trigger the health bar update
        }
    }



    // Make this method public
    public void UpdateHealthBar()
    {
        if (healthBarUI != null)
        {
            Debug.Log("Updating Health Bar");
            healthBarUI.UpdateHealthBarSmoothly();
        }
    }



    public void SavePlayerHealth()
    {
        PlayerPrefs.SetFloat(PlayerHealthKey, currentHealth); // Save the current health
        PlayerPrefs.Save(); // Save changes
    }

    public void LoadPlayerHealth()
    {
        if (PlayerPrefs.HasKey(PlayerHealthKey))
        {
            currentHealth = PlayerPrefs.GetFloat(PlayerHealthKey); // Load health from PlayerPrefs
        }
        else
        {
            currentHealth = maxHealth; // Default to max health if no data exists
        }
        UpdateHealthBar(); // Update the health bar to reflect loaded health
    }

}