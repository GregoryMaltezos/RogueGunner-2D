using System.Collections;
using UnityEngine;

public class CthuluController : MonoBehaviour
{
    public GameObject projectilePrefab; // Prefab for the projectile
    public int rows = 12; // Number of rows of projectiles
    public int columns = 12; // Total projectiles per row
    public float projectileSpeed = 5f; // Speed at which the projectiles move horizontally
    public float spawnDelay = 0.5f; // Delay between firing rows
    public float gapChance = 0.3f; // Chance of leaving a gap (30%)
    public float rowGap = 0.5f; // Gap between each row of projectiles

    public float flyingSpeed = 5f; // Speed at which the boss flies towards the center
    private bool isFlying = false; // Is the boss currently flying?
    private bool isFiring = false; // Is the boss currently firing projectiles?
    private bool isMoving = false; // Is the boss currently moving towards the player
    private bool isAttacking = false; // Is the boss currently in the attack phase

    public float minAttackDelay = 5f; // Minimum delay before the boss attacks
    public float maxAttackDelay = 10f; // Maximum delay before the boss attacks
    public float stayInMiddleDelay = 2f; // Additional time to stay in the middle after firing

    private Transform player; // Reference to the player
    private bool hasEnteredRoom = false; // Track if the player has entered the room

    // Detection radius
    public float detectionRadius = 10f; // Side length of the square detection area
    public bool showDetectionRadius = true; // Toggle to visualize detection area
    private SpriteRenderer spriteRenderer;

    // Buffer distance and attack trigger distance
    public float bufferDistance = 2f; // The distance to maintain from the player
    public float attackTriggerDistance = 1f; // The distance at which to trigger the attack
    public float oscillationAmplitude = 1f; // Amplitude of the vertical movement
    public float oscillationSpeed = 2f; // Speed of the vertical oscillation
    public float verticalOscillationAmplitude = 2f; // Amplitude for vertical projectiles' oscillation
    public float verticalOscillationFrequency = 1f; // Frequency for vertical projectiles' oscillation
    public bool IsFlying => isFlying;
    private Animator animator; // Reference to the Animator component
    public LayerMask obstacleLayer; // Layer mask to detect obstacles
    private float colliderXOffset;
    private BossHp bossHp;
    private bool isDead = false;
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player object has the 'Player' tag.");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Get the X offset from the first collider
        Collider2D firstCollider = GetComponent<Collider2D>();
        if (firstCollider != null)
        {
            colliderXOffset = firstCollider.bounds.extents.x; // Get the X offset
        }
        else
        {
            Debug.LogError("No Collider2D found on the boss.");
        }

        // Find the BossHp component
        bossHp = GetComponent<BossHp>();
        if (bossHp == null)
        {
            Debug.LogError("BossHp component not found on the boss.");
        }

        // Start the flying routine
        StartCoroutine(FlyingRoutine());
    }


    void Update()
    {
        if (bossHp != null && bossHp.CurrentHp <= 0)
        {
            HandleDeath();
            return; // Exit update to stop further actions
        }

        // Check and adjust oscillation parameters based on boss's health
        if (bossHp != null)
        {
            if (bossHp.CurrentHp <= bossHp.MaxHp * 0.2f)
            {
                // Set high oscillation values when health is below 20%
                verticalOscillationAmplitude = 2f;
                verticalOscillationFrequency = 1f;
            }
            else if (bossHp.CurrentHp <= bossHp.MaxHp * 0.65f)
            {
                // Set moderate oscillation values when health is below 65%
                oscillationAmplitude = 2f;
                oscillationSpeed = 1f;
            }
        }

        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
            if (player == null) return;
        }

        // Check if the player is within the room boundary
        if (IsPlayerInRoom())
        {
            // If the player has just entered the room
            if (!hasEnteredRoom)
            {
                hasEnteredRoom = true; // Player has entered the room
                Debug.Log("Player has entered the room.");
            }
        }
        else if (hasEnteredRoom)
        {
            // Player is trying to leave the room after having entered it
            Debug.Log("You have left the room! Teleporting back...");
            TeleportPlayerToCenter();
        }

        // Only move towards the player if they are within the detection radius and not firing, flying, or attacking
        if (hasEnteredRoom && !isFiring && !isFlying && !isAttacking) // Check if the player has entered the room
        {
            MoveTowardsPlayer();
        }

        // Check if the player is trying to leave the room bounds
        CheckPlayerPosition();
    }

    private void CheckPlayerPosition()
    {
        // Define the room's center and boundary size
        Vector2 roomCenter = Vector2.zero;  // Room center at (0, 0)
        float roomBoundary = 10f;  // Half the side length of a 10x10 room

        // Check if the player has left the bounds of the room (±5 on x and y axes from room center)
        if (hasEnteredRoom && (Mathf.Abs(player.position.x - roomCenter.x) > roomBoundary || Mathf.Abs(player.position.y - roomCenter.y) > roomBoundary))
        {
            Debug.Log("Player is out of bounds! Teleporting back to the center...");
            TeleportPlayerToCenter();
        }
    }



    private float damageCooldown = 2f; // Time in seconds before damage can be applied again
    private float lastDamageTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Skip damage if the boss is flying
            if (isFlying) return; // Boss is invincible while flying

            if (Time.time - lastDamageTime < damageCooldown)
            {
                return; // Exit if cooldown hasn't expired
            }

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // Check if the player is invincible before applying damage
            if (PlayerController.instance != null && PlayerController.instance.isInvincible)
            {
                return; // Do not apply damage if the player is invincible
            }

            // Apply damage to the player
            if (playerHealth != null)
            {
                float damageAmount = 22f; // Adjust damage amount as needed
                playerHealth.TakeDamage(damageAmount); // Call the TakeDamage method
                lastDamageTime = Time.time; // Update the last damage time
                Debug.Log("Player damaged by boss!");
            }
        }
    }
    private bool wasFlying = false;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if collided with a bullet
        if (collision.collider.CompareTag("FrBullet"))
        {
            // Skip hit animation if the boss is already dead
            if (isDead)
            {
                return; // Boss is dead, do nothing
            }

            // Cancel the hit animation if attacking
            if (isAttacking)
            {
                CancelHitAnimation(); // Cancel hit animation if attacking
                return; // Don't play hit animation
            }

            // Only set the hurt animation if the boss wasn't flying recently
            if (!isFlying)
            {
                animator.SetTrigger("Hit");
                StartCoroutine(ReturnToWalking(0.5f)); // Wait for half a second before returning to walking
            }
        }
    }

    private void CancelHitAnimation()
    {
        // If hit animation is playing, you may want to reset the trigger or handle it as needed
        animator.ResetTrigger("Hit"); // Resets the hit trigger; adjust if your animator uses a different approach
        // Here you might want to also trigger a different state or animation
    }


    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Check the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float closeDistanceThreshold = 5f;

        if (distanceToPlayer < closeDistanceThreshold)
        {
            float offset = 2f;
            Vector2 targetPosition;

            if (transform.position.x < player.position.x)
                targetPosition = new Vector2(player.position.x - offset, player.position.y);
            else
                targetPosition = new Vector2(player.position.x + offset, player.position.y);

            if (Vector2.Distance(transform.position, player.position) <= attackTriggerDistance)
            {
                TriggerAttack();
            }
            else
            {
                if (Vector2.Distance(targetPosition, player.position) > bufferDistance)
                    direction = (targetPosition - (Vector2)transform.position).normalized;
                else
                    direction = Vector2.zero;
            }
        }
        else
        {
            direction = (player.position - transform.position).normalized;
        }

        if (direction != Vector2.zero)
        {
            // Use Raycast to detect obstacles before moving
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, flyingSpeed * Time.deltaTime, obstacleLayer);
            if (hit.collider == null) // Only move if no obstacle in the way
            {
                transform.position += (Vector3)(direction * flyingSpeed * Time.deltaTime);

                // Update sprite flip based on movement direction
                if (Mathf.Abs(direction.x) > 0.1f)
                {
                    spriteRenderer.flipX = direction.x < 0; // Flip sprite to face the movement direction

                    // Optional: If you have colliders that should also react to the flip
                    foreach (Transform child in transform)
                    {
                        // Optionally adjust child collider settings here
                        // Example: child.GetComponent<Collider2D>().isTrigger = !child.GetComponent<Collider2D>().isTrigger;
                    }
                }

                if (!isMoving)
                {
                    isMoving = true;
                    TriggerWalkingAnimation();
                }
            }
            else
            {
                // If there's an obstacle, stop movement
                StopWalkingAnimation();
            }
        }
        else
        {
            StopWalkingAnimation();
        }
    }



    private void TriggerAttack()
    {
        if (!isAttacking) // Only trigger if not currently attacking
        {
            isAttacking = true; // Set attacking state

            // Check the facing direction and set the appropriate trigger
            if (spriteRenderer.flipX) // If facing left
            {
                animator.SetTrigger("AttackL"); // Trigger left attack animation
            }
            else // If facing right
            {
                animator.SetTrigger("Attack"); // Trigger right attack animation
            }

            Debug.Log("Attack Triggered!");

            float walkingAnimationDelay = 0.6f; // Set your desired delay here
            StartCoroutine(AttackAndRetreatRoutine(walkingAnimationDelay)); // Pass the delay to the routine
        }
    }


    private IEnumerator AttackAndRetreatRoutine(float walkingAnimationDelay)
    {
        // Wait for the attack animation duration
        float attackDuration = 0.6f; // Adjust this based on the length of your attack animation
        yield return new WaitForSeconds(attackDuration); // Wait for the attack to finish

        // Delay before starting to walk again
        yield return new WaitForSeconds(walkingAnimationDelay); // Add your custom delay here

        // Now set the walking animation
        TriggerWalkingAnimation(); // Start walking animation

        // Start moving away from the player
        yield return StartCoroutine(MoveAwayFromPlayer(2f)); // Move away for 2 seconds

        TriggerBreathingAnimation();

        // Wait for the breathing animation to finish (assuming 1.5 seconds for breathing)
        float breathingDuration = 2.5f; // Adjust based on breathing animation length
        yield return new WaitForSeconds(breathingDuration);

        // Reset the attacking state after retreating
        isAttacking = false; // Allow attacking again after moving away
    }

    private IEnumerator ReturnToWalking(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Only trigger walking animation if not attacking
        if (!isAttacking)
        {
            TriggerWalkingAnimation();
        }
    }

    private IEnumerator MoveAwayFromPlayer(float duration)
    {
        float elapsed = 0f;
        Vector2 retreatDirection = (transform.position - player.position).normalized;
        spriteRenderer.flipX = retreatDirection.x < 0;

        float roomBoundary = 9.5f;

        while (elapsed < duration)
        {
            Vector2 newPosition = (Vector2)transform.position + (retreatDirection * flyingSpeed * Time.deltaTime);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, retreatDirection, flyingSpeed * Time.deltaTime, obstacleLayer);

            if (hit.collider == null && Mathf.Abs(newPosition.x) <= roomBoundary && Mathf.Abs(newPosition.y) <= roomBoundary)
            {
                transform.position = newPosition;
                elapsed += Time.deltaTime;
            }
            else
            {
                break; // Stop moving if an obstacle is detected
            }

            yield return null;
        }

        StopWalkingAnimation();
    }




    private IEnumerator FlyingRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minAttackDelay, maxAttackDelay)); // Random wait before flying

            if (bossHp != null)
            {
                bossHp.SetCanTakeDamage(false); // Disable damage while flying
            }

            isFlying = true; // Set flying state
            TriggerFlyingAnimation(); // Start flying animation

            // Fly to the center (0,0)
            Vector2 targetPosition = Vector2.zero;
            while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
            {
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                transform.position += (Vector3)(direction * flyingSpeed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }

            // Start firing projectiles immediately after reaching the center
            isFiring = true; // Set firing state
            yield return StartCoroutine(SpawnProjectilesRoutine());

            // Additional attack if health is below 40%
            if (bossHp != null && bossHp.CurrentHp <= bossHp.MaxHp * 0.4f)
            {
                yield return StartCoroutine(SpawnVerticalProjectilesRoutine());
            }

            // Continue to stay in the middle while still flying
            yield return new WaitForSeconds(stayInMiddleDelay); // Stay in the middle for additional time

            // Reset flying state and enable damage
            if (bossHp != null)
            {
                bossHp.SetCanTakeDamage(true); // Re-enable damage after flying
            }

            isFiring = false; // Reset firing state
            isFlying = false; // Exit flying state
            StopFlyingAnimation(); // Stop the flying animation
            StopBreathingAnimation();
            TriggerWalkingAnimation(); // Transition to walking after flying
            isMoving = true; // Set moving state to true for walking

            // If the boss was flying and has just landed, don't play hurt animation
            if (wasFlying)
            {
                wasFlying = false; // Reset flag
                continue; // Skip this iteration
            }
        }
    }

    private IEnumerator SpawnVerticalProjectilesRoutine()
    {
        int columns = 10;             // Number of projectiles in each row
        int totalRows = 15;           // Total rows to spawn
        float xStart = -9.5f;         // Start of horizontal range
        float xEnd = 9.5f;            // End of horizontal range
        float yPos = 9.5f;            // Initial Y position (top of the screen)
        float xGap = (xEnd - xStart) / (columns - 1); // Horizontal gap between projectiles
        float rowSpacing = 1.0f;      // Distance between rows on the Y-axis

        for (int row = 0; row < totalRows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                float xPos = xStart + (col * xGap); // Calculate X position for each column

                if (ShouldSpawnProjectile(col)) // Optional: if you have conditions for spawning
                {
                    GameObject projectile = Instantiate(projectilePrefab, new Vector2(xPos, yPos), Quaternion.Euler(0, 0, 90)); // Rotate 90 degrees left
                    Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

                    if (rb != null)
                    {
                        // Apply the oscillation effect with the specific vertical oscillation parameters
                        StartCoroutine(OscillateVerticalProjectile(rb, xPos, yPos));
                    }
                    else
                    {
                        Debug.LogError("Projectile prefab is missing Rigidbody2D component.");
                    }
                }
            }

            yPos += rowSpacing; // Move Y position down for the next row
            yield return new WaitForSeconds(spawnDelay); // Delay between each row spawn
        }
    }

    // Coroutine to apply the oscillation effect for vertical projectiles
    private IEnumerator OscillateVerticalProjectile(Rigidbody2D rb, float initialX, float initialY)
    {
        float timer = 0f;

        while (rb != null)
        {
            // Calculate new position with oscillation on X-axis
            float xOscillation = initialX + Mathf.Sin(timer * verticalOscillationFrequency) * verticalOscillationAmplitude;
            rb.position = new Vector2(xOscillation, rb.position.y - (projectileSpeed * Time.deltaTime)); // Move down and oscillate

            timer += Time.deltaTime;
            yield return null;
        }
    }
    public void SetVerticalOscillation(float amplitude, float frequency)
    {
        verticalOscillationAmplitude = amplitude;
        verticalOscillationFrequency = frequency;
    }



    private IEnumerator SpawnProjectilesRoutine()
    {
        // Start firing all rows at the same time
        for (int row = 0; row < rows; row++)
        {
            StartCoroutine(SpawnProjectiles(row)); // Start firing projectiles in this row
        }

        yield return new WaitForSeconds(spawnDelay); // Optional delay before the next firing sequence
    }
    private IEnumerator SpawnProjectiles(int row)
    {
        float minY = -9f; // Minimum Y value for the last row
        float maxY = 9f;  // Maximum Y value for the first row
        float totalHeight = maxY - minY - (rowGap * (rows - 1)); // Subtract total gap height
        float yPos = Mathf.Lerp(maxY, minY, (float)row / (rows - 1)); // Spread evenly between maxY and minY

        for (int col = 0; col < columns; col++)
        {
            if (ShouldSpawnProjectile(col))
            {
                float xPos = 11f; // Start from the right side
                GameObject projectile = Instantiate(projectilePrefab, new Vector2(xPos, yPos), Quaternion.identity);
                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    // Set up projectile to move leftward and oscillate
                    StartCoroutine(MoveProjectile(rb, yPos, row));
                }
                else
                {
                    Debug.LogError("Projectile prefab is missing Rigidbody2D component.");
                }
            }
            yield return new WaitForSeconds(0.2f); // Adjust delay as needed
        }
    }

    private IEnumerator MoveProjectile(Rigidbody2D rb, float initialYPos, int row)
    {
        float time = 0f;
        while (rb != null) // Continue moving as long as the projectile exists
        {
            // Move horizontally (left)
            Vector2 velocity = new Vector2(-projectileSpeed, 0);

            // Calculate vertical oscillation based on sine wave
            float yOffset = Mathf.Sin(time * oscillationSpeed + row) * oscillationAmplitude;
            Vector2 newPosition = new Vector2(rb.position.x, initialYPos + yOffset);

            // Apply the movement
            rb.MovePosition(newPosition + velocity * Time.fixedDeltaTime);

            time += Time.fixedDeltaTime; // Update time for sine wave calculation
            yield return new WaitForFixedUpdate();
        }
    }
    private bool ShouldSpawnProjectile(int col)
    {
        return Random.value > gapChance; // Chance to skip projectile
    }




    private void StartFiringProjectiles()
    {
        if (!isFiring)
        {
            isFiring = true; // Set firing state
            StartCoroutine(FireProjectiles()); // Start firing coroutine
        }
    }

    private IEnumerator FireProjectiles()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Randomly decide to create a gap
                if (Random.value > gapChance)
                {
                    // Instantiate the projectile
                    GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                    Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.velocity = new Vector2((col - (columns - 1) / 2f) * projectileSpeed, -row * rowGap);
                    }
                }
            }
            yield return new WaitForSeconds(spawnDelay); // Delay between firing rows
        }
        isFiring = false; // Reset firing state after completing the firing sequence
    }

    private bool IsPlayerInRoom()
    {
        // Check if the player's position is within the square room centered at (0,0)
        Vector2 roomCenter = Vector2.zero;  // Room center at (0,0)
        float roomBoundary = 10f;  // Half the side length of a 10x10 room

        return Mathf.Abs(player.position.x - roomCenter.x) <= roomBoundary &&
               Mathf.Abs(player.position.y - roomCenter.y) <= roomBoundary;
    }




    private void HandleDeath()
    {
        if (!isDead)
        {
            isDead = true;
            StopAllCoroutines(); // Stop all active coroutines
            StopBossActions(); // Stop all other boss actions

            // Ensure we are interrupting any animations
            animator.ResetTrigger("Hit"); // Reset the hit state if needed
            animator.SetTrigger("Die"); // Play death animation
            Debug.Log("Boss has died!");
        }
    }

    private void StopBossActions()
    {
        // Reset all action states to ensure no further actions are taken
        isFlying = false;
        isFiring = false;
        isMoving = false;
        isAttacking = false;
        isBreathing = false;

        // Disable the collider in the child object
        Collider2D childCollider = GetComponentInChildren<Collider2D>();
        if (childCollider != null)
        {
            childCollider.enabled = false; // Disable the child collider
        }
        else
        {
            Debug.LogWarning("No Collider2D found in children of the boss.");
        }
        StopBreathingAnimation();
        StopWalkingAnimation();
        StopFlyingAnimation(); 
    }



    private float breathingDurationLeft = 0f;
    private bool isBreathing = false;
    private void TriggerBreathingAnimation()
    {
        if (!isBreathing) // Only start breathing if not already breathing
        {
            isBreathing = true;
            animator.SetTrigger("IsBreathing"); // Trigger breathing animation
            breathingDurationLeft = 2.5f; // Set the breathing animation duration (adjust based on your animation length)
            StartCoroutine(BreathingWaitCoroutine()); // Start coroutine to track breathing duration
        }
    }
    private IEnumerator BreathingWaitCoroutine()
    {
        // While there's breathing time left, wait each frame
        while (breathingDurationLeft > 0f)
        {
            breathingDurationLeft -= Time.deltaTime;
            yield return null;
        }

        // Breathing is done, so stop the animation and transition to walking
        StopBreathingAnimation();
        TriggerWalkingAnimation();
    }

    private void StopBreathingAnimation()
    {
        isBreathing = false; // Reset breathing state
        animator.ResetTrigger("IsBreathing"); // Reset breathing animation trigger
    }
   

    private void TeleportPlayerToCenter()
    {
        player.position = new Vector2(0, 0); // Teleport the player back to center
    }

    private void TriggerWalkingAnimation()
    {
        animator.SetTrigger("IsWalking"); // Trigger walking animation
    }

    private void StopWalkingAnimation()
    {
        animator.ResetTrigger("IsWalking"); // Reset walking animation trigger
        isMoving = false; // Reset moving state
    }

    private void TriggerFlyingAnimation()
    {
        animator.SetTrigger("IsFlying"); // Trigger flying animation
    }

    private void StopFlyingAnimation()
    {
        animator.ResetTrigger("IsFlying"); // Reset flying animation trigger
    }
}