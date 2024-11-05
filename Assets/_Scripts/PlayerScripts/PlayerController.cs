using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public static event Action OnPlayerDeath;
    public static event Action<float> OnPlayerWalkDistance;

    // Movement Parameters
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dashSpeed = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float invincibilityDuration = 0.5f;
    private bool isDead = false;
    // Attack Parameters
    [Header("Attack Settings")]
    public float attackCooldown = 0.5f;  // Cooldown time between attacks
    public float attackDelay = 1.0f;      // Time delay before the next attack can be initiated
    private bool canAttack = true;         // Flag to prevent spamming
    public bool isAttacking = false;       // Flag to check if the player is attacking
    private bool isAttackDelayActive = false; // Flag to check if the delay is active
    private bool canFireWeapon = true;     // Control if the player can fire the weapon
    public int currentAmmo = 30;           // Current ammo count

    // State Flags
    [Header("State Flags")]
    private bool isDashing = false;
    private bool canDash = true;
    public bool isInvincible = false;

    // References
    [Header("References")]
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private Transform spriteTransform;
    private Vector3 originalScale;
    private BoxCollider2D playerCollider;
    private Camera mainCamera;
    public Transform weaponParent;
    public PlayerHealth playerHealth;
    public GameObject deathMenu;

    // Facing Direction
    [Header("Facing Direction")]
    private bool facingRight = true;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Prevent player object from being destroyed on scene load
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;

        deathMenu = GameObject.FindGameObjectWithTag("DeathMenu"); // Search by tag
        if (deathMenu != null)
        {
            deathMenu.SetActive(false); // Ensure the death menu is hidden at the start
        }
        else
        {
            Debug.LogError("Death menu not found in the scene. Please ensure it is tagged correctly.");
        }
    }

    void Update()
    {
        // Input handling
        if (isDead) return; // Skip processing if the player is dead

        // Input handling
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");


        // Dash input
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        // Attack input (V key)
        if (Input.GetKeyDown(KeyCode.V) && canAttack && !isAttacking && !isAttackDelayActive)
        {
            StartCoroutine(Attack());
        }

        // Weapon fire input (left-click)
        if (Input.GetMouseButtonDown(0) && canFireWeapon && !isAttacking)
        {
            FireWeapon();
        }

        // Flip the sprite based on the mouse position
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        FlipPlayerBasedOnMouse(mousePos);

        // Correct weapon orientation based on mouse position
        CorrectWeaponOrientation(mousePos);
        if (playerHealth.currentHealth <= 0)
        {
            Die();
        }

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
                Vector3 gunScale = new Vector3(Mathf.Abs(gun.localScale.x), gun.localScale.y, gun.localScale.z);
                gunScale.x *= -1;
                gun.localScale = gunScale;

                GunRotation gunRotation = gun.GetComponent<GunRotation>();
                if (gunRotation != null)
                {
                    gunRotation.UpdateGunRotation();
                }
            }
        }
    }

    void CorrectWeaponOrientation(Vector3 mousePos)
    {
        if (weaponParent != null)
        {
            foreach (Transform child in weaponParent)
            {
                GunRotation gunRotation = child.GetComponent<GunRotation>();
                if (gunRotation != null)
                {
                    Vector3 direction = mousePos - child.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    child.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                }
            }
        }
    }

    IEnumerator Attack()
    {
        canAttack = false;    // Prevent further attacks until cooldown ends
        isAttacking = true;   // Prevent actions during the attack animation
        canFireWeapon = false;  // Prevent weapon firing during attack

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Wait for the attack animation duration (adjust this duration as needed)
        yield return new WaitForSeconds(1.4f); // Increase duration for the attack animation

        isAttacking = false;  // Allow other actions after the attack animation finishes
        canFireWeapon = true;  // Allow weapon fire after attack

        // Start the manual delay after the attack is complete
        yield return StartCoroutine(AttackDelay());

        // Wait for the cooldown period before allowing another attack
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true; // Reset attack flag after cooldown
    }


    IEnumerator AttackDelay()
    {
        isAttackDelayActive = true; // Set the flag to indicate the delay is active
        yield return new WaitForSeconds(attackDelay); // Wait for the specified attack delay
        isAttackDelayActive = false; // Reset the flag after the delay
    }

    void FireWeapon()
    {
        if (currentAmmo > 0)
        {
            Debug.Log("Weapon fired!");
            currentAmmo--;
        }
        else
        {
            Debug.Log("Out of ammo!");
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        isInvincible = true;
        animator.SetTrigger("Dash");

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Arrow"), true);

        rb.velocity = movement * dashSpeed;
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        rb.velocity = Vector2.zero;
        isInvincible = false;

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Arrow"), false);

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDashing && collision.gameObject.CompareTag("Obstacle"))
        {
            // Handle collision with obstacles during dash
        }
        else if (!isInvincible)
        {
            // Regular damage logic
        }
    }

    public void Die()
    {
        if (isDead) return; // Prevent multiple calls to Die

        Debug.Log("Player died!");
        isDead = true;

        rb.velocity = Vector2.zero;
        DisableColliders();
        DisableWeapons();

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Die");
        }

        OnPlayerDeath?.Invoke();
        StartCoroutine(ShowDeathMenuAfterDelay());
    }


    // Method to disable all player colliders
    private void DisableColliders()
    {
        if (playerCollider != null)
        {
            playerCollider.enabled = false; // Disable the player's collider
        }
    }

    // Method to disable all weapon objects under WeaponsParent
    private void DisableWeapons()
    {
        if (weaponParent != null)
        {
            foreach (Transform weapon in weaponParent)
            {
                weapon.gameObject.SetActive(false); // Disable each weapon game object
            }
        }
    }
    private void EnableColliders()
    {
        if (playerCollider != null)
        {
            playerCollider.enabled = true; // Enable the player's collider
        }
    }

    private void EnableWeapons()
    {
        if (weaponParent != null)
        {
            foreach (Transform weapon in weaponParent)
            {
                weapon.gameObject.SetActive(true); // Enable each weapon game object
            }
        }
    }



    private IEnumerator ShowDeathMenuAfterDelay()
    {
        // Wait for the death animation duration (adjust based on your animation length)
        yield return new WaitForSeconds(3f); // Adjust this value as needed

        // Show the death menu after the animation finishes
        if (deathMenu != null)
        {
            deathMenu.SetActive(true); // Show the death menu
        }
    }

    void ResetPlayerState()
    {
        currentAmmo = 30; // Reset to initial ammo count
        isDead = false;
        isAttacking = false;
        isDashing = false;
        isInvincible = false;
        canAttack = true;
        canDash = true;
        canFireWeapon = true;

        // Reset animation states if necessary
        if (animator != null)
        {
            animator.SetBool("IsDead", false);
            animator.SetBool("IsMoving", false);
        }

        // Re-enable colliders and weapons
        EnableColliders();
        EnableWeapons();

        // Hide death menu if it was previously shown
        if (deathMenu != null)
        {
            deathMenu.SetActive(false);
        }
    }


    public bool IsFacingRight()
    {
        return facingRight;
    }

    public bool IsDashing()
    {
        return isDashing;
    }
}
