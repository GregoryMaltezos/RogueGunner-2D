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

    public void ResetIsAttacking()
    {
        IsAttacking = false;
    }

    private void Update()
    {
        if (IsAttacking)
            return;

        Vector2 direction = (PointerPosition - (Vector2)transform.position).normalized;
        transform.right = direction;

        Vector2 scale = transform.localScale;
        if (direction.x < 0)
        {
            scale.y = -1;
        }
        else if (direction.x > 0)
        {
            scale.y = 1;
        }
        transform.localScale = scale;

        // Adjust sorting order if needed, or remove if unnecessary
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
        Gizmos.color = Color.red;
        Vector3 position = attackOrigin == null ? Vector3.zero : attackOrigin.position;
        Gizmos.DrawWireSphere(position, attackRadius);
    }

    public void DetectColliders()
    {
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(attackOrigin.position, attackRadius))
        {
            if (collider.isTrigger == false)
                continue;
            // Debug.Log(collider.name);
            Health health;
            if (health = collider.GetComponent<Health>())
            {
                health.GetHit(1, gameObject);
            }
        }
    }
}
