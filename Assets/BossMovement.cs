using System.Collections;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    // Existing variables
    public float speed = 2f;
    public float changeDirectionTime = 2f;
    public float movementRadius = 5f;
    public float attackRange = 3f;
    public float attackCooldown = 2f;
    public int attackDamage = 10;
    public GameObject projectilePrefab;
    public int projectileCount = 8;
    public float projectileSpeed = 5f;
    public Animator animator;
    public int maxHealth = 100;
    public Color flashColor = Color.red;
    public float flashDuration = 1f;
    public Color lowHealthFlashColor = Color.black;
    public float lowHealthFlashDuration = 0.5f;

    public int currentHealth;
    private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Vector2 direction;
    private Vector2 startPosition;
    private Transform player;
    private bool canAttack = true;
    private bool isFacingRight = true;
    private bool isAttacking = false;

    private bool hasEnteredHalfHealthPhase = false;
    private bool hasEnteredLowHealthPhase = false;

    public LayerMask obstacleLayer; // New variable for obstacle layer mask

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        rb.bodyType = RigidbodyType2D.Kinematic;

        StartCoroutine(ChangeDirection());
        FindPlayer();
    }

    private void FixedUpdate()
    {
        if (player != null)
        {
            if (!isAttacking || currentHealth <= maxHealth / 2)
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

            // Check for collisions with obstacles
            RaycastHit2D hit = Physics2D.Raycast(rb.position, direction, speed * Time.fixedDeltaTime, obstacleLayer);
            if (hit.collider == null)
            {
                if (Vector2.Distance(nextPosition, startPosition) <= movementRadius)
                {
                    rb.MovePosition(nextPosition);
                }
                else
                {
                    direction = -direction;
                }
            }
            else
            {
                direction = Vector2.Reflect(direction, hit.normal);
            }
        }
        else
        {
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
            if (!isAttacking || currentHealth <= maxHealth / 2)
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
        animator.SetTrigger("Attack");

        float animationDuration = 2.01f;
        yield return new WaitForSeconds(animationDuration);

        FireProjectiles();

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
        isAttacking = false;
        animator.SetTrigger("Idle");
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

            proj.GetComponent<Rigidbody2D>().velocity = new Vector2(projectileMoveDirection.x, projectileMoveDirection.y);

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
        projectileCount = 16;
        Debug.Log("Half Health Phase Active: Projectile Count increased.");
    }

    private void EnterLowHealthPhase()
    {
        hasEnteredLowHealthPhase = true;
        attackCooldown = 1f;
        attackRange = 8f;
        projectileCount = 22;
        projectileSpeed = 7f;
        movementRadius = 7f;
        changeDirectionTime = 0.6f;
        StartCoroutine(FlashBlack());
        Debug.Log("Low Health Phase Active: Adjusted parameters and flashing black.");
    }

    private IEnumerator FlashBlack()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = lowHealthFlashColor;
        yield return new WaitForSeconds(lowHealthFlashDuration);
        spriteRenderer.color = originalColor;
    }

    private void SetCurrentHealth(int health)
    {
        currentHealth = health;
        Debug.Log($"Boss current health set to: {currentHealth}");
    }

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
