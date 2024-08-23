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

    public int currentHealth;                // Current health of the boss
    private SpriteRenderer spriteRenderer;    // SpriteRenderer component for color changes

    private Rigidbody2D rb;
    private Vector2 direction;
    private Vector2 startPosition;
    private Transform player;
    private bool canAttack = true;
    private bool isFacingRight = true;
    private bool isAttacking = false;

    private BossHp bossHpScript; // Reference to BossHp script

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component
        bossHpScript = GetComponent<BossHp>(); // Get the BossHp component
        currentHealth = maxHealth; // Initialize health
        StartCoroutine(ChangeDirection());
        FindPlayer();
    }

    void FixedUpdate()
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
    }

    void Move()
    {
        Vector2 nextPosition = rb.position + direction * speed * Time.fixedDeltaTime;

        if (Vector2.Distance(nextPosition, startPosition) <= movementRadius)
        {
            rb.MovePosition(nextPosition);
        }
        else
        {
            direction = -direction;
        }
    }

    void FacePlayer()
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

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    IEnumerator ChangeDirection()
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

    IEnumerator Attack()
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

    void FireProjectiles()
    {
        int effectiveProjectileCount = currentHealth <= maxHealth / 2 ? projectileCount * 2 : projectileCount; // Double projectiles if health is low

        float angleStep = 360f / effectiveProjectileCount;
        float angle = 0f;

        for (int i = 0; i < effectiveProjectileCount; i++)
        {
            float projectileDirXPosition = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180);
            float projectileDirYPosition = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180);

            Vector2 projectileVector = new Vector2(projectileDirXPosition, projectileDirYPosition);
            Vector2 projectileMoveDirection = (projectileVector - (Vector2)transform.position).normalized * projectileSpeed;

            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            // Set the velocity of the projectile
            proj.GetComponent<Rigidbody2D>().velocity = new Vector2(projectileMoveDirection.x, projectileMoveDirection.y);

            // Rotate the projectile to match the direction
            float angleToPlayer = Mathf.Atan2(projectileMoveDirection.y, projectileMoveDirection.x) * Mathf.Rad2Deg;
            proj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angleToPlayer));

            angle += angleStep;
        }
    }

    void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    // Call this method to trigger the flashing effect
    public void FlashRed()
    {
        StartCoroutine(FlashRedCoroutine());
    }

    // Coroutine to handle the red flash effect
    IEnumerator FlashRedCoroutine()
    {
        Color originalColor = spriteRenderer.color; // Store the original color
        spriteRenderer.color = flashColor; // Set the flash color
        yield return new WaitForSeconds(flashDuration); // Wait for the duration of the flash
        spriteRenderer.color = originalColor; // Revert to the original color
    }
}
