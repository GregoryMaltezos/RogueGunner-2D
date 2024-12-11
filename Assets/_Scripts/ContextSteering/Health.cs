using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class Health : MonoBehaviour
{
    // -------------------- Health Variables --------------------
    [Header("Health Settings")]
    [SerializeField]
    private int currentHealth; // Current health of the entity

    [SerializeField]
    private int maxHealth; // Maximum health of the entity

    [SerializeField]
    public bool isDead = false; // Is the entity dead?
    [SerializeField] private EventReference death;
    [SerializeField] private EventReference hit;

    // -------------------- Hit Sound Cooldown --------------------
    [Header("Hit Sound Settings")]
    [SerializeField]
    private float hitSoundCooldown = 0.5f; // Minimum time between hit sounds
    private float lastHitSoundTime = -Mathf.Infinity; // Time the last hit sound was played

    // -------------------- Events --------------------
    [Header("Health Events")]
    public UnityEvent<GameObject> OnHitWithReference; // Event triggered when hit
    public UnityEvent<GameObject> OnDeathWithReference; // Event triggered on death

    // -------------------- Health Drop Settings --------------------
    [Header("Health Drop Settings")]
    [SerializeField]
    private GameObject healthPrefab; // Reference to the health prefab to spawn

    [SerializeField]
    private GameObject ammoPrefab; // Reference to the ammo prefab to spawn

    [SerializeField]
    [Range(0f, 1f)]
    private float healthDropChance = 0.2f; // 20% chance to drop health

    [SerializeField]
    [Range(0f, 1f)]
    private float ammoDropChance = 0.3f; // 30% chance to drop ammo

    // -------------------- Animation & Movement References --------------------
    [Header("Animation & Movement Settings")]
    private AgentAnimations agentAnimations; // Reference to the AgentAnimations script
    private AgentMover agentMover; // Reference to the component responsible for movement

    [SerializeField]
    private float deathAnimationDuration = 1.0f; // Duration of the death animation before destruction

    // -------------------- Unity Lifecycle Methods --------------------

    /// <summary>
    /// Initializes the references to AgentAnimations and AgentMover components.
    /// </summary>
    private void Start()
    {
        // Get reference to the AgentAnimations and AgentMover scripts
        agentAnimations = GetComponentInChildren<AgentAnimations>();
        agentMover = GetComponent<AgentMover>();
    }

    // -------------------- Initialization --------------------
    /// <summary>
    /// Initializes the health system with a specified health value.
    /// Sets the current and max health to the provided value and ensures the entity is not dead.
    /// </summary>
    /// <param name="healthValue">The initial health value of the entity</param>
    public void InitializeHealth(int healthValue)
    {
        currentHealth = healthValue;
        maxHealth = healthValue;
        isDead = false;
    }

    // -------------------- Damage Handling --------------------
    /// <summary>
    /// Handles the entity getting hit with damage. Decreases health and checks for death.
    /// Triggers events and animations based on the entity's current state.
    /// </summary>
    /// <param name="amount">Amount of damage to apply</param>
    /// <param name="sender">The source of the damage (e.g., bullet, grenade)</param>
    public void GetHit(int amount, GameObject sender)
    {
        if (isDead)
            return;

        // Check if the sender is on the same layer (ignore friendly fire)
        if (sender.layer == gameObject.layer)
            return;

        // Check if the sender is a grenade
        if (sender.CompareTag("PlayerGrenade"))
        {
            Debug.Log($"{gameObject.name} hit by grenade! Damage: {amount}");
        }
        // Apply damage
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            if (!isDead)
            {
                isDead = true;
                OnDeathWithReference?.Invoke(sender);
                HandleDeath();
            }
        }
        else
        {
            PlayHitSound();
            HandleHit(sender);
        }
    }

    /// <summary>
    /// Plays the hit sound if the cooldown period has elapsed since the last hit sound.
    /// </summary>
    private void PlayHitSound()
    {
        // Only play the hit sound if enough time has passed since the last one
        if (Time.time >= lastHitSoundTime + hitSoundCooldown)
        {
            AudioManager.instance.PlayOneShot(hit, this.transform.position);
            lastHitSoundTime = Time.time; // Update the time of the last hit sound
        }
    }

    /// <summary>
    /// Handles the entity's death. Stops movement, triggers the death animation, and schedules destruction.
    /// </summary>
    private void HandleDeath()
    {
        if (agentAnimations != null)
        {
            // Stop movement
            if (agentMover != null)
            {
                agentMover.SetMovement(false);
            }
            // Play death sound
            AudioManager.instance.PlayOneShot(death, this.transform.position);
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
            AudioManager.instance.PlayOneShot(death, this.transform.position);
            Destroy(gameObject);
        }

        // Notify GameManager that an enemy was killed
        NotifyDeathToGameManager();
    }

    /// <summary>
    /// Handles the entity getting hit. Plays the "Hit" animation and triggers other related behaviors.
    /// </summary>
    /// <param name="sender">The source of the hit</param>
    private void HandleHit(GameObject sender)
    {
        if (agentAnimations != null)
        {
            // Trigger the "Hit" animation and start the coroutine to resume movement after it finishes
            agentAnimations.TriggerHitAnimation();
            StartCoroutine(ResumeMovementAfterDelay(0.5f));
        }

        OnHitWithReference?.Invoke(sender);
    }

    // -------------------- Coroutine Methods --------------------
    /// <summary>
    /// Resumes movement after a specified delay, used for hit animations or delays.
    /// </summary>
    /// <param name="delay">Delay in seconds before movement is resumed</param>
    private IEnumerator ResumeMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (agentMover != null && !isDead)
        {
            agentMover.SetMovement(true); // Resume movement after delay
        }
    }

    /// <summary>
    /// Destroys the entity after a delay, also attempts to drop health or ammo.
    /// </summary>
    /// <param name="delay">Delay in seconds before destruction</param>
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!TryDropAmmo())
        {
            TryDropHealth();
        }

        Destroy(gameObject); // Destroy the game object after the delay
    }

    // -------------------- Drop Methods --------------------
    /// <summary>
    /// Attempts to drop health by instantiating a health prefab if the drop chance is met.
    /// </summary>
    private void TryDropHealth()
    {
        if (healthPrefab != null && Random.value <= healthDropChance)
        {
            Instantiate(healthPrefab, transform.position, Quaternion.identity);
        }
    }
    /// <summary>
    /// Attempts to drop ammo by instantiating an ammo prefab if the drop chance is met.
    /// </summary>
    /// <returns>True if ammo was dropped, otherwise false</returns>
    private bool TryDropAmmo()
    {
        if (ammoPrefab != null && Random.value <= ammoDropChance)
        {
            Instantiate(ammoPrefab, transform.position, Quaternion.identity);
            return true;
        }
        return false;
    }

    // -------------------- Collision Handling --------------------
    /// <summary>
    /// Handles collisions with other objects. Checks for specific tags like "FrBullet" or "Sword".
    /// </summary>
    /// <param name="collision">The collider of the object the entity collided with</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("FrBullet"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                GetHit(bullet.damage, collision.gameObject); // Apply damage from bullet
                Destroy(collision.gameObject); // Destroy the bullet after it hits
            }
        }

        if (collision.gameObject.CompareTag("Sword"))
        {
            GetHit(22, collision.gameObject); // Apply 22 damage if hit by sword
        }
    }

    /// <summary>
    /// Notifies the GameManager that an enemy has been killed.
    /// </summary>
    private void NotifyDeathToGameManager()
    {
        // Inform the GameManager that an enemy has died
        GameManager.Instance.OnEnemyKilled(); // Call a method in the GameManager
    }


}
