using UnityEngine;
using System.Collections;

public class ReaperBoss : MonoBehaviour
{
    public Animator animator;             // Reference to the Animator component
    public Rigidbody2D rb;                // Reference to the Rigidbody2D component
    public GameObject spawnPrefab;        // Prefab to spawn above the boss's head
    public float moveSpeed = 10f;         // Speed at which the boss dashes towards the player
    public float spawnHeight = 2f;        // Height above the boss where the prefab will spawn
    public float delayBeforeMoving = 2f;  // Time to wait before starting the movement and attack animation
    public float attackRadius = 5f;       // Radius within which the boss will start attacking
    public float dashDuration = 0.5f;     // Duration of the dash towards the player
    public float attackPause = 1f;        // Time to pause after an attack
    public float additionalDelay = 1f;    // Additional delay before dashing

    private Transform player;             // Reference to the player's Transform
    private bool isAttacking = false;     // Flag to indicate if the boss is attacking

    private void Start()
    {
        // Automatically find the player in the scene
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Player not found. Make sure the player GameObject has the 'Player' tag.");
            return; // Exit if player is not found
        }

        // Start the attack sequence
        StartCoroutine(AttackSequence());
    }

    private IEnumerator AttackSequence()
    {
        while (true)
        {
            // Spawn the prefab above the boss's head
            SpawnPrefab();
            // Wait for the prefab to appear and then wait a bit longer
            yield return new WaitForSeconds(0.5f + additionalDelay);
            // Start the dash
            yield return StartCoroutine(DashTowardsPlayer());
            // Pause after attacking
            yield return new WaitForSeconds(attackPause);
        }
    }

    private void SpawnPrefab()
    {
        if (spawnPrefab != null)
        {
            Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y + spawnHeight, transform.position.z);
            GameObject spawnedObject = Instantiate(spawnPrefab, spawnPosition, Quaternion.identity);
            Destroy(spawnedObject, 1.2f); // Destroy the prefab after half a second
        }
    }

    private IEnumerator DashTowardsPlayer()
    {
        if (player == null) yield break;

        // Play attack animation
        animator.SetTrigger("AttackAnimation");
        isAttacking = true;

        // Calculate the direction to the player
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        // Dash duration
        yield return new WaitForSeconds(dashDuration);

        // Stop the dash
        rb.velocity = Vector2.zero;
        isAttacking = false;
    }
}
