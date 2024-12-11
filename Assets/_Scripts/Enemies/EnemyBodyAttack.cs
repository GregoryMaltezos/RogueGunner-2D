using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class EnemyBodyAttack : MonoBehaviour
{
    public SpriteRenderer characterRenderer;
    public Vector2 PointerPosition { get; set; }

    public Animator animator;
    public float attackDelay = 0.3f;
    private bool attackBlocked;

    public bool IsAttacking { get; private set; }

    public Transform attackOrigin;
    public float attackRadius;
    public LayerMask playerLayer;

    private DamageSource damageSource;

    [SerializeField]
    private float attackDistance = 1.5f; // Distance within which the enemy can attack
    [SerializeField]
    private float stopDistance = 0.5f; // Distance at which the enemy should stop moving toward the player
    [SerializeField] private EventReference mimicAttack;

    private PlayerHealth playerHealth; // Cached reference to the player's health

    /// <summary>
    /// Initializes references and ensures required components are available.
    /// </summary>
    private void Start()
    {
        damageSource = GetComponent<DamageSource>();
        if (damageSource == null)
        {
            Debug.LogError("DamageSource component not found on the enemy!");
        }

        // Cache the player's health component
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    /// <summary>
    /// Resets the IsAttacking flag to allow the enemy to attack again.
    /// </summary>
    public void ResetIsAttacking()
    {
        IsAttacking = false;
    }

    /// <summary>
    /// Handles enemy behavior, including detecting and attacking the player.
    /// </summary>
    private void Update()
    {
        // Check if the player exists and is alive
        if (playerHealth == null || playerHealth.isDead)
        {
            // Stop attacking and reset attacking state
            IsAttacking = false;
            return;
        }

        // Check if enemy is close to the player and attack if possible
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

            // Check if within attack range
            if (distanceToPlayer <= attackDistance && !IsAttacking)
            {
                Attack(); // Trigger the attack
                return; // Exit Update to prevent further processing
            }

            // Stop moving if within stopDistance
            if (distanceToPlayer <= stopDistance)
            {
                return; // Exit Update to prevent further movement
            }
        }

        if (IsAttacking)
            return;

        // Adjust to move with the enemy
        Vector2 parentPosition = (Vector2)transform.parent.position; // Get parent's position
        Vector2 direction = (PointerPosition - parentPosition).normalized; // Use parent's position

        transform.right = direction;

        Vector2 scale = transform.localScale;
        scale.y = direction.x < 0 ? -1 : 1;
        transform.localScale = scale;
    }

    /// <summary>
    /// Initiates the attack sequence and triggers related effects.
    /// </summary>
    public void Attack()
    {
        // Ensure the player is alive and attack isn't currently blocked
        if (playerHealth == null || playerHealth.isDead || attackBlocked)
            return;
        // Trigger attack animation and play sound effect
        animator.SetTrigger("Attack");
        AudioManager.instance.PlayOneShot(mimicAttack, this.transform.position);
        IsAttacking = true;
        attackBlocked = true;
        // Handle attack completion with a delay
        StartCoroutine(HandleAttack());
    }

    /// <summary>
    /// Handles the attack process, including delay and detecting player collision.
    /// </summary>
    private IEnumerator HandleAttack()
    {
        yield return new WaitForSeconds(attackDelay);
        DetectColliders(); // Detect and apply damage to the player
        attackBlocked = false;
        IsAttacking = false;
    }

    /// <summary>
    /// Visualizes the attack radius in the editor for debugging.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, attackRadius);
    }

    /// <summary>
    /// Detects player colliders within the attack radius and applies damage if applicable.
    /// </summary>
    private void DetectColliders()
    {
        if (attackOrigin == null || playerHealth == null || playerHealth.isDead) return;

        Debug.Log($"Detecting colliders at position: {attackOrigin.position} with radius: {attackRadius}");
        // Find all colliders within the attack radius that belong to the player layer
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackOrigin.position, attackRadius, playerLayer);
        Debug.Log("Number of colliders detected: " + hitColliders.Length);
        // Process each detected collider
        foreach (Collider2D collider in hitColliders)
        {
            Debug.Log("Hit collider: " + collider.name);
            PlayerHealth playerHealth = collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (damageSource != null)
                {
                    float damage = damageSource.GetDamage(); // Apply damage to the player
                    Debug.Log("PlayerHealth component found. Dealing damage: " + damage);
                    playerHealth.TakeDamage(damage);
                }
                else
                {
                    Debug.LogWarning("DamageSource component is missing.");
                }
            }
            else
            {
                Debug.Log("PlayerHealth component not found on: " + collider.name);
            }
        }
    }
}
