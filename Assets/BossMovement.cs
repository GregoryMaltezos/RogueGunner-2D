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
    private int currentHealth;                // Current health of the boss

    private Rigidbody2D rb;
    private Vector2 direction;
    private Vector2 startPosition;
    private Transform player;
    private bool canAttack = true;
    private bool isFacingRight = true;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        currentHealth = maxHealth; // Initialize health
        StartCoroutine(ChangeDirection());
        FindPlayer();
    }

    void FixedUpdate()
    {
        if (player != null)
        {
            if (!isAttacking || currentHealth < maxHealth / 2) // Always move if HP is low
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
            if (!isAttacking || currentHealth < maxHealth / 2) // Change direction while moving
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
        float angleStep = 360f / projectileCount;
        float angle = 0f;

        for (int i = 0; i < projectileCount; i++)
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

    // Method to take damage and update health
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die(); // Handle death if needed
        }
    }

    void Die()
    {
        // Handle the boss's death (e.g., play death animation, drop loot, etc.)
        Destroy(gameObject); // Example: destroy the boss
    }
}
