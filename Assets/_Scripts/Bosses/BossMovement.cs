using System.Collections;
using UnityEngine;
using FMODUnity;

public class BossMovement : MonoBehaviour
{
    public float speed = 2f;
    public float changeDirectionTime = 2f;
    public float movementRadius = 5f;
    public float attackRange = 8f;
    public float attackCooldown = 2f;
    public int attackDamage = 10;
    public GameObject projectilePrefab;
    public int projectileCount = 8;
    public float projectileSpeed = 5f;
    public Animator animator;
    public int maxHealth = 100;
    public Color flashColor = Color.red;
    public float flashDuration = 1f;
    public Color lowHealthFlashColor = Color.black;
    public float lowHealthFlashDuration = 0.5f;

    public int currentHealth;
    private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Vector2 direction;
    private Vector2 startPosition;
    private Transform player;
    private bool canAttack = true;
    private bool isFacingRight = true;
    private bool isAttacking = false;

    private bool hasEnteredHalfHealthPhase = false;
    private bool hasEnteredLowHealthPhase = false;

    public LayerMask obstacleLayer; 
    private bool isDead = false; 

    [SerializeField]
    private EventReference constantSoundEvent; // FMOD Event Reference for the constant sound

    private FMOD.Studio.EventInstance constantSoundInstance; // FMOD sound instance
    [SerializeField] private EventReference attackStart;
    [SerializeField] private EventReference releaseStone;



    /// <summary>
    /// Initializes the boss variables and starts its behavior.
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        rb.bodyType = RigidbodyType2D.Kinematic;

        StartCoroutine(ChangeDirection()); // Start changing direction at regular intervals
        FindPlayer(); // Find and assign the player object
    }

    /// <summary>
    /// Handles boss updates including movement, attacks, and health-based behaviors.
    /// </summary>
    private void FixedUpdate()
    {
        if (isDead) return; // Exit if the boss is dead

        if (player != null)
        {
            if (!isAttacking || currentHealth <= maxHealth / 2)
            {
                Move(); // Move the boss
                FacePlayer(); // Ensure the boss is facing the player
            }

            float distanceToPlayer = Vector2.Distance(player.position, rb.position); // Calculate distance to the player
            // Manage constant sound and music when the player enters or exits the attack range
            if (distanceToPlayer <= attackRange)
            {
                // Trigger boss music when the player is within attack range
                AudioManager.instance.SetMusicArea(MusicType.Boss); // No need for a reference, use the singleton
            }
         
            if (distanceToPlayer <= attackRange && !constantSoundInstance.isValid())
            {
                StartConstantSound();
            }
            else if (distanceToPlayer > attackRange && constantSoundInstance.isValid())
            {
                StopConstantSound();
            }
            // Attack the player if within range and allowed to attack
            if (distanceToPlayer <= attackRange && canAttack)
            {
                StartCoroutine(Attack());
            }
        }

        // Transition to different health phases based on current health
        if (!hasEnteredHalfHealthPhase && currentHealth <= maxHealth * 0.5f)
        {
            Debug.Log("Entering Half Health Phase");
            EnterHalfHealthPhase();
        }

        if (!hasEnteredLowHealthPhase && currentHealth <= maxHealth * 0.2f)
        {
            Debug.Log("Entering Low Health Phase");
            EnterLowHealthPhase();
        }
    }

    /// <summary>
    /// Moves the boss in a random direction, handling obstacle collisions.
    /// </summary>
    private void Move()
    {
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            // Calculate the next position based on the current direction
            Vector2 nextPosition = rb.position + direction * speed * Time.fixedDeltaTime;

            // Check for collisions with obstacles
            RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, speed * Time.fixedDeltaTime, obstacleLayer);
            if (hit.collider == null)
            {
                // Move if within the allowed radius from the start position
                if (Vector2.Distance(nextPosition, startPosition) <= movementRadius)
                {
                    rb.MovePosition(nextPosition);
                }
                else
                {
                    direction = -direction; // Reverse direction if out of bounds
                }
            }
            else
            {
                direction = Vector2.Reflect(direction, hit.normal); // Reflect direction upon collision
            }
        }
        else
        {
            // Apply velocity-based movement for non-Kinematic Rigidbody
            Vector2 movement = direction * speed * Time.fixedDeltaTime;
            rb.velocity = movement;
        }
    }

    /// <summary>
    /// Adjusts the boss's facing direction to follow the player.
    /// </summary>
    private void FacePlayer()
    {
        Vector2 directionToPlayer = player.position - transform.position;

        if (directionToPlayer.x > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (directionToPlayer.x < 0 && isFacingRight)
        {
            Flip();
        }
    }


    /// <summary>
    /// Flips the boss's sprite direction.
    /// </summary>
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1; // Flip the sprite horizontally
        transform.localScale = scale;
    }

    /// <summary>
    /// Periodically changes the boss's movement direction.
    /// </summary>
    private IEnumerator ChangeDirection()
    {
        while (true)
        {
            if (!isAttacking || currentHealth <= maxHealth / 2)
            {
                direction = Random.insideUnitCircle.normalized; // Choose a random normalized direction
            }
            yield return new WaitForSeconds(changeDirectionTime); // Wait before changing direction again
        }
    }

    /// <summary>
    /// Executes the boss's attack sequence.
    /// </summary>
    private IEnumerator Attack()
    {
        canAttack = false; // Prevent further attacks during the cooldown
        isAttacking = true; // Mark the boss as currently attacking

        animator.SetTrigger("Attack");
        AudioManager.instance.PlayOneShot(attackStart, this.transform.position);
        float animationDuration = 2.01f; // Wait for the attack animation to complete
        yield return new WaitForSeconds(animationDuration);

        FireProjectiles(); 
        AudioManager.instance.PlayOneShot(releaseStone, this.transform.position);// Play projectile release sound
        yield return new WaitForSeconds(attackCooldown); // Wait for the attack cooldown

        canAttack = true; // Allow attacks again
        isAttacking = false; // Mark the boss as no longer attacking
        animator.SetTrigger("Idle"); // Return to idle animation
    }

    /// <summary>
    /// Fires projectiles in a circular pattern
    /// </summary>
    private void FireProjectiles()
    {
        // Adjust projectile count and speed based on health phase
        int effectiveProjectileCount = projectileCount;

        if (hasEnteredLowHealthPhase)
        {
            effectiveProjectileCount = 28;
        }
        else if (hasEnteredHalfHealthPhase)
        {
            effectiveProjectileCount = 16;
        }

        float effectiveProjectileSpeed = projectileSpeed;

        if (hasEnteredLowHealthPhase)
        {
            effectiveProjectileSpeed = 7f;
        }

        float angleStep = 360f / effectiveProjectileCount; // Calculate angle step between projectiles
        float angle = 0f;

        for (int i = 0; i < effectiveProjectileCount; i++)
        {
            // Calculate the projectile's direction and position
            float projectileDirXPosition = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180);
            float projectileDirYPosition = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180);

            Vector2 projectileVector = new Vector2(projectileDirXPosition, projectileDirYPosition);
            Vector2 projectileMoveDirection = (projectileVector - (Vector2)transform.position).normalized * effectiveProjectileSpeed;
            // Spawn and configure the projectile
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<Rigidbody2D>().velocity = new Vector2(projectileMoveDirection.x, projectileMoveDirection.y);
            // Set projectile rotation to face its movement direction
            float angleToPlayer = Mathf.Atan2(projectileMoveDirection.y, projectileMoveDirection.x) * Mathf.Rad2Deg;
            proj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angleToPlayer));

            angle += angleStep; // Increment angle for the next projectile
        }
    }
    /// <summary>
    /// Finds the player object in the scene.
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure there is a GameObject with the 'Player' tag.");
        }
    }

    /// <summary>
    /// Activates the half-health phase, adjusting boss behavior and projectiles.
    /// </summary>
    private void EnterHalfHealthPhase()
    {
        hasEnteredHalfHealthPhase = true;
        projectileCount = 16;
        Debug.Log("Half Health Phase Active: Projectile Count increased.");
    }

    /// <summary>
    /// Activates the low-health phase, further adjusting boss behavior and visuals.
    /// </summary>
    private void EnterLowHealthPhase()
    {
        hasEnteredLowHealthPhase = true; // Mark that the low-health phase has started
        // Adjust parameters to make the boss more aggressive
        attackCooldown = 1f;
        attackRange = 8f;
        projectileCount = 22;
        projectileSpeed = 7f;
        movementRadius = 7f;
        changeDirectionTime = 0.6f;
        StartCoroutine(FlashBlack());
        Debug.Log("Low Health Phase Active: Adjusted parameters and flashing black.");
    }

    /// <summary>
    /// Flashes the boss's sprite to a specific color and then resets it, used for visual feedback.
    /// </summary>
    private IEnumerator FlashBlack()
    {
        Color originalColor = spriteRenderer.color; // Save the original sprite color
        spriteRenderer.color = lowHealthFlashColor; // Change to the flashing color
        yield return new WaitForSeconds(lowHealthFlashDuration); // Wait for the flash duration
        spriteRenderer.color = originalColor;  // Reset to the original color
    }

    /// <summary>
    /// Sets the current health of the boss and handles death logic if health reaches zero.
    /// </summary>
    /// <param name="health">The new health value to set.</param>
    private void SetCurrentHealth(int health)
    {
        currentHealth = health;
        Debug.Log($"Boss current health set to: {currentHealth}");

        if (currentHealth <= 0 && !isDead) // Check if boss is dead
        {
            isDead = true; // Set dead flag
            StopConstantSound(); // Stop the sound
            StartCoroutine(Die()); // Start dying process
        }
    }

    /// <summary>
    /// Handles the boss's death sequence, including animations and object destruction.
    /// </summary>
    private IEnumerator Die()
    {
        animator.SetTrigger("Die"); // Trigger dying animation
        yield return new WaitForSeconds(2.01f); // Wait for animation to finish 

        Destroy(gameObject); // Destroy the boss game object
    }


    /// <summary>
    /// Starts the boss's constant sound effect, ensuring it is valid.
    /// </summary>
    private void StartConstantSound()
    {
        if (!constantSoundInstance.isValid())
        {
            constantSoundInstance = RuntimeManager.CreateInstance(constantSoundEvent);
            constantSoundInstance.start();
        }
    }
    /// <summary>
    /// Stops the boss's constant sound effect, ensuring proper cleanup.
    /// </summary>
    private void StopConstantSound()
    {
        if (constantSoundInstance.isValid())
        {
            constantSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            constantSoundInstance.release();
        }
    }
    /// <summary>
    /// Ensures that the boss's constant sound effect is stopped when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        StopConstantSound();
    }
}
