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

    public float minAttackDelay = 5f; // Minimum delay before the boss attacks
    public float maxAttackDelay = 10f; // Maximum delay before the boss attacks
    public float stayInMiddleDelay = 2f; // Additional time to stay in the middle after firing

    private Transform player; // Reference to the player
    private bool hasEnteredRoom = false; // Track if the player has entered the room

    // Detection radius
    public float detectionRadius = 10f; // Side length of the square detection area
    public bool showDetectionRadius = true; // Toggle to visualize detection area
    private SpriteRenderer spriteRenderer;

    // New variables for vertical oscillation
    public float oscillationAmplitude = 1f; // Amplitude of the vertical movement
    public float oscillationSpeed = 2f; // Speed of the vertical oscillation

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player object has the 'Player' tag.");
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        // Only move towards the player if they are within the detection radius and not firing or flying
        if (IsPlayerInDetectionRadius() && !isFiring && !isFlying)
        {
            MoveTowardsPlayer();
        }

        // Check if the player is trying to leave the room bounds
        CheckPlayerPosition();
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Flip the sprite based on the player's position using the SpriteRenderer
        if (direction.x > 0) // Player is to the right
        {
            spriteRenderer.flipX = false; // Face right
        }
        else if (direction.x < 0) // Player is to the left
        {
            spriteRenderer.flipX = true; // Face left
        }

        // Move towards player
        transform.position += (Vector3)(direction * flyingSpeed * Time.deltaTime);

        // Trigger the walking animation only if not already moving
        if (!isMoving)
        {
            isMoving = true; // Set moving state
            TriggerWalkingAnimation();
        }
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

    private void CheckPlayerPosition()
    {
        // Check if the player is outside the bounds of the room
        if (!IsPlayerInRoom() && hasEnteredRoom)
        {
            Debug.Log("You may not leave!");
            TeleportPlayerToCenter();
        }
    }

    private void TeleportPlayerToCenter()
    {
        player.position = Vector3.zero; // Teleport player to (0, 0, 0)
    }

    private bool IsPlayerInRoom()
    {
        // Check if the player is within the square detection area
        return player.position.x >= -10f && player.position.x <= 10f &&
               player.position.y >= -10f && player.position.y <= 10f;
    }

    private bool IsPlayerInDetectionRadius()
    {
        // Check if the player is within the detection radius
        return player.position.x >= -detectionRadius / 2 && player.position.x <= detectionRadius / 2 &&
               player.position.y >= -detectionRadius / 2 && player.position.y <= detectionRadius / 2;
    }

    void TriggerFlyingAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsFlying", true); // Use a boolean to loop the flying animation
        }
        else
        {
            Debug.LogError("Boss prefab is missing Animator component.");
        }
    }

    void StopFlyingAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsFlying", false); // Reset the animation state
        }
    }

    void TriggerWalkingAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsWalking", true); // Use a boolean to start walking animation
        }
    }

    void StopWalkingAnimation()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsWalking", false); // Reset the walking animation state
        }
    }

    // Draw the room and detection area in the Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Color of the room
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(10f, 10f, 10f)); // Draw a wireframe cube for the 10x10 room

        if (showDetectionRadius)
        {
            Gizmos.color = Color.blue; // Color for the detection radius
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(detectionRadius, detectionRadius, 0)); // Draw a wireframe cube for detection radius
        }
    }
}
