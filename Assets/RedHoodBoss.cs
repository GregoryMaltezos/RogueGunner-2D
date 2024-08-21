using System.Collections;
using UnityEngine;

public class RedHoodBoss : MonoBehaviour
{
    public float waitTime = 15f; // Time before the boss starts the disappear sequence
    public Animator animator; // Animator for playing animations
    public SpriteRenderer spriteRenderer; // To control visibility
    public Collider2D bossCollider; // Collider to disable when invisible
    public SpriteRenderer shadowRenderer; // To control shadow visibility
    public float teleportRadius = 10f; // Radius within which the boss can teleport further from the player
    public LayerMask dungeonLayerMask; // Layer mask to ensure the boss spawns in the dungeon bounds
    public string playerTag = "Player"; // Tag assigned to the player GameObject
    public float runAwayDistance = 3f; // Distance to run away from the player after attacking
    public float invisibilityDuration = 5f; // Duration the boss remains invisible before reappearing
    public float reappearanceDelay = 3f; // Delay before the next sequence starts
    public float cycleTime = 10f; // Time between cycles, adjustable in Inspector
    public float chaseDistanceThreshold = 0.5f; // Distance within which the boss will stop chasing and attack
    public float chaseSpeed = 2f; // Speed at which the boss chases the player

    private Transform player; // Reference to the player's transform

    void Start()
    {
        StartCoroutine(BossRoutine());
    }

    IEnumerator BossRoutine()
    {
        // Wait for the specified time before starting the routine
        yield return new WaitForSeconds(waitTime);

        // Find the player dynamically
        FindPlayer();

        if (player != null)
        {
            while (true)
            {
                // Play the disappear animation
                Debug.Log("Boss is disappearing");
                if (animator)
                {
                    animator.SetTrigger("Disappear");
                    // Wait for 1 second before becoming invisible
                    yield return new WaitForSeconds(0.8f);
                }

                // Become invisible (disable sprite only)
                spriteRenderer.enabled = false;

                // Disable the hitbox and shadow
                bossCollider.enabled = false;
                if (shadowRenderer != null)
                {
                    shadowRenderer.enabled = false;
                }

                // Wait for an additional delay before teleporting
                yield return new WaitForSeconds(0.4f);

                // Teleport further from the player
                Debug.Log("Boss is teleporting");
                Vector2 oldPosition = transform.position; // Record the old position
                TeleportFurtherFromPlayer();

                // Move the shadow to the new position immediately
                if (shadowRenderer != null)
                {
                    shadowRenderer.transform.position = transform.position + Vector3.down * 0.5f; // Adjust this offset as needed
                }

                // Enable the hitbox and shadow after teleportation
                bossCollider.enabled = true;
                if (shadowRenderer != null)
                {
                    shadowRenderer.enabled = true;
                }

                // Move towards the player while invisible, actively chasing
                Debug.Log("Boss is actively chasing the player");
                yield return StartCoroutine(ChasePlayerUntilClose());

                // Become fully visible when close to the player
                Debug.Log("Boss is becoming visible");
                spriteRenderer.enabled = true;

                if (animator)
                {
                    animator.SetTrigger("Visible");
                    yield return StartCoroutine(WaitForAnimation(animator, "Visible"));
                }

                // Attack the player
                Debug.Log("Boss is attacking");
                if (animator)
                {
                    animator.SetTrigger("Attack");
                    yield return StartCoroutine(WaitForAnimation(animator, "Attack"));
                }

                // Wait for 0.4 seconds after the attack
                yield return new WaitForSeconds(0.4f);

                // After attack is complete, transition to idle state and then run away
                if (animator)
                {
                    animator.SetTrigger("Idle");
                }

                // Start moving away from the player and flip the boss to face away from the player
                Debug.Log("Boss is running away");
                yield return new WaitForSeconds(0.2f); // Additional wait before flipping
                FlipBoss(false); // Flip to face away from the player
                yield return StartCoroutine(MoveAwayFromPlayer());

                // Play the disappear animation again after running away
                Debug.Log("Boss is disappearing again");
                if (animator)
                {
                    animator.SetTrigger("Disappear");
                    // Wait for the animation to finish before becoming invisible
                    yield return new WaitForSeconds(0.8f);
                }

                // Make the boss invisible
                spriteRenderer.enabled = false;
                bossCollider.enabled = false;

                // Hide the shadow immediately
                if (shadowRenderer != null)
                {
                    shadowRenderer.enabled = false;
                }

                // Wait for the specified invisibility duration
                yield return new WaitForSeconds(invisibilityDuration);

                // Wait for the cycle time before repeating the sequence
                yield return new WaitForSeconds(cycleTime);
            }
        }
        else
        {
            Debug.LogWarning("Player not found!");
        }
    }

    void FindPlayer()
    {
        // Find the player by tag
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void TeleportFurtherFromPlayer()
    {
        Vector2 teleportPosition = Vector2.zero;
        bool positionFound = false;

        int maxAttempts = 10;
        int attempt = 0;

        // Determine the direction the player is facing
        Vector2 playerFacingDirection = player.right;

        while (!positionFound && attempt < maxAttempts)
        {
            attempt++;

            // Get a random distance from the player
            float randomDistance = Random.Range(teleportRadius / 2, teleportRadius);

            // Calculate a direction that is behind the player based on their facing direction
            Vector2 behindPlayerDirection = -playerFacingDirection;
            teleportPosition = (Vector2)player.position + behindPlayerDirection * randomDistance;

            // Ensure the calculated position is actually behind the player
            if (Vector2.Dot((teleportPosition - (Vector2)player.position).normalized, playerFacingDirection) > 0)
            {
                // If the teleport position is not behind the player, adjust it
                teleportPosition = (Vector2)player.position - playerFacingDirection * randomDistance;
            }

            // Debug output for teleport position
            Debug.Log($"Attempt {attempt}: Trying position {teleportPosition}");

            // Check if the position is within the dungeon bounds
            if (Physics2D.OverlapCircle(teleportPosition, 0.5f, dungeonLayerMask))
            {
                positionFound = true;
            }
        }

        // If no valid position is found, default to a position behind the player
        if (!positionFound)
        {
            teleportPosition = (Vector2)player.position + -playerFacingDirection * teleportRadius;
        }

        // Set the boss's position to the calculated teleport position
        transform.position = teleportPosition;

        // Debug output for final teleport position
        Debug.Log($"Final teleport position: {teleportPosition}");
    }

    IEnumerator ChasePlayerUntilClose()
    {
        float distanceToPlayer;

        // Continue chasing until within a threshold distance
        do
        {
            // Calculate distance to the player
            distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Calculate the direction to move towards the player
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            // Calculate the target position
            Vector2 targetPosition = (Vector2)transform.position + directionToPlayer * chaseSpeed * Time.deltaTime;

            // Check if the boss is trying to move in front of the player
            if (Vector2.Dot(directionToPlayer, player.right) > 0 && targetPosition.x > player.position.x)
            {
                // Adjust target position to stop before moving in front of the player
                targetPosition.x = player.position.x;
            }
            else if (Vector2.Dot(directionToPlayer, player.right) < 0 && targetPosition.x < player.position.x)
            {
                // Adjust target position to stop before moving in front of the player from the other side
                targetPosition.x = player.position.x;
            }

            // Move towards the adjusted target position
            transform.position = targetPosition;

            // Flip the boss to face the player
            FlipBoss(true);

            yield return null;

        } while (distanceToPlayer > chaseDistanceThreshold); // Continue chasing until close enough

        // Ensure the boss is exactly at the target position, but not in front of the player
        transform.position = new Vector2(Mathf.Clamp(transform.position.x, player.position.x - chaseDistanceThreshold, player.position.x + chaseDistanceThreshold), player.position.y);
    }


    IEnumerator MoveAwayFromPlayer()
    {
        float journeyDuration = 2f; // Duration to move away from the player
        float elapsed = 0f;

        Vector2 startPosition = transform.position;
        Vector2 targetPosition = (Vector2)transform.position + (Vector2)((transform.position - player.position).normalized * runAwayDistance);

        while (elapsed < journeyDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsed / journeyDuration);
            yield return null;
        }

        transform.position = targetPosition;
    }

    void FlipBoss(bool facePlayer)
    {
        // Adjust shadow position to match the boss's feet
        if (shadowRenderer != null)
        {
            Vector3 shadowOffset = new Vector3(0, -0.5f, 0); // Adjust this offset as needed
            shadowRenderer.transform.position = transform.position + shadowOffset;
        }

        // Check if the player is to the right or left of the boss
        if (facePlayer)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
        else
        {
            spriteRenderer.flipX = player.position.x >= transform.position.x;
        }
    }

    IEnumerator WaitForAnimation(Animator animator, string stateName)
    {
        // Wait until the specified animation has finished
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName(stateName) && stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
    }
}