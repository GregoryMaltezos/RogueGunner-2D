using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonController : MonoBehaviour
{
    private Rigidbody2D rb2d;

    [SerializeField]
    private float speed = 2f;                // Base speed of the enemy
    [SerializeField]
    private float maxSpeed = 2f;             // Maximum speed
    [SerializeField]
    private float acceleration = 50f;         // Acceleration rate
    [SerializeField]
    private float deacceleration = 100f;      // Deceleration rate
    [SerializeField]
    private float stopDistance = 2f;         // Distance from the player to maintain
    [SerializeField]
    private float avoidanceDistance = 1f;     // Distance to check for walls
    [SerializeField]
    private float minMovementTime = 0.5f;    // Minimum movement time
    [SerializeField]
    private float maxMovementTime = 1.5f;    // Maximum movement time

    private Transform player;                // Reference to the player's transform
    private float currentSpeed = 0f;         // Current speed of the enemy
    private Vector2 randomDirection;          // Direction to move in randomly
    private float movementTimer;              // Timer to track random movement change
    private bool canMove = true;              // Variable to control movement

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();    // Get the Rigidbody2D component
    }

    void Start()
    {
        FindPlayer();                          // Attempt to find the player at startup
        SetRandomDirection();                  // Set the initial random direction
        movementTimer = Random.Range(minMovementTime, maxMovementTime); // Set initial timer
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            FindPlayer();                      // If the player is null, keep trying to find them
            return;
        }

        float distanceToPlayer = Vector2.Distance(rb2d.position, player.position);

        if (canMove)
        {
            if (distanceToPlayer > stopDistance)
            {
                // Move semi-randomly towards the player without going directly to them
                SemiRandomMovement();
            }
            else
            {
                // Move randomly while avoiding obstacles
                RandomMovement();
            }

            // Apply velocity based on current speed
            rb2d.velocity = randomDirection * currentSpeed;
        }
        else
        {
            // If the agent can't move, stop all movement
            rb2d.velocity = Vector2.zero;
        }
    }

    void FindPlayer()
    {
        // Find the player GameObject by tag (ensure your player has the tag "Player")
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;  // Set the player transform if found
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure the player GameObject is tagged 'Player'.");
        }
    }

    void SemiRandomMovement()
    {
        Vector2 directionToPlayer = ((Vector2)player.position - rb2d.position).normalized; // Calculate direction to the player
        randomDirection = Vector2.Lerp(randomDirection, directionToPlayer, 0.3f); // Blend between random direction and direction to player

        // Check for wall collision ahead in the direction towards the player
        RaycastHit2D hit = Physics2D.Raycast(rb2d.position, randomDirection, avoidanceDistance, LayerMask.GetMask("Obstacle"));

        if (hit.collider != null) // If there's an obstacle in front
        {
            // Increase the weight of direction towards player
            randomDirection = directionToPlayer * 1.5f; // Adjust the weight of the player's direction
        }

        // Accelerate in the semi-random direction
        currentSpeed += acceleration * speed * Time.fixedDeltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
    }

    void RandomMovement()
    {
        movementTimer -= Time.fixedDeltaTime; // Decrease the timer

        if (movementTimer <= 0f) // Check if it's time to change direction
        {
            SetRandomDirection(); // Set a new random direction
            movementTimer = Random.Range(minMovementTime, maxMovementTime); // Reset the timer
        }

        // Check for wall collision in the random movement direction
        RaycastHit2D hit = Physics2D.Raycast(rb2d.position, randomDirection, avoidanceDistance, LayerMask.GetMask("Obstacle"));

        if (hit.collider != null) // If there's an obstacle in front
        {
            // Increase the weight of direction towards player
            Vector2 directionToPlayer = ((Vector2)player.position - rb2d.position).normalized; // Calculate direction to the player
            randomDirection = directionToPlayer * 1.5f; // Adjust the weight of the player's direction
        }

        // Accelerate in the random direction
        currentSpeed += acceleration * speed * Time.fixedDeltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
    }

    void SetRandomDirection()
    {
        // Generate a random direction
        float randomX = Random.Range(-1f, 1f);
        float randomY = Random.Range(-1f, 1f);
        randomDirection = new Vector2(randomX, randomY).normalized; // Normalize to maintain consistent speed
    }

    private void OnDrawGizmos()
    {
        // Optional: Visualize the stopDistance and avoidanceDistance for debugging
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, stopDistance);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rb2d.position, avoidanceDistance); // Visualize the avoidance distance
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Check if the enemy is about to push the player away
            rb2d.velocity = Vector2.zero; // Stops the enemy's movement when colliding with the player
        }
    }

    // Method to enable or disable movement
    public void SetMovement(bool enabled)
    {
        canMove = enabled;

        // If movement is disabled, immediately stop the Rigidbody2D's velocity
        if (!enabled)
        {
            rb2d.velocity = Vector2.zero;
        }
    }
}
