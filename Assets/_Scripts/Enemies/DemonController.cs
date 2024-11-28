using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class DemonController : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private Agent agent;  // Reference to the Agent component
    private Transform spriteTransform; // Reference to the sprite transform
    private Health health; // Reference to the Health component

    [SerializeField]
    private float speed = 2f;                // Base speed of the enemy
    [SerializeField]
    private float maxSpeed = 2f;             // Maximum speed
    [SerializeField]
    private float acceleration = 50f;        // Acceleration rate
    [SerializeField]
    private float stopDistance = 2f;         // Distance from the player to maintain
    [SerializeField]
    private float minMovementTime = 0.5f;    // Minimum movement time
    [SerializeField]
    private float maxMovementTime = 1.5f;    // Maximum movement time
    [SerializeField]
    private float attackCooldown = 2f;       // Time between attacks
    [SerializeField]
    private float attackDuration = 1f;       // How long the attack lasts

    [SerializeField]
    private float chaseRadius = 10f;         // Chase radius within which the demon will follow the player

    [SerializeField] private EventReference attack;

    private Transform player;                // Reference to the player's transform
    private float currentSpeed = 0f;         // Current speed of the enemy
    private Vector2 randomDirection;         // Direction to move in randomly
    private float movementTimer;             // Timer to track random movement change
    private bool canMove = true;             // Variable to control movement
    private bool isAttacking = false;        // Is the enemy currently attacking
    private bool attackOnCooldown = false;   // Is the attack on cooldown

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();    // Get the Rigidbody2D component
        agent = GetComponent<Agent>();         // Get the Agent component
        health = GetComponent<Health>();       // Get the Health component
        spriteTransform = transform.GetChild(0); // Assuming the sprite is the first child
    }

    void Start()
    {
        FindPlayer();                          // Attempt to find the player at startup
        SetRandomDirection();                  // Set the initial random direction
    
    }

    void FixedUpdate()
    {
        // Check if the demon is dead
        if (health != null && health.isDead)
        {
            rb2d.velocity = Vector2.zero; // Stop all movement
                      // Stop movement sound if dead
            return; // Exit the method if dead
        }

        if (player == null)
        {
            FindPlayer();                      // If the player is null, keep trying to find them
            return;
        }

        float distanceToPlayer = Vector2.Distance(rb2d.position, player.position);

        // If the player is within chase radius
        if (distanceToPlayer <= chaseRadius)
        {
            if (distanceToPlayer <= stopDistance && !isAttacking && !attackOnCooldown)
            {
                StartCoroutine(AttackPlayer());    // Start attack coroutine if within attack range
            }

            if (canMove && !isAttacking)           // Move only if not attacking
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
            else if (isAttacking)
            {
                // If attacking, stop movement
                rb2d.velocity = Vector2.zero;
            }
        }
        else
        {
            // Stop moving when outside of chase radius
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
        // Optional: Visualize the stopDistance for debugging
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, chaseRadius);
        }
    }

    // Coroutine to handle attack behavior
    IEnumerator AttackPlayer()
    {
        isAttacking = true;                      // Set the attacking state
        rb2d.velocity = Vector2.zero;            // Stop the enemy movement

        // Make the agent perform the attack
        AudioManager.instance.PlayOneShot(attack, this.transform.position);
        agent.PerformAttack();                   // Trigger the attack using Agent script

        // Wait for the attack duration
        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;                     // End the attack
        attackOnCooldown = true;                 // Set cooldown
        StartCoroutine(AttackCooldown());
    }

    // Coroutine for attack cooldown
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        attackOnCooldown = false;
    }
}
