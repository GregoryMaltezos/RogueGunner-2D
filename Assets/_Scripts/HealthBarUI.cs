using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    public Image healthBarFill;  // Reference to the health bar fill image
    public float animationDuration = 0.5f;  // Duration of the health bar animation

    private PlayerHealth playerHealth;  // Direct reference to the PlayerHealth component

    private void Start()
    {
        // Initial update attempt
        UpdatePlayerHealthReference();
        StartCoroutine(UpdateHealthBarSmoothly());  // Start coroutine for smooth update
    }

    private void Update()
    {
        // Continuously check if the playerHealth reference needs to be updated
        UpdatePlayerHealthReference();
    }

    private void UpdatePlayerHealthReference()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();

            if (playerHealth != null)
            {
                Debug.Log($"PlayerHealth reference found: Current Health = {playerHealth.currentHealth}, Max Health = {playerHealth.maxHealth}");
                StartCoroutine(UpdateHealthBarSmoothly());  // Start coroutine for smooth update
            }
        }
    }

    public IEnumerator UpdateHealthBarSmoothly()
    {
        if (playerHealth != null && healthBarFill != null)
        {
            float startValue = healthBarFill.fillAmount;
            float targetValue = Mathf.Clamp01(playerHealth.currentHealth / playerHealth.maxHealth);
            float elapsedTime = 0f;

            Debug.Log($"Starting health bar update: Start Value = {startValue}, Target Value = {targetValue}");

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / animationDuration);
                healthBarFill.fillAmount = Mathf.Lerp(startValue, targetValue, t);
                yield return null;
            }

            healthBarFill.fillAmount = targetValue;  // Ensure the final value is set
            Debug.Log("Health bar update complete.");
        }
        else
        {
            Debug.LogWarning("PlayerHealth or HealthBarFill is null. Cannot update health bar.");
        }
    }

}
