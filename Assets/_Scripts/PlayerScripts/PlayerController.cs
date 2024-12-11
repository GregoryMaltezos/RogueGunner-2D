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
    private Vector3 lastPosition;
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
    public static event Action OnEnemyKill;


    /// <summary>
    /// Singleton pattern to ensure only one instance of PlayerController exists.
    /// Initializes input actions and enables them.
    /// </summary>
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
        inputActions.Enable(); // Activate input actions

    }


    
    /// <summary>
    /// Sets up necessary references (Rigidbody, Animator, Camera, etc.) at the start.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;
        lastPosition = transform.position;
        playerFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.playerFootsteps);
    }

    /// <summary>
    /// Main update loop, handles movement, actions, and events based on player input.
    /// </summary>

    void Update()
    {
        if (isDead) return; // Skip if the player is dead
        // Detect movement distance for challenge
        float distanceTravelled = Vector3.Distance(lastPosition, transform.position);

        // If the player has moved a significant distance, trigger the event
        if (distanceTravelled > 0.1f) // Example threshold for triggering event
        {
            OnPlayerWalkDistance?.Invoke(distanceTravelled);
        }

        // Update the last position for the next frame
        lastPosition = transform.position;
        // Get movement input from new system
        Vector2 inputMovement = moveAction.ReadValue<Vector2>();
        movement.x = inputMovement.x;
        movement.y = inputMovement.y;

        // Dash input
        if (dashAction.triggered && canDash)
        {
            StartCoroutine(Dash()); // Dash if possible
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

    /// <summary>
    /// Flips the player's facing direction based on mouse position.
    /// </summary>
    void FlipPlayerBasedOnMouse(Vector3 mousePos)
{
    // Convert mouse position from screen space to world space
    Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));

    // Compare the mouse's world position with the player's position
    if (worldMousePos.x < transform.position.x && facingRight)
    {
        Flip(); // Flip left
        }
    else if (worldMousePos.x > transform.position.x && !facingRight)
    {
        Flip(); // Flip right
        }
}



    /// <summary>
    /// Flips the player's sprite (mirror horizontally) to change facing direction.
    /// </summary>
    void Flip()
    {
        // Toggle the facing direction
        facingRight = !facingRight;

        // Flip the player sprite by changing its scale
        Vector3 playerScale = transform.localScale;
        playerScale.x *= -1; // Mirror the player sprite on the X axis
        transform.localScale = playerScale;
    }

    /// <summary>
    /// Corrects the weapon's rotation based on mouse position and player facing direction.
    /// </summary>
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



    /// <summary>
    /// Initiates the attack sequence, including playing animations and sound effects.
    /// </summary>
    IEnumerator Attack()
    {
        canAttack = false;    // Prevent further attacks until cooldown ends
        isAttacking = true;   // Prevent actions during the attack animation
        canFireWeapon = false;  // Prevent weapon firing during attack

        // Play attack animation 
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

        // Wait for the attack animation duration 
        yield return new WaitForSeconds(1.4f); 

        isAttacking = false;  // Allow other actions after the attack animation finishes
        canFireWeapon = true;  // Allow weapon fire after attack

        // Start the manual delay after the attack is complete
        yield return StartCoroutine(AttackDelay());

        // Wait for the cooldown period before allowing another attack
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true; // Reset attack flag after cooldown
    }


    /// <summary>
    /// Adds a delay after each attack before another can be triggered.
    /// </summary>
    IEnumerator AttackDelay()
    {
        isAttackDelayActive = true; // Set the flag to indicate the delay is active
        yield return new WaitForSeconds(attackDelay); // Wait for the specified attack delay
        isAttackDelayActive = false; // Reset the flag after the delay
    }

    /// <summary>
    /// Fires the weapon if the player has ammo.
    /// </summary>
    void FireWeapon()
    {
        if (currentAmmo > 0)
        {
            Debug.Log("Weapon fired!");
            currentAmmo--; // Decrease ammo count
        }
        else
        {
            Debug.Log("Out of ammo!");
        }
    }


    /// <summary>
    /// Handles the dash action, including invincibility and collision ignoring.
    /// </summary>
    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        isInvincible = true;
        animator.SetTrigger("Dash");
        AudioManager.instance.PlayOneShot(Slide, this.transform.position);
        // Ignore collisions with certain objects while dashing
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Arrow"), true);

        rb.velocity = movement * dashSpeed; // Dash movement
        yield return new WaitForSeconds(dashDuration); // Wait for dash duration

        isDashing = false;
        rb.velocity = Vector2.zero; // Stop movement after dash
        isInvincible = false; // Disable invincibility

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Arrow"), false); // Re-enable collision

        StartCoroutine(DashCooldown()); // Start cooldown for dash
    }

    /// <summary>
    /// Handles the cooldown period after a dash.
    /// </summary>
    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true; // Re-enable dashing after cooldown
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

    /// <summary>
    /// Handles the player's death, triggering death animation and events.
    /// </summary>
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
        isInvincible = true; // make the player invincible during death animation

        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            AudioManager.instance.PlayOneShot(death, this.transform.position);
            animator.SetTrigger("Die");
        }

        // Invoke death event for death menu or game logic
        OnPlayerDeath?.Invoke();
    }



    /// <summary>
    /// Disables the player's collider to prevent interactions or collisions.
    /// </summary>
    private void DisableColliders()
    {
        if (playerCollider != null)
        {
            playerCollider.enabled = false; // Disable the player's collider
        }
    }

    /// <summary>
    /// Disables all weapons under the parent object `weaponParent`. This can be used when the player is not allowed to use weapons (e.g., after death or during certain states).
    /// </summary>
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

    /// <summary>
    /// Enables the player's collider, allowing interactions and collisions.
    /// </summary>
    private void EnableColliders()
    {
        if (playerCollider != null)
        {
            playerCollider.enabled = true; // Enable the player's collider
        }
    }

    /// <summary>
    /// Enables all weapons under the parent object `weaponParent`. This can be used when the player is allowed to use weapons (e.g., after respawn or during normal gameplay).
    /// </summary>
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




    /// <summary>
    /// Resets various player states such as ammo, attack/dash flags, death status, and animation states.
    /// This is usually called when the player respawns or after certain events, ensuring the player starts fresh.
    /// </summary>
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

    /// <summary>
    /// Checks if the player is currently facing to the right.
    /// </summary>
    public bool IsFacingRight()
    {
        return facingRight;
    }
    /// <summary>
    /// Checks if the player is currently dashing.
    /// </summary>
    public bool IsDashing()
    {
        return isDashing;
    }

    /// <summary>
    /// Updates the player's footstep sound based on whether the player is moving. If the player is moving and not dashing, the footsteps sound is played.
    /// Otherwise, the sound is stopped or faded out.
    /// </summary>
    private void UpdateSound()
    {
        if ((rb.velocity.x != 0 || rb.velocity.y != 0) && !isDashing) // Check if the player is moving and not dashing
        {
            PLAYBACK_STATE playbackState;
            playerFootsteps.getPlaybackState(out playbackState); // Get current playback state of footsteps sound
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED)) // If the footsteps sound is not playing
            {
                playerFootsteps.start(); // Start the footsteps sound
            }
        }
        else
        {
            playerFootsteps.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); // Stop the footsteps sound with fade out if the player is not moving

        }
    }
}