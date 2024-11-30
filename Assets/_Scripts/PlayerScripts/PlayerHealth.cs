using System;
using System.Collections;
using UnityEngine;
using FMOD.Studio; // Include FMOD namespace
using FMODUnity; // Include FMODUnity for EventReference

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;  // The maximum health of the player
    public float currentHealth;     // The current health of the player

    [Header("Health Feedback")]
    public float respawnTime = 5f;  // Time before respawning the player

    public bool isDead = false;
    private HealthBarUI healthBarUI;  // Reference to the HealthBarUI script
    public const string PlayerHealthKey = "PlayerHealth";
    private Animator animator;

    [Header("Audio Settings")]
    [SerializeField] private EventReference gruntNoise; // FMOD Event Reference for grunt noises
    private EventInstance gruntInstance; // Instance to manage the grunt sound
    [Header("Damage Cooldown")]
    private float damageCooldown = 0.4f;  // Cooldown duration (seconds)
    private float lastDamageTime = -Mathf.Infinity;  // Time when the player last took damage

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

        // Create an instance for the grunt noise
        gruntInstance = RuntimeManager.CreateInstance(gruntNoise);
    }

    private void OnDestroy()
    {
        // Ensure the grunt instance is released when the object is destroyed
        gruntInstance.release();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - lastDamageTime < damageCooldown)
        {
            return;  // Ignore damage if within cooldown
        }
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
        lastDamageTime = Time.time;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            // Trigger hurt animation
            PlayHurtAnimation();
            PlayGruntNoise();
        }

        if (healthBarUI != null)
        {
            StartCoroutine(healthBarUI.UpdateHealthBarSmoothly());  // Trigger the health bar update
        }
    }

    private void PlayGruntNoise()
    {
        if (gruntInstance.isValid())
        {
            gruntInstance.start(); // Play the grunt noise
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
