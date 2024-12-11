using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    public Image healthBarFill;  // Reference to the health bar fill image
    public float animationDuration = 0.5f;  // Duration of the health bar animation

    private PlayerHealth playerHealth;  // Direct reference to the PlayerHealth component

    /// <summary>
    /// Initializes the health bar by setting up the player health reference and starting the update coroutine.
    /// </summary>
    private void Start()
    {
        // Initial update attempt
        UpdatePlayerHealthReference();
        StartCoroutine(UpdateHealthBarSmoothly());  // Start coroutine for smooth update
    }

    /// <summary>
    /// Continuously checks for and updates the PlayerHealth reference if it's null.
    /// </summary>
    private void Update()
    {
        // Ensure the PlayerHealth reference remains updated
        UpdatePlayerHealthReference();
    }

    /// <summary>
    /// Attempts to find the PlayerHealth component in the scene and update the reference.
    /// If found, starts the health bar update coroutine.
    /// </summary>
    private void UpdatePlayerHealthReference()
    {
        // Check if the playerHealth reference is null
        if (playerHealth == null)
        {
            // Try to find a PlayerHealth component in the scene
            playerHealth = FindObjectOfType<PlayerHealth>();

            if (playerHealth != null)
            {
                // Debug.Log($"PlayerHealth reference found: Current Health = {playerHealth.currentHealth}, Max Health = {playerHealth.maxHealth}");
                // If found, start the health bar update coroutine
                StartCoroutine(UpdateHealthBarSmoothly()); 
            }
        }
    }

    /// <summary>
    /// Smoothly updates the health bar fill amount based on the player's current health.
    /// </summary>
    /// <returns>An IEnumerator for coroutine execution.</returns>
    public IEnumerator UpdateHealthBarSmoothly()
    {
        // Ensure references are valid
        if (playerHealth != null && healthBarFill != null)
        {
            float startValue = healthBarFill.fillAmount;
            float targetValue = Mathf.Clamp01(playerHealth.currentHealth / playerHealth.maxHealth);
            float elapsedTime = 0f;

          //  Debug.Log($"Starting health bar update: Start Value = {startValue}, Target Value = {targetValue}");

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / animationDuration);
                healthBarFill.fillAmount = Mathf.Lerp(startValue, targetValue, t);
                yield return null;
            }

            healthBarFill.fillAmount = targetValue;  // Ensure the health bar reaches the exact target value at the end
         //   Debug.Log("Health bar update complete.");
        }
        else
        {
            Debug.LogWarning("PlayerHealth or HealthBarFill is null. Cannot update health bar.");
        }
    }

}
