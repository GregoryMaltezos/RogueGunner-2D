using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponParent : MonoBehaviour
{
    public SpriteRenderer characterRenderer, weaponRenderer;
    public Vector2 PointerPosition { get; set; }

    public Animator animator;
    public float delay = 0.3f;
    private bool attackBlocked;

    public bool IsAttacking { get; private set; }

    public Transform circleOrigin;
    public float radius;

    public GameObject projectilePrefab; // Reference to the projectile prefab
    public float projectileSpeed = 5f; // Speed of the projectile

    private bool isCardinalAttack = true; // Track the current attack type

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

        if (transform.eulerAngles.z > 0 && transform.eulerAngles.z < 180)
        {
            weaponRenderer.sortingOrder = characterRenderer.sortingOrder - 1;
        }
        else
        {
            weaponRenderer.sortingOrder = characterRenderer.sortingOrder + 1;
        }
    }

    public void Attack()
    {
        if (attackBlocked)
            return;

        animator.SetTrigger("Attack");
        IsAttacking = true;
        attackBlocked = true;
        StartCoroutine(DelayAttack());

        // Call the method to spawn projectiles based on the current attack type
        SpawnProjectiles();

        // Switch attack type for the next attack
        isCardinalAttack = !isCardinalAttack;
    }

    private IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(delay);
        attackBlocked = false;
    }

    private void SpawnProjectiles()
    {
        // Cardinal directions (North, East, South, West)
        Vector2[] cardinalDirections = new Vector2[]
        {
            Vector2.up,    // North
            Vector2.right, // East
            Vector2.down,  // South
            Vector2.left   // West
        };

        // Diagonal directions (NorthEast, NorthWest, SouthEast, SouthWest)
        Vector2[] diagonalDirections = new Vector2[]
        {
            new Vector2(1, 1).normalized, // NorthEast
            new Vector2(-1, 1).normalized, // NorthWest
            new Vector2(1, -1).normalized, // SouthEast
            new Vector2(-1, -1).normalized  // SouthWest
        };

        // Choose the correct direction array based on the current attack type
        Vector2[] directionsToUse = isCardinalAttack ? cardinalDirections : diagonalDirections;

        foreach (Vector2 direction in directionsToUse)
        {
            // Calculate the spawn position slightly offset from the circleOrigin
            Vector2 spawnPosition = (Vector2)circleOrigin.position + direction * radius;

            // Instantiate the projectile at the calculated position and rotation
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            // Set the direction of the projectile based on the cardinal/diagonal direction
            DemonBullet projScript = projectile.GetComponent<DemonBullet>();
            if (projScript != null)
            {
                projScript.SetDirection(direction); // Assign the movement direction
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 position = circleOrigin == null ? Vector3.zero : circleOrigin.position;
        Gizmos.DrawWireSphere(position, radius);
    }

    public void DetectColliders()
    {
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(circleOrigin.position, radius))
        {
            if (collider.isTrigger == false)
                continue;

            Health health;
            if ((health = collider.GetComponent<Health>()) != null)
            {
                health.GetHit(1, transform.parent.gameObject);
            }
        }
    }
}
