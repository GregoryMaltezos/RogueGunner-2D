using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class Ballista : EnemyAI
{
    public float detectionRange = 10f;   // Range within which the ballista can detect the player
    public float rotationSpeed = 5f;      // Speed of rotation towards the player
    public GameObject projectilePrefab;   // The projectile the ballista will shoot
    public Transform firePoint;           // The point from where the projectile will be fired
    public float shootInterval = 1f;      // Time between shots
    private Transform player;             // Reference to the player's transform
    private float shootTimer;

    [SerializeField] private EventReference bowPull;
    [SerializeField] private EventReference bowRelease;

    // A reference to the EnemyManager and whether the ballista is actively chasing the player
    private bool isChasing = false;

    private void Start()
    {
        // Automatically find the player in the scene by tag
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Register with the EnemyManager
        EnemyManager.instance?.RegisterEnemy(this);
    }

    private void Update()
    {
        DetectAndShoot();
    }

    private void DetectAndShoot()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (!isChasing)
            {
                // Notify the EnemyManager that this enemy is chasing the player
                EnemyManager.instance?.NotifyEnemyChasing(this);
                isChasing = true;
            }

            Vector2 direction = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            shootTimer += Time.deltaTime;
            if (shootTimer >= shootInterval)
            {
                AudioManager.instance.PlayOneShot(bowPull, this.transform.position);
                Shoot();
                shootTimer = 0f;
            }
        }
        else
        {
            if (isChasing)
            {
                // Notify the EnemyManager that this enemy stopped chasing the player
                EnemyManager.instance?.NotifyEnemyStoppedChasing(this);
                isChasing = false;
            }
        }
    }

    private void Shoot()
    {
        AudioManager.instance.PlayOneShot(bowRelease, this.transform.position);
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = (player.position - firePoint.position).normalized;
            rb.velocity = direction * 10f;
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        else
        {
            Debug.LogWarning("Animator component not found!");
        }
    }

    // OnDrawGizmos for visualizing the detection range in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    // Clean up and deregister from the EnemyManager if necessary
    private void OnDestroy()
    {
        EnemyManager.instance?.DeregisterEnemy(this);
    }
}
