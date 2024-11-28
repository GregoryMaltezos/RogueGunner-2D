using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
public class Ballista : MonoBehaviour
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
    private void Start()
    {
        // Automatically find the player in the scene by tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        DetectAndShoot();
    }

    private void DetectAndShoot()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
      //  Debug.Log($"Ballista Position: {transform.position}, Player Position: {player.position}");
      //  Debug.Log($"Distance to player: {distanceToPlayer}");

        if (distanceToPlayer <= detectionRange)
        {
           // Debug.Log("Player detected");

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
         //   Debug.Log("Player out of range");
        }
    }

    private void Shoot()
    {
        //  Debug.Log("Shooting projectile");
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
          //  Debug.Log("Shoot trigger set");
        }
        else
        {
            Debug.LogWarning("Animator component not found!");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
