using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int currentHealth, maxHealth;

    public UnityEvent<GameObject> OnHitWithReference, OnDeathWithReference;

    [SerializeField]
    public bool isDead = false;

    private AgentAnimations agentAnimations;
    private AgentMover agentMover; // Reference to the component responsible for movement

    [SerializeField]
    private float deathAnimationDuration = 1.0f; // Duration of the death animation before destruction

    [SerializeField]
    private GameObject healthPrefab; // Reference to the health prefab to spawn

    [SerializeField]
    private float healthDropChance = 0.2f; // 20% chance to drop health

    private void Start()
    {
        // Get reference to the AgentAnimations script to control animations
        agentAnimations = GetComponentInChildren<AgentAnimations>();
        agentMover = GetComponent<AgentMover>();
    }

    public void InitializeHealth(int healthValue)
    {
        currentHealth = healthValue;
        maxHealth = healthValue;
        isDead = false;
    }

    public void GetHit(int amount, GameObject sender)
    {
        if (isDead)
            return;
        if (sender.layer == gameObject.layer)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            if (!isDead)
            {
                isDead = true;
                OnDeathWithReference?.Invoke(sender);

                if (agentAnimations != null)
                {
                    // Stop movement
                    if (agentMover != null)
                    {
                        agentMover.SetMovement(false); // Stop movement
                    }

                    // Trigger the "Die" animation and start the coroutine to destroy after a delay
                    agentAnimations.TriggerDeathAnimation();
                    StartCoroutine(DestroyAfterDelay(deathAnimationDuration));
                }
                else
                {
                    // If no animations, stop movement and destroy immediately
                    if (agentMover != null)
                    {
                        agentMover.SetMovement(false); // Stop movement
                    }
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            if (agentAnimations != null)
            {
                // Trigger the "Hit" animation and start the coroutine to resume movement after it finishes
                agentAnimations.TriggerHitAnimation();
                StartCoroutine(ResumeMovementAfterDelay(0.5f));
            }

            OnHitWithReference?.Invoke(sender);
        }
    }

    private IEnumerator ResumeMovementAfterDelay(float delay)
    {
        // Wait for the hit animation to finish (0.5 seconds)
        yield return new WaitForSeconds(delay);

        // Re-enable movement after the animation is done
        if (agentMover != null && !isDead)
        {
            agentMover.SetMovement(true);
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        // Wait for the death animation to finish
        yield return new WaitForSeconds(delay);

        // Try to drop a health item with a 20% chance
        TryDropHealth();

        // Destroy the game object
        Destroy(gameObject);
    }

    private void TryDropHealth()
    {
        // Check if a health prefab is set and perform the random chance check
        if (healthPrefab != null && Random.value <= healthDropChance)
        {
            // Instantiate the health prefab at the enemy's position
            Instantiate(healthPrefab, transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for collision with a bullet
        if (collision.gameObject.CompareTag("FrBullet"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                GetHit(bullet.damage, collision.gameObject);
                Destroy(collision.gameObject);
            }
        }

        // Check for collision with a sword
        if (collision.gameObject.CompareTag("Sword"))
        {
            // Apply 10 damage when hit by a sword
            GetHit(10, collision.gameObject);
        }
    }
}
