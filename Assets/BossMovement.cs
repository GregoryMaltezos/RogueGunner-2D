using System.Collections;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    public float speed = 2f;                  // Movement speed of the boss
    public float changeDirectionTime = 2f;    // Time in seconds before changing direction
    public float movementRadius = 5f;         // Radius of the movement area
    public float attackRange = 3f;            // Distance within which the boss attacks the player
    public float attackCooldown = 2f;         // Cooldown time between attacks
    public int attackDamage = 10;             // Damage dealt by the boss
    public GameObject projectilePrefab;       // The projectile prefab to be instantiated
    public int projectileCount = 8;           // Number of projectiles to fire in a circle
    public float projectileSpeed = 5f;        // Speed of the projectiles
    public Animator animator;                 // Animator component for the boss
    public int maxHealth = 100;               // Maximum health of the boss
    public Color flashColor = Color.red;      // Color to flash when health is low
    public float flashDuration = 1f;          // Duration of the flash effect
    public Color lowHealthFlashColor = Color.black; // Flash black color when at low health
    public float lowHealthFlashDuration = 0.5f; // Flash duration when at low health

    public int currentHealth;                // Current health of the boss
    private SpriteRenderer spriteRenderer;    // SpriteRenderer component for color changes

    private Rigidbody2D rb;
    private Vector2 direction;
    private Vector2 startPosition;
    private Transform player;
    private bool canAttack = true;
    private bool isFacingRight = true;
    private bool isAttacking = false;

    private bool hasEnteredHalfHealthPhase = false;  // Flag to check if half-health phase has started
    private bool hasEnteredLowHealthPhase = false;   // Flag to check if low-health phase has started

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component
        currentHealth = maxHealth; // Initialize health

        // Set Rigidbody2D to Kinematic to avoid knockback
        rb.bodyType = RigidbodyType2D.Kinematic;

        StartCoroutine(ChangeDirection());
        FindPlayer();
    }

    private void FixedUpdate()
    {
        if (player != null)
        {
            if (!isAttacking || currentHealth <= maxHealth / 2) // Always move if HP is low
            {
                Move();
                FacePlayer();
            }

            if (Vector2.Distance(player.position, rb.position) <= attackRange && canAttack)
            {
                StartCoroutine(Attack());
            }
        }

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

    private void Move()
    {
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            Vector2 nextPosition = rb.position + direction * speed * Time.fixedDeltaTime;

            // Check for collisions
            RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, speed * Time.fixedDeltaTime, LayerMask.GetMask("Wall"));
            if (hit.collider == null)
            {
                // Ensure that the boss stays within the movement radius
                if (Vector2.Distance(nextPosition, startPosition) <= movementRadius)
                {
                    rb.MovePosition(nextPosition);
                }
                else
                {
                    direction = -direction; // Change direction if out of bounds
                }
            }
            else
            {
                // Stop movement if collision is detected
                direction = Vector2.Reflect(direction, hit.normal);
            }
        }
        else
        {
            // For non-kinematic Rigidbody2D, just control velocity
            Vector2 movement = direction * speed * Time.fixedDeltaTime;
            rb.velocity = movement;
        }
    }

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

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private IEnumerator ChangeDirection()
    {
        while (true)
        {
            if (!isAttacking || currentHealth <= maxHealth / 2) // Change direction while moving
            {
                direction = Random.insideUnitCircle.normalized;
            }
            yield return new WaitForSeconds(changeDirectionTime);
        }
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        isAttacking = true;
        animator.SetTrigger("Attack"); // Trigger the attack animation

        // Wait for the attack animation to finish
        float animationDuration = 2.01f; // Replace with your animation length
        yield return new WaitForSeconds(animationDuration);

        FireProjectiles();

        // Wait for cooldown before allowing the next attack
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
        isAttacking = false;
        animator.SetTrigger("Idle"); // Trigger transition to idle/patrol animation
    }

    private void FireProjectiles()
    {
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

        float angleStep = 360f / effectiveProjectileCount;
        float angle = 0f;

        for (int i = 0; i < effectiveProjectileCount; i++)
        {
            float projectileDirXPosition = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180);
            float projectileDirYPosition = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180);

            Vector2 projectileVector = new Vector2(projectileDirXPosition, projectileDirYPosition);
            Vector2 projectileMoveDirection = (projectileVector - (Vector2)transform.position).normalized * effectiveProjectileSpeed;

            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            // Set the velocity of the projectile
            proj.GetComponent<Rigidbody2D>().velocity = new Vector2(projectileMoveDirection.x, projectileMoveDirection.y);

            // Rotate the projectile to match the direction
            float angleToPlayer = Mathf.Atan2(projectileMoveDirection.y, projectileMoveDirection.x) * Mathf.Rad2Deg;
            proj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angleToPlayer));

            angle += angleStep;
        }
    }

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

    private void EnterHalfHealthPhase()
    {
        hasEnteredHalfHealthPhase = true;
        projectileCount = 16; // Increase projectile count to 16
        Debug.Log("Half Health Phase Active: Projectile Count increased.");
    }

    private void EnterLowHealthPhase()
    {
        hasEnteredLowHealthPhase = true;
        attackCooldown = 1f; // Reduce cooldown to 1 second
        attackRange = 8f; // Increase attack range to 8
        projectileCount = 22; // Increase projectile count to 22
        projectileSpeed = 7f; // Increase projectile speed to 7
        movementRadius = 7f; // Increase movement radius to 7
        changeDirectionTime = 0.6f; // Decrease direction change time to 0.6 seconds
        StartCoroutine(FlashBlack()); // Flash black
        Debug.Log("Low Health Phase Active: Adjusted parameters and flashing black.");
    }

    // Coroutine to flash black when entering the low health phase
    private IEnumerator FlashBlack()
    {
        Color originalColor = spriteRenderer.color; // Store the original color
        spriteRenderer.color = lowHealthFlashColor; // Set the black flash color
        yield return new WaitForSeconds(lowHealthFlashDuration); // Wait for the duration of the flash
        spriteRenderer.color = originalColor; // Revert to the original color
    }

    // Called by BossHp script to update current health
    private void SetCurrentHealth(int health)
    {
        currentHealth = health;
        Debug.Log($"Boss current health set to: {currentHealth}");
    }

    // Called by BossHp script to flash red
    private void FlashRed()
    {
        StartCoroutine(FlashRedCoroutine());
    }

    private IEnumerator FlashRedCoroutine()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}
