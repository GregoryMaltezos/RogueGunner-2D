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


    /// <summary>
    /// Resets the IsAttacking flag, allowing the next attack.
    /// </summary>
    public void ResetIsAttacking()
    {
        IsAttacking = false;
    }

    /// <summary>
    /// Updates the weapon's orientation and sorting order based on the direction of the pointer.
    /// </summary>
    private void Update()
    {
        if (IsAttacking)
            return; // Skip update while attacking

        Vector2 direction = (PointerPosition - (Vector2)transform.position).normalized; // Calculate the direction from the current position to the pointer position
        transform.right = direction; // Rotate the weapon to face the pointer

        Vector2 scale = transform.localScale; // Flip the scale based on the direction of the pointer
        if (direction.x < 0) 
        {
            scale.y = -1; // Flip to the left
        }
        else if (direction.x > 0)
        {
            scale.y = 1; // Flip to the right
        }
        transform.localScale = scale;
        // Adjust weapon's sorting order based on rotation
        if (transform.eulerAngles.z > 0 && transform.eulerAngles.z < 180) 
        {
            weaponRenderer.sortingOrder = characterRenderer.sortingOrder - 1; // Behind the character
        }
        else
        {
            weaponRenderer.sortingOrder = characterRenderer.sortingOrder + 1; // In front of the character
        }
    }

    /// <summary>
    /// Initiates an attack, spawns projectiles, and handles attack delays.
    /// </summary>
    public void Attack()
    {
        if (attackBlocked)
            return; // Block attacks while attack delay is in effect

        animator.SetTrigger("Attack");
        IsAttacking = true;
        attackBlocked = true;
        StartCoroutine(DelayAttack()); // Start the delay before the next attack

        // Call the method to spawn projectiles based on the current attack type
        SpawnProjectiles();

        // Switch attack type for the next attack
        isCardinalAttack = !isCardinalAttack;
    }


    /// <summary>
    /// Coroutine to implement a delay between attacks, allowing the attack to be blocked.
    /// </summary>
    /// <returns>Yield instruction for coroutine.</returns>
    private IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(delay);
        attackBlocked = false;
    }

    /// <summary>
    /// Spawns projectiles based on the current attack type (cardinal or diagonal).
    /// </summary>
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

    /// <summary>
    /// Draws a wireframe circle in the editor to visualize the attack radius.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 position = circleOrigin == null ? Vector3.zero : circleOrigin.position;
        Gizmos.DrawWireSphere(position, radius);
    }

    /// <summary>
    /// Detects colliders within the attack radius and applies damage to any affected entities.
    /// </summary>
    public void DetectColliders()
    {
        // Detect all colliders within the circle of the specified radius
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(circleOrigin.position, radius))
        {
            if (collider.isTrigger == false)
                continue; // Skip colliders that are not triggers

            Health health;
            // If the collider has a Health component, apply damage
            if ((health = collider.GetComponent<Health>()) != null)
            {
                health.GetHit(1, transform.parent.gameObject); // Apply damage to the target
            }
        }
    }
}
