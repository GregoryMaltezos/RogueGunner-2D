using System.Collections;
using UnityEngine;
using FMODUnity;
public class ReaperBoss : MonoBehaviour
{
    public Animator animator;             // Reference to the Animator component
    public Rigidbody2D rb;                // Reference to the Rigidbody2D component
    public GameObject spawnPrefab;        // Prefab to spawn above the boss's head
    public float moveSpeed = 2f;          // Speed at which the boss moves towards the player
    public float dashSpeed = 10f;         // Speed at which the boss dashes towards the player
    public float spawnHeight = 2f;        // Height above the boss where the prefab will spawn
    public float delayBeforeMoving = 2f;  // Time to wait before starting the movement and attack animation
    public float detectionRadius = 8f;    // Radius within which the boss will detect the player
    public float attackRadius = 5f;       // Radius within which the boss will start attacking
    public float dashDuration = 0.5f;     // Duration of the dash towards the player
    public float attackPause = 1f;        // Time to pause after an attack
    public float additionalDelay = 1f;    // Additional delay before dashing
    public float invincibleDuration = 3f; // Duration of the invincible animation

    [Range(0f, 1f)]
    public float invincibilityChance = 0.1f; // Chance of becoming invincible (0 to 1)

    private float maxHealth = 100f;        // Maximum health of the boss
    private float currentHealth;           // Current health of the boss

    private Transform player;              // Reference to the player's Transform
    private bool isAttacking = false;      // Flag to indicate if the boss is attacking
    private bool isFacingRight = true;     // Flag to check if the boss is facing right
    private bool playerDetected = false;   // Flag to track if the player has been detected
    private bool isInvincible = false;      // Flag to indicate if the boss is invincible
    private bool isDead = false;            // Flag to indicate if the boss is dead
    private SpriteRenderer spriteRenderer;  // Reference to the SpriteRenderer component
    private BossHp bossHp;                  // Reference to the BossHp script
    [SerializeField] private EventReference attack;
    [SerializeField] private EventReference detectionNoiseEvent;
    private FMOD.Studio.EventInstance detectionNoiseInstance;
    [SerializeField] private EventReference orb;
    private void Start()
    {
        // Automatically find the player in the scene
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player not found. Make sure the player GameObject has the 'Player' tag.");
            return; // Exit if player is not found
        }

        // Get the BossHp component
        bossHp = GetComponent<BossHp>();
        if (bossHp == null)
        {
            Debug.LogError("BossHp component missing from the boss.");
            return;
        }

        // Initialize health
        currentHealth = maxHealth;
        bossHp.UpdateHealthBar(); // Set initial health bar value

        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component missing from the boss.");
            return;
        }

        // Boss starts in idle animation
        animator.SetTrigger("IdleAnimation");
        rb.velocity = Vector2.zero;

        // Start the attack sequence coroutine
        StartCoroutine(AttackSequence());

        // Start the idle and invincibility coroutine
        StartCoroutine(IdleAndInvincibilitySequence());
        detectionNoiseInstance = RuntimeManager.CreateInstance(detectionNoiseEvent);
    }

    private void Update()
    {
        if (isDead) return; // Stop executing if the boss is dead

        // If player is detected, move towards the player
        if (playerDetected && !isAttacking)
        {
            MoveTowardsPlayer();
        }
        else
        {
            // Check if the player is within the detection radius
            if (!playerDetected && Vector2.Distance(transform.position, player.position) <= detectionRadius)
            {
                playerDetected = true; // Player detected for the first time
                PlayDetectionNoise();
            }
            else if (playerDetected && Vector2.Distance(transform.position, player.position) > detectionRadius)
            {
                playerDetected = false; // Player is no longer detected
                StopDetectionNoise();   // Stop the noise when player exits detection range
            }
        }
    }
    private void PlayDetectionNoise()
    {
        if (detectionNoiseInstance.isValid())
        {
            detectionNoiseInstance.start();  // Start the sound effect if not already playing
        }
    }

    private void StopDetectionNoise()
    {
        if (detectionNoiseInstance.isValid())
        {
            detectionNoiseInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);  // Stop the sound effect immediately
        }
    }
    private void MoveTowardsPlayer()
    {
        // Flip the sprite to face the player
        FlipSprite();
        AudioManager.instance.SetMusicArea(MusicType.Boss);
        // Move the boss towards the player at the move speed
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    private IEnumerator AttackSequence()
    {
        while (!isDead) // Continue only if the boss is not dead
        {
            // Only attack if the player has been detected and is within the attack radius
            if (playerDetected && !isAttacking && Vector2.Distance(transform.position, player.position) <= attackRadius)
            {
                // Start the attack
                isAttacking = true;

                // Flip the sprite to face the player
                FlipSprite();

                // Spawn the prefab above the boss's head
                SpawnPrefab();

                // Wait for the prefab to appear and then wait a bit longer
                yield return new WaitForSeconds(0.5f + additionalDelay);

                // Start the dash
                yield return StartCoroutine(DashTowardsPlayer());

                // Pause after attacking
                yield return new WaitForSeconds(attackPause);

                // Reset to idle state after attacking
                animator.SetTrigger("IdleAnimation");

                isAttacking = false;
            }

            // Short delay before checking again
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator IdleAndInvincibilitySequence()
    {
        while (!isDead) // Continue only if the boss is not dead
        {
            // If not attacking, there's a chance to become invincible
            if (!isAttacking && !isInvincible)
            {
                // Use the configurable chance for invincibility
                if (Random.value <= invincibilityChance)
                {
                    yield return StartCoroutine(PlayInvincibilityAnimation());
                }
            }

            // Continue in idle state
            animator.SetTrigger("IdleAnimation");

            // Wait for a bit before checking again
            yield return new WaitForSeconds(2f); // Adjust as needed for idle timing
        }
    }

    private IEnumerator PlayInvincibilityAnimation()
    {
        // Set the boss to invincible and reduce opacity
        isInvincible = true;
        Color color = spriteRenderer.color;
        color.a = 0.5f; // Reduce opacity by 50%
        spriteRenderer.color = color;

        // Trigger the invincibility animation
        animator.SetTrigger("InvincibilityAnimation");

        // Wait for the duration of the invincible animation
        yield return new WaitForSeconds(invincibleDuration);

        // Revert the boss's invincibility and opacity
        isInvincible = false;
        color.a = 1f; // Restore full opacity
        spriteRenderer.color = color;
    }

    private void SpawnPrefab()
    {
        if (spawnPrefab != null)
        {
            Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + spawnHeight, transform.position.z);
            AudioManager.instance.PlayOneShot(orb, this.transform.position);
            GameObject spawnedObject = Instantiate(spawnPrefab, spawnPosition, Quaternion.identity);

            // Set the boss as the parent of the spawned object
            spawnedObject.transform.SetParent(transform);

            // Optionally destroy the prefab after a certain time
            Destroy(spawnedObject, 1.2f);
        }
    }

    private IEnumerator DashTowardsPlayer()
    {
        if (player == null || isDead) yield break;

        animator.SetTrigger("AttackAnimation");
        AudioManager.instance.PlayOneShot(attack, this.transform.position);
        // Initial dash direction toward the player
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * dashSpeed;

        // Check if the boss should track the player (health < 25%)
        if (currentHealth <= maxHealth * 0.25f)
        {
            float trackingDuration = dashDuration; // Total duration for tracking
            float trackingInterval = 0.3f; // Frequency of re-adjusting direction during dash

            while (trackingDuration > 0)
            {
                // Recalculate direction towards the player for better tracking
                direction = (player.position - transform.position).normalized;
                rb.velocity = direction * dashSpeed;

                // Wait for the interval before adjusting again
                yield return new WaitForSeconds(trackingInterval);
                trackingDuration -= trackingInterval;
            }
        }
        else
        {
            // Wait for the regular dash duration if not tracking
            yield return new WaitForSeconds(dashDuration);
        }

        // Stop the dash and reset to idle animation
        rb.velocity = Vector2.zero;
        animator.SetTrigger("IdleAnimation");
    }

    private void FlipSprite()
    {
        // Check the direction to the player
        bool playerIsToTheRight = player.position.x > transform.position.x;

        // Flip the sprite if the player is on the opposite side
        if (isFacingRight && !playerIsToTheRight || !isFacingRight && playerIsToTheRight)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collision detected with: " + other.gameObject.tag);

        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            if (!isInvincible)
            {
                // Damage the player if colliding
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(10f); // Adjust damage value as needed
                }
                else
                {
                    Debug.LogWarning("PlayerHealth component not found on the Player.");
                }
            }
        }
        // Check if the colliding object is a bullet
        else if (other.CompareTag("FrBullet"))
        {
            if (!isInvincible)
            {
                // Use the BossHp component to handle damage
                Bullet bullet = other.GetComponent<Bullet>();
                if (bullet != null)
                {
                    // Damage the boss using the BossHp method
                    bossHp.TakeDamage((int)bullet.damage); // Pass damage to BossHp
                }

                // Destroy the bullet after hitting the boss
                Destroy(other.gameObject);
            }
        }
    }

    // Optional: Add a method to set current health, if needed
    public void SetCurrentHealth(int health)
    {
        currentHealth = health;
        bossHp.UpdateHealthBar();

        if (currentHealth <= maxHealth * 0.5f)
        {
            moveSpeed = 4f;
            dashSpeed = 12f;
        }

        if (currentHealth <= maxHealth * 0.25f)
        {
            attackPause = 0.5f;
        }

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true; // Set the dead flag
        rb.velocity = Vector2.zero; // Stop all movement
        animator.SetTrigger("Die"); // Trigger death animation
        StopDetectionNoise(); // Stop the detection noise when the boss dies
                              // Optionally, destroy the boss object after a delay
        Destroy(gameObject, 2f); // Adjust the delay as needed
    }


    // Optional: Method to flash red color
    public void FlashRed()
    {
        StartCoroutine(FlashRedCoroutine());
    }

    private IEnumerator FlashRedCoroutine()
    {
        Color originalColor = spriteRenderer.color; // Store the original color
        spriteRenderer.color = Color.red; // Change to red

        // Wait for a short duration
        yield return new WaitForSeconds(0.1f);

        // Restore the original color
        spriteRenderer.color = originalColor;
    }
}
