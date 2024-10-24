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
    private Animator animator; // Reference to the Animator component

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player object has the 'Player' tag.");
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Get the Animator component
        // Start the flying routine
        StartCoroutine(FlyingRoutine());
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
            if (player == null) return;
        }

        // Check if the player is within the square detection area
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
        if (IsPlayerInDetectionRadius() && !isFiring && !isFlying && !isAttacking)
        {
            MoveTowardsPlayer();
        }

        // Check if the player is trying to leave the room bounds
        CheckPlayerPosition();
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Check the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Define a threshold for how close is "relatively close"
        float closeDistanceThreshold = 5f; // You can adjust this value as needed

        if (distanceToPlayer < closeDistanceThreshold)
        {
            // Calculate a new target position to move to the left or right of the player
            float offset = 2f; // This defines how far to the left/right the boss will move
            Vector2 targetPosition;

            // Determine if the boss should be on the left or right of the player
            if (transform.position.x < player.position.x)
            {
                targetPosition = new Vector2(player.position.x - offset, player.position.y); // Move to the left of the player
            }
            else
            {
                targetPosition = new Vector2(player.position.x + offset, player.position.y); // Move to the right of the player
            }

            // Attack if within attack distance
            if (Vector2.Distance(transform.position, player.position) <= attackTriggerDistance)
            {
                // Trigger the attack
                TriggerAttack();
            }
            else
            {
                // Maintain the buffer distance
                if (Vector2.Distance(targetPosition, player.position) > bufferDistance)
                {
                    // Move towards the calculated target position
                    direction = (targetPosition - (Vector2)transform.position).normalized;

                    // Only flip the sprite if the boss is moving
                    if (Mathf.Abs(direction.x) > 0.1f) // Check if the boss is moving horizontally
                    {
                        spriteRenderer.flipX = direction.x < 0; // Flip based on the movement direction
                    }
                }
                else
                {
                    // Stop moving if within buffer distance
                    direction = Vector2.zero;
                }
            }
        }
        else
        {
            // If the boss is further away, move towards the player normally
            direction = (player.position - transform.position).normalized;

            // Set the sprite direction based on movement
            if (Mathf.Abs(direction.x) > 0.1f)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }

        // Move towards the target direction if not stopped
        if (direction != Vector2.zero)
        {
            transform.position += (Vector3)(direction * flyingSpeed * Time.deltaTime);

            // Trigger the walking animation only if not already moving
            if (!isMoving)
            {
                isMoving = true; // Set moving state
                TriggerWalkingAnimation();
            }
        }
        else
        {
            // If not moving, stop walking animation
            StopWalkingAnimation();
        }
    }

    private void TriggerAttack()
    {
        if (!isAttacking) // Only trigger if not currently attacking
        {
            isAttacking = true; // Set attacking state
            animator.SetTrigger("Attack"); // Trigger the attack animation
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

        // Reset the attacking state after retreating
        isAttacking = false; // Allow attacking again after moving away
    }

    private IEnumerator MoveAwayFromPlayer(float duration)
    {
        float elapsed = 0f;

        // Calculate the retreat direction, which is away from the player
        Vector2 retreatDirection = (transform.position - player.position).normalized;

        // Ensure the sprite faces away from the player by flipping it correctly
        spriteRenderer.flipX = retreatDirection.x < 0; // Flip if retreating to the left

        while (elapsed < duration)
        {
            // Move away from the player
            transform.position += (Vector3)(retreatDirection * flyingSpeed * Time.deltaTime);
            elapsed += Time.deltaTime; // Increment elapsed time
            yield return null; // Wait for the next frame
        }

        // Stop walking animation after retreat
        StopWalkingAnimation();
    }



    private IEnumerator FlyingRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minAttackDelay, maxAttackDelay)); // Random wait before flying

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

            // Continue to stay in the middle while still flying
            yield return new WaitForSeconds(stayInMiddleDelay); // Stay in the middle for additional time

            isFiring = false; // Reset firing state
            isFlying = false; // Exit flying state
            StopFlyingAnimation(); // Stop the flying animation

            // Stop moving animation
            isMoving = false;
            StopWalkingAnimation();
        }
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

    private bool IsPlayerInDetectionRadius()
    {
        return Vector2.Distance(transform.position, player.position) <= detectionRadius;
    }

    private bool IsPlayerInRoom()
    {
        return IsPlayerInDetectionRadius(); // Assuming the room is defined by the detection radius
    }

    private void CheckPlayerPosition()
    {
        if (player.position.y < -6) // Assuming -6 is the y-bound for the room
        {
            Debug.Log("Player is out of bounds! Teleporting back...");
            TeleportPlayerToCenter();
        }
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
