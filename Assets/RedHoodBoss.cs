using System.Collections;
using UnityEngine;
using UnityEngine.UI; // For UI elements like health bars

public class RedHoodBoss : MonoBehaviour
{
    public float waitTime = 15f; // Time before the boss starts the disappear sequence
    public Animator animator; // Animator for playing animations
    public SpriteRenderer spriteRenderer; // To control visibility
    public Collider2D bossCollider; // Collider to disable when invisible
    public SpriteRenderer shadowRenderer; // To control shadow visibility
    public Image hpBarBackground; // Image component for the health bar background
    public Image hpBarFill; // Image component for the health bar fill
    public float teleportRadius = 10f; // Radius within which the boss can teleport further from the player
    public LayerMask dungeonLayerMask; // Layer mask to ensure the boss spawns in the dungeon bounds
    public string playerTag = "Player"; // Tag assigned to the player GameObject
    public float runAwayDistance = 3f; // Distance to run away from the player after attacking
    public float invisibilityDuration = 5f; // Duration the boss remains invisible before reappearing
    public float reappearanceDelay = 3f; // Delay before the next sequence starts
    public float cycleTime = 10f; // Time between cycles, adjustable in Inspector
    public float chaseDistanceThreshold = 0.5f; // Distance within which the boss will stop chasing and attack
    public float chaseSpeed = 2f; // Speed at which the boss chases the player
    public int attackDamage = 10; // Amount of damage dealt to the player

    private Transform player; // Reference to the player's transform
    private Rigidbody2D rb; // Reference to the Rigidbody2D for physics interactions
    private Color originalBackgroundColor;
    private Color originalFillColor;
    private Color shadowOriginalColor;

    private bool isDead = false; // Track whether the boss is dead

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // Ensure the boss is not affected by physics forces
        }

        if (hpBarBackground != null && hpBarFill != null)
        {
            originalBackgroundColor = hpBarBackground.color;
            originalFillColor = hpBarFill.color;
        }
        else
        {
            Debug.LogWarning("Health bar components are not assigned. Please assign them in the Inspector.");
        }

        if (shadowRenderer != null)
        {
            shadowOriginalColor = shadowRenderer.color;
        }
        else
        {
            Debug.LogWarning("Shadow renderer is not assigned. Please assign it in the Inspector.");
        }

        StartCoroutine(BossRoutine());
    }

    IEnumerator BossRoutine()
    {
        yield return new WaitForSeconds(waitTime);

        FindPlayer();

        if (player != null)
        {
            while (!isDead) // Check if the boss is dead before continuing
            {
                Debug.Log("Boss is disappearing");
                if (animator)
                {
                    animator.SetTrigger("Disappear");
                    yield return new WaitForSeconds(0.8f);
                }

                SetBossVisibility(false);
                SetHealthBarTransparency(0); // Make the health bar transparent
                SetShadowOpacity(0f); // Ensure shadow stays invisible

                yield return new WaitForSeconds(0.4f);

                Debug.Log("Boss is teleporting");
                TeleportFurtherFromPlayer();

                // Start shadow with low opacity while moving towards the player
                if (shadowRenderer != null)
                {
                    SetShadowOpacity(0.2f); // Low opacity only if the boss is visible again later
                }

                Debug.Log("Boss is actively chasing the player");
                yield return StartCoroutine(ChasePlayerUntilClose());

                // Increase shadow opacity as it gets closer only when boss is about to reappear
                if (shadowRenderer != null)
                {
                    float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                    float normalizedDistance = Mathf.InverseLerp(chaseDistanceThreshold, 0, distanceToPlayer);
                    SetShadowOpacity(Mathf.Lerp(0.2f, 1f, normalizedDistance));
                }

                // Only after reaching the chase distance threshold, make the boss visible
                Debug.Log("Boss is becoming visible for attack");
                SetBossVisibility(true);
                SetHealthBarTransparency(1); // Make the health bar visible

                if (animator)
                {
                    animator.SetTrigger("Attack");
                    yield return StartCoroutine(WaitForAnimation(animator, "Attack"));
                }

                // Deal damage to the player
                DealDamageToPlayer();

                // After the attack, health bar should stay visible until the next invisibility
                yield return new WaitForSeconds(0.4f);

                if (animator)
                {
                    animator.SetTrigger("Idle");
                }

                Debug.Log("Boss is running away");
                yield return StartCoroutine(MoveAwayFromPlayer());

                Debug.Log("Boss is disappearing again");
                if (animator)
                {
                    animator.SetTrigger("Disappear");
                    yield return new WaitForSeconds(0.8f);
                }

                SetBossVisibility(false);
                SetHealthBarTransparency(0); // Make the health bar transparent

                // Ensure shadow stays invisible when the boss disappears
                SetShadowOpacity(0f);

                yield return new WaitForSeconds(invisibilityDuration);

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

        Vector2 playerFacingDirection = player.right;

        while (!positionFound && attempt < maxAttempts)
        {
            attempt++;
            float randomDistance = Random.Range(teleportRadius / 2, teleportRadius);
            Vector2 behindPlayerDirection = -playerFacingDirection;
            teleportPosition = (Vector2)player.position + behindPlayerDirection * randomDistance;

            if (Vector2.Dot((teleportPosition - (Vector2)player.position).normalized, playerFacingDirection) > 0)
            {
                teleportPosition = (Vector2)player.position - playerFacingDirection * randomDistance;
            }

            if (Physics2D.OverlapCircle(teleportPosition, 0.5f, dungeonLayerMask))
            {
                positionFound = true;
            }
        }

        if (!positionFound)
        {
            teleportPosition = (Vector2)player.position - playerFacingDirection * teleportRadius;
        }

        transform.position = teleportPosition;

        // Ensure shadow stays invisible after teleporting
        if (shadowRenderer != null)
        {
            SetShadowOpacity(0f);
        }

        Debug.Log($"Final teleport position: {teleportPosition}");
    }

    IEnumerator ChasePlayerUntilClose()
    {
        float distanceToPlayer;

        do
        {
            distanceToPlayer = Vector2.Distance(transform.position, player.position);
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            Vector2 targetPosition = (Vector2)transform.position + directionToPlayer * chaseSpeed * Time.deltaTime;

            if (Vector2.Dot(directionToPlayer, player.right) > 0 && targetPosition.x > player.position.x)
            {
                targetPosition.x = player.position.x;
            }
            else if (Vector2.Dot(directionToPlayer, player.right) < 0 && targetPosition.x < player.position.x)
            {
                targetPosition.x = player.position.x;
            }

            transform.position = targetPosition;
            FlipBoss(true);

            yield return null;

        } while (distanceToPlayer > chaseDistanceThreshold && !isDead); // Check if the boss is dead

        transform.position = new Vector2(Mathf.Clamp(transform.position.x, player.position.x - chaseDistanceThreshold, player.position.x + chaseDistanceThreshold), player.position.y);
    }

    IEnumerator MoveAwayFromPlayer()
    {
        // Immediately exit if the boss is dead
        if (isDead)
        {
            yield break; // Exit the coroutine
        }

        // If the boss is alive, proceed with the run away logic
        float journeyDuration = 2f;
        float elapsed = 0f;

        Vector2 startPosition = transform.position;

        // Check if runAwayDistance is greater than 0 before trying to move
        Vector2 targetPosition = (Vector2)transform.position + (Vector2)((transform.position - player.position).normalized * runAwayDistance);

        while (elapsed < journeyDuration)
        {
            // Check if the boss is dead at each iteration
            if (isDead)
            {
                // Stop any ongoing actions and immediately exit
                yield break;
            }

            elapsed += Time.deltaTime;

            // Adjust shadow opacity during the movement
            float normalizedTime = elapsed / journeyDuration;
            if (shadowRenderer != null)
            {
                SetShadowOpacity(Mathf.Lerp(1f, 0f, normalizedTime));
            }

            // Flip the boss to face away from the player while running away
            FlipBoss(false);

            // Move the boss towards the target position
            transform.position = Vector2.Lerp(startPosition, targetPosition, normalizedTime);
            yield return null;
        }

        // Ensure the position is set to the final target position even if the loop ends
        transform.position = targetPosition;

        // Ensure shadow is fully invisible when the movement ends and remains invisible
        if (shadowRenderer != null)
        {
            SetShadowOpacity(0f);
        }
    }




    void FlipBoss(bool facePlayer)
    {
        if (shadowRenderer != null)
        {
            Vector3 shadowOffset = new Vector3(0, -0.5f, 0);
            shadowRenderer.transform.position = transform.position + shadowOffset;
        }

        // Flip the boss sprite based on the desired direction
        if (facePlayer)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
        else
        {
            spriteRenderer.flipX = player.position.x > transform.position.x;
        }
    }

    IEnumerator WaitForAnimation(Animator animator, string stateName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName(stateName) && stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }
    }

    void DealDamageToPlayer()
    {
        if (player != null)
        {
            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Player took {attackDamage} damage.");
            }
        }
    }

    void SetBossVisibility(bool visible)
    {
        spriteRenderer.enabled = visible;
        bossCollider.enabled = visible;
    }

    void SetHealthBarTransparency(float alpha)
    {
        if (hpBarBackground != null)
        {
            hpBarBackground.color = new Color(originalBackgroundColor.r, originalBackgroundColor.g, originalBackgroundColor.b, alpha);
        }
        if (hpBarFill != null)
        {
            hpBarFill.color = new Color(originalFillColor.r, originalFillColor.g, originalFillColor.b, alpha);
        }
    }

    void SetShadowOpacity(float opacity)
    {
        if (shadowRenderer != null)
        {
            Color shadowColor = shadowOriginalColor;
            shadowColor.a = opacity;
            shadowRenderer.color = shadowColor;
        }
    }

    // Call this method to handle the boss's death
    public void HandleDeath()
    {
        if (!isDead) // Ensure this logic only runs once
        {
            isDead = true; // Mark the boss as dead

            // Interrupt all animations and set the death parameter
            if (animator != null)
            {
                animator.SetBool("IsDead", true); // Assuming you have set up a boolean parameter
                animator.SetTrigger("Death"); // Trigger the death animation
            }

            // Set run away distance to 0
            runAwayDistance = 0f;

            StopAllCoroutines(); // Stop any ongoing actions
            SetBossVisibility(false); // Optionally hide boss after death

            // Ensure the health bar is also hidden after death
            SetHealthBarTransparency(0);

            // Optionally set position to prevent movement after death
            if (rb != null)
            {
                rb.velocity = Vector2.zero; // Ensure no movement occurs after death
                rb.isKinematic = true; // Make it kinematic to prevent further physics interaction
            }
        }
    }


}
