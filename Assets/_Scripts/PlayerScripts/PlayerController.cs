using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;


    public static event Action OnPlayerDeath;
    public static event Action<float> OnPlayerWalkDistance;

    public float moveSpeed = 5f;
    public float dashSpeed = 10f; // Speed of the dash
    public float dashDuration = 0.2f; // Duration of the dash in seconds
    public float dashCooldown = 1f; // Cooldown between dashes in seconds
    public float invincibilityDuration = 0.5f; // Duration of invincibility after dashing

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private Transform spriteTransform;
    private Vector3 originalScale;
    private bool isDashing = false;
    private bool canDash = true;
    private bool isInvincible = false;




    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Duplicate PlayerController instance found. Only one should exist.");
            Destroy(gameObject);
        }
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;

        if (animator == null)
        {
            Debug.LogError("Animator not found. Make sure your character prefab has a child with an Animator component.");
        }

        // Store the original scale
        originalScale = spriteTransform.localScale;
    }

    void Update()
    {
        // Input handling
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

        // Dash input
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        // Attack input
        if (Input.GetKeyDown(KeyCode.V))
        {
            Attack();
        }

        // Flip the sprite based on the movement direction
        if (movement.x < 0)
        {
            spriteTransform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        }
        else if (movement.x > 0)
        {
            spriteTransform.localScale = originalScale;
        }

        // Update the Speed parameter in the Animator
        if (animator != null)
        {
            // Check if the player is moving
            bool isMoving = movement.sqrMagnitude > 0;
            animator.SetBool("IsMoving", isMoving);
        }
    }

    void Attack()
    {
        // Trigger the attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        isInvincible = true;

        // Trigger Dash animation
        animator.SetTrigger("Dash");

        // Wait for the duration of the dash
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        isInvincible = false;

        // Start cooldown
        StartCoroutine(DashCooldown());
    }

    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            // Smooth Movement
            rb.velocity = movement * moveSpeed;
        }
    }

    public void Die()
    {
        Debug.Log("Player died!"); // Example: Log message
                                   // Add logic here to handle player death
        OnPlayerDeath?.Invoke();
    }
}