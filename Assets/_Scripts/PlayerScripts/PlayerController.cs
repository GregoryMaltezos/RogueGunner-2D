using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public static event Action OnPlayerDeath;
    public static event Action<float> OnPlayerWalkDistance;

    public float moveSpeed = 5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float invincibilityDuration = 0.5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private Transform spriteTransform;
    private Vector3 originalScale;
    private bool isDashing = false;
    private bool canDash = true;
    private bool isInvincible = false;

    private Camera mainCamera; // For getting mouse position
    private bool facingRight = true; // To track the player’s facing direction
    public Transform weaponParent;

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
        mainCamera = Camera.main;

        if (animator == null)
        {
            Debug.LogError("Animator not found. Make sure your character prefab has a child with an Animator component.");
        }

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

        // Flip the sprite based on the mouse position
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        FlipPlayerBasedOnMouse(mousePos);

        // Correct weapon orientation based on mouse position
        CorrectWeaponOrientation(mousePos);

        // Update the Speed parameter in the Animator
        if (animator != null)
        {
            bool isMoving = movement.sqrMagnitude > 0;
            animator.SetBool("IsMoving", isMoving);
        }
    }

    void FlipPlayerBasedOnMouse(Vector3 mousePos)
    {
        if (mousePos.x < transform.position.x && facingRight)
        {
            Flip();
        }
        else if (mousePos.x > transform.position.x && !facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        // Toggle the facing direction
        facingRight = !facingRight;

        // Flip the player sprite
        Vector3 playerScale = transform.localScale;
        playerScale.x *= -1; // Flip the player's scale
        transform.localScale = playerScale;

        // Ensure all gun objects under this GunRotation are flipped
        if (weaponParent != null)
        {
            foreach (Transform gun in weaponParent)
            {
                // Reset the gun's scale before flipping
                Vector3 gunScale = new Vector3(Mathf.Abs(gun.localScale.x), gun.localScale.y, gun.localScale.z);

                // Flip the gun's scale
                gunScale.x *= -1; // Flip the gun's scale
                gun.localScale = gunScale;

                // Call the UpdateGunRotation method on the gun scripts
                GunRotation gunRotation = gun.GetComponent<GunRotation>();
                if (gunRotation != null)
                {
                    gunRotation.UpdateGunRotation(); // Ensure the gun's rotation is updated immediately
                }

                // Debug: Log each gun's scale after setting
                Debug.Log($"Gun Scale after Flip: {gun.localScale}");
            }
        }

        // Debug: Log the player scale after flipping
        Debug.Log($"Player Scale after Flip: {transform.localScale}");
    }

    // Method to correct the weapon orientation
    void CorrectWeaponOrientation(Vector3 mousePos)
    {
        if (weaponParent != null)
        {
            foreach (Transform child in weaponParent)
            {
                GunRotation gunRotation = child.GetComponent<GunRotation>();
                if (gunRotation != null)
                {
                    // Calculate the angle to rotate the gun towards the mouse
                    Vector3 direction = mousePos - child.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    // Update the gun's rotation based on the angle
                    child.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                }
            }
        }
    }

    void Attack()
    {
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

        // Wait for the dash duration
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        isInvincible = false;

        // Start dash cooldown
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
            rb.velocity = movement * moveSpeed;
        }
    }

    public void Die()
    {
        Debug.Log("Player died!");
        OnPlayerDeath?.Invoke();
    }

    public bool IsFacingRight()
    {
        return facingRight;
    }
}
