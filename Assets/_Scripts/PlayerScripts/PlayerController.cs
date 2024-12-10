using System;
using System.Collections;
using UnityEngine;
using FMOD.Studio;
using UnityEngine.InputSystem;
using FMODUnity;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public static event Action OnPlayerDeath;
    public static event Action<float> OnPlayerWalkDistance;

    private EventInstance playerFootsteps;
    [SerializeField] private EventReference SwordDown;
    [SerializeField] private EventReference SwordUp;
    [SerializeField] private EventReference Slide;
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

    private NewControls inputActions; // New input actions class
    private InputAction moveAction;
    private InputAction dashAction;
    private InputAction attackAction;
    private InputAction fireWeaponAction;
    private InputAction mousePositionAction;
    // Facing Direction
    [Header("Facing Direction")]
    private bool facingRight = true;
    [SerializeField] private EventReference death;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        inputActions = new NewControls();
        moveAction = inputActions.PlayerInput.Movement; // Assign the correct move action
        dashAction = inputActions.PlayerInput.Dash;
        attackAction = inputActions.PlayerInput.Sword;
        fireWeaponAction = inputActions.PlayerInput.Attack;
        mousePositionAction = inputActions.PlayerInput.PointerPosition;
        inputActions.Enable(); // This should be called to activate input actions

    }



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;

        playerFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.playerFootsteps);
    }

    void Update()
    {
        if (isDead) return;

        // Get movement input from new system
        Vector2 inputMovement = moveAction.ReadValue<Vector2>();
        movement.x = inputMovement.x;
        movement.y = inputMovement.y;

        // Dash input
        if (dashAction.triggered && canDash)
        {
            StartCoroutine(Dash());
        }

        // Attack input (V key)
        if (attackAction.triggered && canAttack && !isAttacking && !isAttackDelayActive)
        {
            StartCoroutine(Attack());
        }

        // Weapon fire input (left-click)
        if (fireWeaponAction.triggered && canFireWeapon && !isAttacking)
        {
            FireWeapon();
        }

        // Continuously check and update the player's facing direction based on the mouse position
        Vector3 mousePos = mousePositionAction.ReadValue<Vector2>();
        FlipPlayerBasedOnMouse(mousePos); // Flip player based on mouse position

        // Update weapon rotation based on mouse position
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
    // Convert mouse position from screen space to world space
    Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));

    // Compare the mouse's world position with the player's position
    if (worldMousePos.x < transform.position.x && facingRight)
    {
        Flip();
    }
    else if (worldMousePos.x > transform.position.x && !facingRight)
    {
        Flip();
    }
}




    void Flip()
    {
        // Toggle the facing direction
        facingRight = !facingRight;

        // Flip the player sprite by changing its scale
        Vector3 playerScale = transform.localScale;
        playerScale.x *= -1; // Flip the player's scale
        transform.localScale = playerScale;
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

                    // Correct weapon rotation based on mouse position and player facing direction
                    if (!facingRight)
                    {
                        angle += 180f; // If the player is facing left, flip the gun's angle
                    }

                    child.rotation = Quaternion.Euler(new Vector3(0, 0, angle)); // Set the gun's rotation
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
        yield return new WaitForSeconds(0.2f); // Adjust delay duration as needed

        // Play the first audio clip
        AudioManager.instance.PlayOneShot(SwordDown, this.transform.position);

        // Small delay between audio clips
        yield return new WaitForSeconds(0.3f); // Adjust delay duration as needed

        // Play the second audio clip
        AudioManager.instance.PlayOneShot(SwordUp, this.transform.position);

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
        AudioManager.instance.PlayOneShot(Slide, this.transform.position);
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
            UpdateSound();
        }
        UpdateSound();
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

        rb.velocity = Vector2.zero; // Stop all movement
        DisableColliders();
        DisableWeapons();

        // Disable player input
        movement = Vector2.zero; // Stop the player's movement
        isInvincible = true; // Optionally make the player invincible during death animation

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            AudioManager.instance.PlayOneShot(death, this.transform.position);
            animator.SetTrigger("Die");
        }

        // Invoke death event for death menu or game logic
        OnPlayerDeath?.Invoke();
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


    }


    public bool IsFacingRight()
    {
        return facingRight;
    }

    public bool IsDashing()
    {
        return isDashing;
    }
    

    private void UpdateSound()
    {
        if ((rb.velocity.x != 0 || rb.velocity.y != 0) && !isDashing)
        {
            PLAYBACK_STATE playbackState;
            playerFootsteps.getPlaybackState(out playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                playerFootsteps.start();
            }
        }
        else
        {
            playerFootsteps.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        }
    }
}