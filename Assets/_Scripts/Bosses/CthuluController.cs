using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.UI;
public class CthuluController : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Prefab for the projectile
    public int rows = 12; // Number of rows of projectiles
    public int columns = 12; // Total projectiles per row
    public float projectileSpeed = 5f; // Speed at which the projectiles move horizontally
    public float spawnDelay = 0.5f; // Delay between firing rows
    public float gapChance = 0.3f; // Chance of leaving a gap (30%)
    public float rowGap = 0.5f; // Gap between each row of projectiles

    // Boss Movement and Behavior Settings
    [Header("Boss Movement and Behavior Settings")]
    public float flyingSpeed = 5f; // Speed at which the boss flies towards the center
    private bool isFlying = false; // Is the boss currently flying?
    private bool isFiring = false; // Is the boss currently firing projectiles?
    private bool isMoving = false; // Is the boss currently moving towards the player
    private bool isAttacking = false; // Is the boss currently in the attack phase

    public float minAttackDelay = 5f; // Minimum delay before the boss attacks
    public float maxAttackDelay = 10f; // Maximum delay before the boss attacks
    public float stayInMiddleDelay = 2f; // Additional time to stay in the middle after firing

    // UI message for when the player leaves the room
    public string leaveRoomMessage = "You have left the room!"; // Message to show
    public Font customFont; // Assign font in the inspector
    public int textSize = 70; // Font size that can be changed in the inspector
    private GameObject leaveMessageObject; // The message UI object
    private Canvas canvas; // Reference to the canvas where the message will appear

    // Player Detection Settings
    [Header("Player Detection Settings")]
    private Transform player; // Reference to the player
    private bool hasEnteredRoom = false; // Track if the player has entered the room

    public float detectionRadius = 10f; // Side length of the square detection area
    public bool showDetectionRadius = true; // Toggle to visualize detection area
    private SpriteRenderer spriteRenderer;

    // Buffer and Attack Settings
    [Header("Buffer and Attack Settings")]
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
    private PlayerHealth playerHealth;
    // Damage Settings
    [Header("Damage Settings")]
    private float damageCooldown = 2f; // Time in seconds before damage can be applied again
    private float lastDamageTime;

    [Header("FMOD Sound Settings")]
    public string bossMoveEvent = "event:/SFX/Enemy/Bosses/Cthulu/CWalk"; // FMOD event path
    [SerializeField] private EventReference fly;
    [SerializeField] private EventReference attack;
    private FMOD.Studio.EventInstance flySoundInstance;
    private EventInstance moveSoundInstance; // FMOD event instance
    private bool isSoundPlaying = false; // Flag to check if sound is playing

    /// <summary>
    /// Initializes the boss properties and sets up initial states, such as finding the player and boss health components.
    /// Starts the boss flying routine.
    /// </summary>
    void Start()
    {
        GameObject canvasObject = GameObject.Find("BossCanvas");
        if (canvasObject != null)
        {
            canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("BossCanvas found, but it does not have a Canvas component.");
            }
        }
        else
        {
            Debug.LogError("No Canvas named 'BossCanvas' found in the scene. Please add one.");
        }

        // Find the player object by tag and retrieve its health component
        player = GameObject.FindWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
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
        // Set the music type for the boss area
        AudioManager.instance.SetMusicArea(MusicType.Boss);
        // Start the flying routine
        StartCoroutine(FlyingRoutine());
    }

    /// <summary>
    /// Handles the boss's behavior each frame, including checking 
    /// for player health and adjusting oscillation values based on boss's health.
    /// </summary>
    void Update()
    {
        // Check if the boss is dead and handle death
        if (bossHp != null && bossHp.CurrentHp <= 0)
        {
            HandleDeath();
            return; // Exit update to stop further actions
        }
        // If the player is dead, stop all boss actions
        if (playerHealth != null && playerHealth.currentHealth <= 0)
        {
            StopBossActions(); // Stop all actions of the boss
            return; // Exit early to prevent further actions
        }
        // Adjust oscillation behavior based on the boss's health percentage
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
        // Check and update player reference if it is null
        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
            if (player == null) return;
        }

        // Check if the player is within the room boundary and handle entering/leaving the room
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
            ShowLeaveMessage();
            TeleportPlayerToCenter(); // Teleport player back to room center
        }

        // Move towards the player if they are within the room and boss is not currently performing other actions
        if (hasEnteredRoom && !isFiring && !isFlying && !isAttacking) // Check if the player has entered the room
        {
            MoveTowardsPlayer();
        }

        // Check if the player is trying to leave the room bounds
        CheckPlayerPosition();
    }

    /// <summary>
    /// Displays a leave message on the screen, adds a shake effect to it, 
    /// and hides the message after a short delay.
    /// </summary>
    private void ShowLeaveMessage()
    {
        if (canvas == null) return;

        // Create a new UI Text object for the message
        leaveMessageObject = new GameObject("LeaveMessage");
        leaveMessageObject.transform.SetParent(canvas.transform);

        // Add and configure the Text component
        Text textComponent = leaveMessageObject.AddComponent<Text>();
        textComponent.text = leaveRoomMessage;
        textComponent.font = customFont; // Use the custom font
        textComponent.fontSize = textSize; // Set the size of the text from the inspector
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;

        // Set up RectTransform for positioning
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(900, 100); // Adjust size of the message box

        // Position the message slightly above the middle of the screen
        float positionY = Screen.height / 2 + 250f; // 250f offset above the middle (adjust this value as needed)
        rectTransform.position = new Vector3(Screen.width / 2, positionY, 0); // Centered horizontally, slightly above vertically

        // Start the shake effect coroutine
        StartCoroutine(ShakeMessage(rectTransform));

        // Start coroutine to hide the message after 2 seconds
        StartCoroutine(HideMessageAfterDelay());
    }
    /// <summary>
    /// Coroutine that shakes the message up and down for a brief period.
    /// </summary>
    /// <param name="rectTransform">The RectTransform of the message to shake.</param>
    private IEnumerator ShakeMessage(RectTransform rectTransform)
    {
        float shakeDuration = 1f; // Duration of the shake effect
        float shakeAmount = 15f; // Amount of shaking
        float shakeSpeed = 35f; // Speed of shaking

        Vector3 originalPosition = rectTransform.position;
        float elapsedTime = 0f;

        // Shake the message up and down
        while (elapsedTime < shakeDuration)
        {
            float shakeOffset = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount; // Sin wave for up and down motion
            rectTransform.position = originalPosition + new Vector3(0, shakeOffset, 0);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        // Ensure the message settles at the center after shaking
        rectTransform.position = originalPosition;
    }
    /// <summary>
    /// Coroutine that hides the message after a delay by destroying the message object.
    /// </summary>
    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        Destroy(leaveMessageObject); // Destroy the message object
    }


    /// <summary>
    /// Checks the player's position relative to the room boundaries and teleports the player if out of bounds.
    /// </summary>
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




    /// <summary>
    /// Detects collision with the player and applies damage if conditions allow (such as no flying or invincibility).
    /// </summary>
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

    /// <summary>
    /// Handles the boss's collision with a bullet, triggering an appropriate animation or action.
    /// </summary>
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

    /// <summary>
    /// Resets the hit animation trigger and handles any necessary animation resets.
    /// </summary>
    private void CancelHitAnimation()
    {
        animator.ResetTrigger("Hit"); // Resets the hit trigger
    }

    /// <summary>
    /// Moves the boss towards the player, considering obstacles and adjusting animation and sound effects.
    /// </summary>
    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Check the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float closeDistanceThreshold = 5f;

        bool isMovingTowardsPlayer = false;
        // Adjust behavior if the player is close
        if (distanceToPlayer < closeDistanceThreshold)
        {
            float offset = 2f;
            Vector2 targetPosition;

            if (transform.position.x < player.position.x)
                targetPosition = new Vector2(player.position.x - offset, player.position.y);
            else
                targetPosition = new Vector2(player.position.x + offset, player.position.y);
            // Trigger attack if within attack range
            if (Vector2.Distance(transform.position, player.position) <= attackTriggerDistance)
            {
                TriggerAttack();
            }
            else
            {
                // Adjust movement if too close to target position
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
            // Move the boss towards the player if there is no obstacle
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, flyingSpeed * Time.deltaTime, obstacleLayer);

            if (hit.collider == null) // No obstacles
            {
                transform.position += (Vector3)(direction * flyingSpeed * Time.deltaTime);

                // Update the sprite flip based on direction
                spriteRenderer.flipX = direction.x < 0;

                if (!isMoving)
                {
                    isMoving = true;
                    TriggerWalkingAnimation();
                }

                // Play FMOD sound if not playing already
                if (!isSoundPlaying)
                {
                    // Create and start the FMOD sound event
                    moveSoundInstance = FMODUnity.RuntimeManager.CreateInstance(bossMoveEvent);
                    moveSoundInstance.start(); // Start the sound
                    isSoundPlaying = true;
                }
            }
        }
        else
        {
            // Stop the FMOD sound if the boss stops moving
            if (isSoundPlaying)
            {
                moveSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); // Stop the sound with fade-out
                isSoundPlaying = false;
            }

            StopWalkingAnimation();
        }
    }

    /// <summary>
    /// Cleans up the FMOD sound instance when the boss is destroyed.
    /// </summary>

    private void OnDestroy()
    {
        if (moveSoundInstance.isValid())
        {
            moveSoundInstance.release(); // Release the event instance
        }
    }

    /// <summary>
    /// Plays a sound with a specified delay after an attack.
    /// </summary>
    private IEnumerator PlaySoundWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        AudioManager.instance.PlayOneShot(attack, this.transform.position); // Play the sound after the delay
    }
    /// <summary>
    /// Triggers an attack animation and sound, and initiates the attack and retreat logic.
    /// </summary>
    private void TriggerAttack()
    {
        if (!isAttacking) // Only trigger if not currently attacking
        {
            isAttacking = true; // Set attacking state

            // Pause the sound when the attack starts
            PauseMoveSound();

            // Check the facing direction and set the appropriate trigger
            if (spriteRenderer.flipX) // If facing left
            {
                animator.SetTrigger("AttackL"); // Trigger left attack animation
                StartCoroutine(PlaySoundWithDelay(0.25f)); // Start the coroutine to play the sound after a delay
            }
            else // If facing right
            {
                animator.SetTrigger("Attack"); // Trigger right attack animation
                StartCoroutine(PlaySoundWithDelay(0.25f));
            }

            Debug.Log("Attack Triggered!");

            // Set attack state and start retreating logic after attack
            StartCoroutine(AttackAndRetreatRoutine(0.6f)); // Pass the delay to the routine
        }
    }

    /// <summary>
    /// Pauses the move sound if it's currently playing and the entity is not flying.
    /// </summary>
    private void PauseMoveSound()
    {
        if (isSoundPlaying && !isFlying) // Only pause if not flying
        {
            moveSoundInstance.setPaused(true); // Pause the sound
        }
    }

    /// <summary>
    /// Resumes the move sound if it's currently paused and the entity is not flying.
    /// </summary>
    private void ResumeMoveSound()
    {
        if (isSoundPlaying && !isFlying) // Only resume if not flying
        {
            moveSoundInstance.setPaused(false); // Resume the sound
        }
    }

    /// <summary>
    /// Coroutine that handles the attack and retreat routine, including animations and sound.
    /// </summary>
    /// <param name="walkingAnimationDelay">The delay before triggering the walking animation after attack.</param>
    private IEnumerator AttackAndRetreatRoutine(float walkingAnimationDelay)
{
    // Wait for the attack animation duration
    float attackDuration = 0.6f; // Adjust this based on the length of  attack animation
    yield return new WaitForSeconds(attackDuration); // Wait for the attack to finish

    // Delay before starting to walk again
    yield return new WaitForSeconds(walkingAnimationDelay); // custom delay here
        ResumeMoveSound();
        // Now set the walking animation
        TriggerWalkingAnimation(); // Start walking animation

    // Start moving away from the player
    yield return StartCoroutine(MoveAwayFromPlayer(2f)); // Move away for 2 seconds

    // After moving away, resume the sound
    TriggerBreathingAnimation();
    // Wait for the breathing animation to finish 
    float breathingDuration = 2.5f; // Adjust based on breathing animation length
    yield return new WaitForSeconds(breathingDuration);

    // Reset the attacking state after retreating
    isAttacking = false; // Allow attacking again after moving away
}

    /// <summary>
    /// Coroutine that returns to walking after a specified delay.
    /// </summary>
    /// <param name="delay">The delay before starting the walking animation.</param>
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

    /// <summary>
    /// Coroutine that movesboss away from the player for a specified duration.
    /// </summary>
    /// <param name="duration">How long to move away from the player.</param>
    private IEnumerator MoveAwayFromPlayer(float duration)
    {
        float elapsed = 0f;
        Vector2 retreatDirection = (transform.position - player.position).normalized; // Direction to move away from player
        spriteRenderer.flipX = retreatDirection.x < 0; // Flip sprite based on movement direction

        float roomBoundary = 9.5f; // Room boundary limit

        while (elapsed < duration)
        {
            Vector2 newPosition = (Vector2)transform.position + (retreatDirection * flyingSpeed * Time.deltaTime);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, retreatDirection, flyingSpeed * Time.deltaTime, obstacleLayer); // Check for obstacles
            // If no obstacle is detected and within boundary, update position
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



    /// <summary>
    /// Coroutine that handles the flying routine, including attacks, movement, and animation.
    /// </summary>
    private IEnumerator FlyingRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minAttackDelay, maxAttackDelay)); // Random wait before flying

            if (bossHp != null)
            {
                bossHp.SetCanTakeDamage(false); // Disable damage while flying
            }
            PauseMoveSound();
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
            ResumeMoveSound();
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

    /// <summary>
    /// Coroutine to spawn vertical projectiles in a grid pattern.
    /// </summary>
    private IEnumerator SpawnVerticalProjectilesRoutine()
    {
        int columns = 10;             // Number of projectiles in each row
        int totalRows = 12;           // Total rows to spawn
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

                if (ShouldSpawnProjectile(col)) 
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


    /// <summary>
    /// Coroutine to apply vertical oscillation to a projectile as it moves down.
    /// </summary>
    /// <param name="rb">The Rigidbody2D component of the projectile.</param>
    /// <param name="initialX">The initial X position of the projectile.</param>
    /// <param name="initialY">The initial Y position of the projectile.</param>
    private IEnumerator OscillateVerticalProjectile(Rigidbody2D rb, float initialX, float initialY)
    {
        float timer = 0f;

        while (rb != null)
        {
            // Calculate new position with oscillation on X-axis
            float xOscillation = initialX + Mathf.Sin(timer * verticalOscillationFrequency) * verticalOscillationAmplitude;
            rb.position = new Vector2(xOscillation, rb.position.y - (projectileSpeed * Time.deltaTime)); // Apply oscillation and move down

            timer += Time.deltaTime; // Update timer for oscillation
            yield return null; 
        }
    }
    /// <summary>
    /// Sets the vertical oscillation parameters for projectiles.
    /// </summary>
    /// <param name="amplitude">The amplitude of the oscillation.</param>
    /// <param name="frequency">The frequency of the oscillation.</param>
    public void SetVerticalOscillation(float amplitude, float frequency)
    {
        verticalOscillationAmplitude = amplitude;
        verticalOscillationFrequency = frequency;
    }


    /// <summary>
    /// Coroutine to spawn projectiles in rows across the screen.
    /// </summary>
    private IEnumerator SpawnProjectilesRoutine()
    {
        // Start firing all rows at the same time
        for (int row = 0; row < rows; row++)
        {
            StartCoroutine(SpawnProjectiles(row)); // Start firing projectiles in this row
        }

        yield return new WaitForSeconds(spawnDelay); //  delay before the next firing sequence
    }

    /// <summary>
    /// Coroutine to spawn projectiles in a specific row across the screen.
    /// </summary>
    /// <param name="row">The row number for projectile spawning.</param>
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

    /// <summary>
    /// Coroutine to move a projectile across the screen, including oscillation.
    /// </summary>
    /// <param name="rb">The Rigidbody2D component of the projectile.</param>
    /// <param name="initialYPos">The initial Y position of the projectile.</param>
    /// <param name="row">The row index of the projectile (used for oscillation calculation).</param>
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

    /// <summary>
    /// Determines whether a projectile should spawn based on the column index and chance.
    /// </summary>
    /// <param name="col">The column index for spawning projectiles.</param>
    /// <returns>True if a projectile should spawn, otherwise false.</returns>
    private bool ShouldSpawnProjectile(int col)
    {
        return Random.value > gapChance; // Chance to skip projectile
    }



    /// <summary>
    /// Starts firing projectiles if not already firing.
    /// </summary>
    private void StartFiringProjectiles()
    {
        if (!isFiring)
        {
            isFiring = true; // Set firing state
            StartCoroutine(FireProjectiles()); // Start firing coroutine
        }
    }

    /// <summary>
    /// Coroutine to handle firing projectiles in rows across the screen.
    /// </summary>
    private IEnumerator FireProjectiles()
    {
        // Loop through rows of projectiles
        for (int row = 0; row < rows; row++)
        {
            // Loop through columns for each row
            for (int col = 0; col < columns; col++)
            {
                // Randomly decide whether to create a gap between projectiles
                if (Random.value > gapChance)
                {
                    // Instantiate the projectile at the boss's position
                    GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                    Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        // Set projectile velocity based on column and row
                        rb.velocity = new Vector2((col - (columns - 1) / 2f) * projectileSpeed, -row * rowGap);
                    }
                }
            }
            yield return new WaitForSeconds(spawnDelay); // Delay between firing rows
        }
        isFiring = false; // Reset firing state after completing the firing sequence
    }

    /// <summary>
    /// Checks if the player is within the room boundary (centered at (0, 0)).
    /// </summary>
    /// <returns>True if the player is within the room, false otherwise.</returns>
    private bool IsPlayerInRoom()
    {
        Vector2 roomCenter = Vector2.zero;  // Room center at (0,0)
        float roomBoundary = 10f;  // Half the side length of a 10x10 room
        // Check if player's position is within the defined boundaries
        return Mathf.Abs(player.position.x - roomCenter.x) <= roomBoundary &&
               Mathf.Abs(player.position.y - roomCenter.y) <= roomBoundary;
    }



    /// <summary>
    /// Handles the boss's death sequence, stopping all coroutines and animations.
    /// </summary>
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

    /// <summary>
    /// Stops all boss actions such as movement, flying, and attacking.
    /// </summary>
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
        StopMoveSound();
    }



    private float breathingDurationLeft = 0f;
    private bool isBreathing = false;
    /// <summary>
    /// Triggers the breathing animation and starts the breathing wait coroutine.
    /// </summary>
    private void TriggerBreathingAnimation()
    {
        if (!isBreathing) // Only start breathing if not already breathing
        {
            isBreathing = true;
            animator.SetTrigger("IsBreathing"); // Trigger breathing animation
            breathingDurationLeft = 2.5f; // Set the breathing animation duration (adjust based on your animation length)
            StartCoroutine(BreathingWaitCoroutine()); // Start coroutine to track breathing duration

            // Stop the FMOD sound when the boss starts breathing
            if (isSoundPlaying)
            {
                moveSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); // Stop the sound immediately
                isSoundPlaying = false;
            }
        }
    }


    /// <summary>
    /// Starts the movement sound for the boss when it is moving.
    /// </summary>
    private void StartMoveSound()
    {
        if (!isSoundPlaying && !isFlying) // Ensure sound only plays when not flying
        {
            // Create and start the FMOD sound event
            moveSoundInstance = FMODUnity.RuntimeManager.CreateInstance("event:/BossMoveSound");
            moveSoundInstance.start(); // Start the sound
            isSoundPlaying = true;
        }
    }
    /// <summary>
    /// Stops the movement sound for the boss.
    /// </summary>
    private void StopMoveSound()
    {
        if (isSoundPlaying)
        {
            moveSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); // Stop the sound with fade-out
            isSoundPlaying = false;
        }
    }
    /// <summary>
    /// Coroutine to wait for the breathing animation to complete, then trigger walking animation.
    /// </summary>
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

        // If the boss is still moving, restart the sound
        if (isMoving)
        {
            StartMoveSound();  // Re-start the movement sound if the boss is still moving
        }
    }

    /// <summary>
    /// Stops the breathing animation and resets the breathing state.
    /// </summary>
    private void StopBreathingAnimation()
    {
        isBreathing = false; // Reset breathing state
        animator.ResetTrigger("IsBreathing"); // Reset breathing animation trigger
    }

    /// <summary>
    /// Teleports the player to the center of the room (0, 0).
    /// </summary>
    private void TeleportPlayerToCenter()
    {
        player.position = new Vector2(0, 0); // Teleport the player back to center
    }
    /// <summary>
    /// Triggers the walking animation for the boss.
    /// </summary>
    private void TriggerWalkingAnimation()
    {
        animator.SetTrigger("IsWalking"); // Trigger walking animation
    }
    /// <summary>
    /// Stops the walking animation and resets the walking state.
    /// </summary>
    private void StopWalkingAnimation()
    {
        animator.ResetTrigger("IsWalking"); // Reset walking animation trigger
        isMoving = false; // Reset moving state
    }
    /// <summary>
    /// Triggers the flying animation for the boss and starts the fly sound.
    /// </summary>
    private void TriggerFlyingAnimation()
    {
        animator.SetTrigger("IsFlying"); // Trigger flying animation
        flySoundInstance = AudioManager.instance.PlayOneShot(fly, this.transform.position);
    }
    /// <summary>
    /// Stops the flying animation and stops the fly sound immediately.
    /// </summary>
    private void StopFlyingAnimation()
    {
        animator.ResetTrigger("IsFlying"); // Reset flying animation trigger
        flySoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
}