using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Add these two fields
    [SerializeField]
    private float attackDistance = 1.5f; // Distance within which the enemy can attack
    [SerializeField]
    private float stopDistance = 0.5f; // Distance at which the enemy should stop moving toward the player

    private void Start()
    {
        damageSource = GetComponent<DamageSource>();
        if (damageSource == null)
        {
            Debug.LogError("DamageSource component not found on the enemy!");
        }
    }

    public void ResetIsAttacking()
    {
        IsAttacking = false;
    }

    private void Update()
    {
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

    public void Attack()
    {
        if (attackBlocked)
            return;
        animator.SetTrigger("Attack");
        IsAttacking = true;
        attackBlocked = true;
        StartCoroutine(HandleAttack());
    }

    private IEnumerator HandleAttack()
    {
        yield return new WaitForSeconds(attackDelay);
        DetectColliders();
        attackBlocked = false;
        IsAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, attackRadius);
    }

    private void DetectColliders()
    {
        if (attackOrigin == null) return;

        Debug.Log($"Detecting colliders at position: {attackOrigin.position} with radius: {attackRadius}");

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackOrigin.position, attackRadius, playerLayer);
        Debug.Log("Number of colliders detected: " + hitColliders.Length);

        foreach (Collider2D collider in hitColliders)
        {
            Debug.Log("Hit collider: " + collider.name);
            PlayerHealth playerHealth = collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (damageSource != null)
                {
                    float damage = damageSource.GetDamage();
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
