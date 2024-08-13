using System.Collections;
using UnityEngine;

public class RedHoodBoss : MonoBehaviour
{
    public float waitTime = 15f; // Time before the boss starts the disappear sequence
    public Animator animator; // Animator for playing animations
    public SpriteRenderer spriteRenderer; // To control visibility
    public float teleportRadius = 10f; // Radius within which the boss can teleport further from the player
    public LayerMask dungeonLayerMask; // Layer mask to ensure the boss spawns in the dungeon bounds
    public string playerTag = "Player"; // Tag assigned to the player GameObject
    public float runAwayDistance = 3f; // Distance to run away from the player after attacking
    public float invisibilityDuration = 5f; // Duration the boss remains invisible before reappearing
    public float reappearanceDelay = 3f; // Delay before the next sequence starts

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
                // Play the disappear animation and wait for it to complete
                Debug.Log("Boss is disappearing");
                if (animator)
                {
                    animator.SetTrigger("Disappear");
                    // Wait until the disappear animation is done
                    yield return WaitForAnimation(animator, "Disappear");
                }

                // Become invisible
                spriteRenderer.enabled = false;

                // Wait for an additional delay before teleporting
                yield return new WaitForSeconds(0.4f); // Delay before teleporting

                // Teleport further from the player
                Debug.Log("Boss is teleporting");
                TeleportFurtherFromPlayer();

                // Move towards the player while invisible
                Debug.Log("Boss is moving towards the player");
                yield return StartCoroutine(MoveTowardsPlayerWhileInvisible());

                // Become visible when close to the player
                Debug.Log("Boss is becoming visible");
                spriteRenderer.enabled = true;
                if (animator)
                {
                    animator.SetTrigger("Visible");
                }

                // Attack the player
                if (animator)
                {
                    animator.SetTrigger("Attack");
                    // Wait until the attack animation is done
                    yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                }

                // Transition to idle state while moving away
                if (animator)
                {
                    animator.SetTrigger("Idle");
                }

                Debug.Log("Boss is running away");
                // Move away from the player while visible and in idle state
                yield return StartCoroutine(MoveAwayFromPlayer());

                // Become invisible again
                Debug.Log("Boss is becoming invisible again");
                spriteRenderer.enabled = false;
                if (animator)
                {
                    animator.SetTrigger("Disappear");
                    // Wait until the disappear animation is done
                    yield return WaitForAnimation(animator, "Disappear");
                }

                // Wait for the specified invisibility duration before reappearing
                yield return new WaitForSeconds(invisibilityDuration);

                // Reappear and teleport near the player
                Debug.Log("Boss is reappearing near the player");
                TeleportNearPlayer();  // Reposition near the player
                spriteRenderer.enabled = true;  // Make the boss visible again
                if (animator)
                {
                    animator.SetTrigger("Visible");
                }

                // Wait for a delay before the next sequence starts
                yield return new WaitForSeconds(reappearanceDelay);
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

        int maxAttempts = 10; // Limit attempts to find a valid position to prevent infinite loops
        int attempt = 0;

        while (!positionFound && attempt < maxAttempts)
        {
            attempt++;

            // Get a random direction and a distance much further than the current radius
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(teleportRadius / 2, teleportRadius); // Adjusted to ensure further distance

            // Calculate the new position
            teleportPosition = (Vector2)player.position + randomDirection * randomDistance;

            // Debug output for teleport position
            Debug.Log($"Attempt {attempt}: Trying position {teleportPosition}");

            // Check if the position is within the dungeon bounds
            if (Physics2D.OverlapCircle(teleportPosition, 0.5f, dungeonLayerMask))
            {
                positionFound = true;
            }
        }

        // If no valid position is found, default to a position further away from the player
        if (!positionFound)
        {
            teleportPosition = (Vector2)player.position + Random.insideUnitCircle.normalized * teleportRadius;
        }

        // Set the boss's position to the calculated teleport position
        transform.position = teleportPosition;

        // Debug output for final teleport position
        Debug.Log($"Final teleport position: {teleportPosition}");

        // Flip the boss to face the player
        FlipBoss();
    }

    void TeleportNearPlayer()
    {
        // Calculate a position near the player
        Vector2 nearPosition = (Vector2)player.position + Random.insideUnitCircle.normalized * (teleportRadius / 2);
        // Set the boss's position to the calculated near position
        transform.position = nearPosition;

        // Debug output for final near position
        Debug.Log($"Teleport near position: {nearPosition}");

        // Flip the boss to face the player
        FlipBoss();
    }

    IEnumerator MoveTowardsPlayerWhileInvisible()
    {
        float duration = 2f; // Time to move towards the player while invisible
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = (Vector2)player.position - (Vector2)(player.position - transform.position).normalized;

        // Move towards the player while invisible
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        transform.position = targetPosition;
    }

    IEnumerator MoveAwayFromPlayer()
    {
        float startTime = Time.time;
        float journeyLength = runAwayDistance;
        float journeyDuration = 2f; // Duration to move away from the player (adjust as needed)
        float distanceCovered = 0f;

        Vector2 startPosition = transform.position;
        Vector2 targetPosition = (Vector2)transform.position + (Vector2)((transform.position - player.position).normalized * runAwayDistance);

        // Ensure the target position is within dungeon bounds
        if (!Physics2D.OverlapCircle(targetPosition, 0.5f, dungeonLayerMask))
        {
            // Adjust target position if not in bounds
            targetPosition = (Vector2)transform.position + (Vector2)((transform.position - player.position).normalized * runAwayDistance);
        }

        while (distanceCovered < journeyLength)
        {
            float distance = (Time.time - startTime) * (journeyLength / journeyDuration);
            distanceCovered = distance;
            transform.position = Vector2.Lerp(startPosition, targetPosition, distanceCovered / journeyLength);

            yield return null;
        }

        transform.position = targetPosition;
    }

    void FlipBoss()
    {
        // Check if the player is to the right or left of the boss
        if (player.position.x < transform.position.x)
        {
            // Player is to the left, flip boss to face left
            spriteRenderer.flipX = true;
        }
        else
        {
            // Player is to the right, flip boss to face right
            spriteRenderer.flipX = false;
        }
    }

    // Helper function to wait for a specific animation to complete
    IEnumerator WaitForAnimation(Animator animator, string triggerName)
    {
        // Play the animation
        animator.SetTrigger(triggerName);

        // Wait until the animation has finished
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName(triggerName) && stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
    }
}
