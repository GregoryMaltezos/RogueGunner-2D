using UnityEngine;
using System.Collections;
using FMODUnity;
using UnityEngine.InputSystem;
public class Gun : MonoBehaviour
{
    public static Gun instance;
    public GameObject projectilePrefab; // Prefab of the projectile
    public Transform firePoint; // Point from which the projectile will be fired
    public float projectileSpeed = 20f; // Speed of the projectile
    public bool isShotgun = false; // Is this a shotgun?
    public int shotgunPelletCount = 5; // Number of pellets for shotgun
    public float shotgunSpreadAngle = 20f; // Spread angle for shotgun
    public bool isAutomatic = false; // Is this gun automatic?
    public float fireRate = 0.1f; // Time between shots
    public int maxAmmo = 90; // Maximum ammo the gun can hold
    public int ammoPerClip = 30; // Ammo per clip
    public float reloadTime = 2f; // Time it takes to reload
    public bool infiniteAmmo = false; // Is ammo infinite?
    private bool facingRight = true;  // Assume initially facing right

    [HideInInspector] public int currentClipAmmo; // Current ammo in the clip
    [HideInInspector] public int bulletsRemaining; // Total bullets remaining
    [HideInInspector] public int clipsRemaining; // Total clips remaining

    private bool isReloading = false; // Is the gun currently reloading?
    private float nextFireTime = 0f; // Time when the gun can fire again
    private int gunIndex; // Index of the gun in the weapon manager
    private int shotsFired; // Counter for shots fired when infinite ammo is enabled
    private const int maxShotsWithInfiniteAmmo = 12; // Max bullets before needing to reload

    private NewControls inputActions; // Reference to the NewControls input asset
    private InputAction reloadAction;
    private InputAction fireAction;
    private bool isFiring = false;
    public bool isFirstSpawn = true;
    [SerializeField] private EventReference gunFired;
    [SerializeField] private EventReference gunReload;

    /// <summary>
    /// Initializes the Gun class, sets up input actions, and initializes ammo state.
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        inputActions = new NewControls(); // Create an instance of the NewControls asset
        reloadAction = inputActions.PlayerInput.Reload;
        fireAction = inputActions.PlayerInput.Fire;
    }

    /// <summary>
    /// Initializes ammo and sets up weapon state.
    /// </summary>
    void Start()
    {
        if (WeaponManager.instance == null)
        {
            Debug.LogError("WeaponManager not found! Ensure WeaponManager is in the scene and properly initialized.");
            return;
        }

        gunIndex = WeaponManager.instance.GetGunIndex(this.gameObject);

        if (gunIndex != -1)
        {
            if (isFirstSpawn)
            {
                // Initialize the gun ammo to full values on first spawn
                currentClipAmmo = ammoPerClip;  // Full clip loaded
                clipsRemaining = (maxAmmo - ammoPerClip) / ammoPerClip; // Reserve clips
                bulletsRemaining = maxAmmo - currentClipAmmo; // Remaining bullets in reserve
            }
            else
            {
                // On level transition, restore ammo using quarter logic
                RestoreAmmoFromLevelTransition();
            }

            // Set the ammo in WeaponManager (ensure these values are reflected correctly)
            WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);
            WeaponManager.instance.SetGunBulletsRemaining(gunIndex, bulletsRemaining);
            WeaponManager.instance.SetGunClipsRemaining(gunIndex, clipsRemaining);

            // Update the UI or ammo state
            UpdateAmmoState();
        }
        else
        {
            Debug.LogWarning("Gun index not found in WeaponManager");
        }

        inputActions.Enable();

        reloadAction.performed += OnReloadPerformed;
        fireAction.performed += OnFirePerformed;
        fireAction.canceled += OnFireCanceled; // Subscribe to stopping fire
    }


    /// <summary>
    /// Restores ammo after level transition, refilling  ammo.
    /// </summary>
    public void RestoreAmmoFromLevelTransition()
    {
        // Restore a quarter of the total ammo on level transition
        int ammoToRestore = Mathf.FloorToInt(maxAmmo / 4f); // Calculate 1/4 of max ammo

        // Add quarter ammo to the current clip and reserve ammo
        if (!infiniteAmmo)
        {
            // If the remaining bullets plus ammo to restore exceeds maxAmmo, clamp it to maxAmmo
            bulletsRemaining = Mathf.Min(bulletsRemaining + ammoToRestore, maxAmmo);

            // Update clips remaining based on the updated bullets remaining
            clipsRemaining = bulletsRemaining / ammoPerClip;
        }
        else
        {
            // If infinite ammo is enabled, we don't need to restore reserve ammo
            Debug.Log("Ammo is infinite; no bullets were restored.");
        }

        // Set the current clip ammo to the maximum per clip, unless there is less remaining ammo
        currentClipAmmo = Mathf.Min(ammoPerClip, bulletsRemaining);

        Debug.Log($"Ammo Restored on Level Transition - CurrentClipAmmo: {currentClipAmmo}, BulletsRemaining: {bulletsRemaining}, ClipsRemaining: {clipsRemaining}");

        // Update the WeaponManager with the new ammo state
        if (gunIndex != -1)
        {
            WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);
            WeaponManager.instance.SetGunBulletsRemaining(gunIndex, bulletsRemaining);
            WeaponManager.instance.SetGunClipsRemaining(gunIndex, clipsRemaining);
        }

        // Update UI to reflect the current ammo state
        UpdateAmmoState();
    }

    /// <summary>
    /// Unsubscribes from input actions when the object is destroyed.
    /// </summary>
    void OnDestroy()
    {
        reloadAction.performed -= OnReloadPerformed;
        fireAction.performed -= OnFirePerformed;
        fireAction.canceled -= OnFireCanceled; // Unsubscribe from stopping fire
        inputActions.Disable();
    }

    /// <summary>
    /// Called when fire input is detected to begin the firing process.
    /// </summary>
    private void OnFirePerformed(InputAction.CallbackContext context)
    {
        isFiring = true;

        // Start firing immediately for non-automatic guns
        if (!isAutomatic && Time.time >= nextFireTime)
        {
            AttemptToFire();
        }
    }
    /// <summary>
    /// Called when fire input is released to stop firing.
    /// </summary>
    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        isFiring = false; // Stop firing when input is released
    }

    /// <summary>
    /// Handles automatic fire and reloading input.
    /// </summary>
    void Update()
    {
        // Skip firing logic if paused or reloading
        if (FindObjectOfType<PauseMenu>()?.IsPaused == true || isReloading)
            return;

        // Handle reloading
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (clipsRemaining > 0 || infiniteAmmo)
            {
                StartCoroutine(Reload());
            }
        }

        // Handle automatic firing
        if (isFiring && isAutomatic && Time.time >= nextFireTime)
        {
            AttemptToFire();
        }
    }

    /// <summary>
    /// Initiates the reload process when the reload input is detected.
    /// </summary>
    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        // Early exit if the gun is inactive
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Cannot reload because the weapon is inactive.");
            return;
        }

        // Trigger reload if the action is performed
        if (clipsRemaining > 0 || bulletsRemaining > 0 || infiniteAmmo)
        {
            StartCoroutine(Reload());
        }

    }

    /// <summary>
    /// Attempts to fire the gun, checking if conditions are met (e.g., ammo availability).
    /// </summary>
    void AttemptToFire()
    {
        // Check if the gun is currently reloading
        if (isReloading)
        {
            Debug.Log("Cannot fire while reloading.");
            return; // Prevent firing during reload
        }

        // Check if the gun object is still valid before trying to fire
        if (gameObject == null || !gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Gun object is destroyed or not active!");
            return; // Early exit if gun object is invalid
        }

        // Proceed with firing logic
        if (infiniteAmmo || currentClipAmmo > 0)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
            UpdateAmmoState();
        }
        else if (clipsRemaining > 0)
        {
            StartCoroutine(Reload());
        }
    }


    /// <summary>
    /// Fires the gun, instantiating a projectile and applying force to it.
    /// </summary>
    void Fire()
    {
       
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()); // Calculate direction from fire point to mouse position
        Vector3 shootDirection = mousePosition - firePoint.position;   // Calculate direction from fire point to mouse position
        shootDirection.z = 0;  // Set the z-coordinate to zero to keep it in 2D space
        shootDirection = shootDirection.normalized;

        
        if (!isShotgun) // If not a shotgun, fire a single projectile
        {
            FireProjectile(shootDirection);
        }
        else
        {
            for (int i = 0; i < shotgunPelletCount; i++) // If it's a shotgun, fire multiple pellets with spread
            {
                // Calculate spread angle and apply it to the shooting direction
                float spreadAngle = Random.Range(-shotgunSpreadAngle / 2f, shotgunSpreadAngle / 2f); // Apply rotation to spread
                Vector3 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * shootDirection; // Fire the pellet with the spread direction
                FireProjectile(spreadDirection);
            }
        }

        // Handle ammo decrementing logic for both infinite and finite ammo
        if (!infiniteAmmo)
        {
            if (currentClipAmmo > 0) // Decrement ammo only if there is ammo left
            {
                currentClipAmmo--; // Decrement once per shot
                Debug.Log($"Fired! Current Clip Ammo: {currentClipAmmo}");

                // Update the WeaponManager with the new ammo state
                WeaponManager.instance.SetGunClipAmmo(WeaponManager.instance.GetCurrentGunIndex(), currentClipAmmo);
            }

            // If the clip is empty and there are clips remaining, start reloading
            if (currentClipAmmo <= 0 && clipsRemaining > 0)
            {
                StartCoroutine(Reload());
            }
        }
        else
        {
            // Handle infinite ammo logic
            if (currentClipAmmo > 0)
            {
                currentClipAmmo--; // Decrement for display purposes
                shotsFired++; // Increase shots fired count

                // Update the WeaponManager with the new ammo state
                WeaponManager.instance.SetGunClipAmmo(WeaponManager.instance.GetCurrentGunIndex(), currentClipAmmo);

                // Check if we've reached the max number of shots before reload
                if (shotsFired >= maxShotsWithInfiniteAmmo)
                {
                    StartCoroutine(Reload()); // Force reload after 12 shots
                    shotsFired = 0; // Reset the shot count after reloading
                }
            }
        }

        // Always update the UI to reflect the current state
        UpdateAmmoState(); // Ensure UI is updated after firing
    }


    /// <summary>
    /// Instantiates a projectile and applies direction and velocity to it.
    /// </summary>
    /// <param name="shootDirection">The direction the projectile should travel in</param>
    void FireProjectile(Vector3 shootDirection)
    {
        // Instantiate the projectile at the fire point position
        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = projectileInstance.GetComponent<Rigidbody2D>();  // Get the Rigidbody2D component to apply physics
        AudioManager.instance.PlayOneShot(gunFired, this.transform.position); // Play the gunfire sound
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Ensure continuous collision detection for better accuracy
            Vector2 velocity = shootDirection * projectileSpeed; // Apply velocity to the projectile based on the shoot direction
            rb.velocity = velocity;

            // If the player is facing left, flip the projectile to match the direction
            if (!facingRight)
            {
                Vector3 projectileScale = projectileInstance.transform.localScale;
                projectileScale.x = Mathf.Abs(projectileScale.x) * -1; // Flip the projectile scale
                projectileInstance.transform.localScale = projectileScale;
            }
        }
        else
        {
            Debug.LogError("Rigidbody2D component not found on the projectile prefab!");
        }
    }

    private FMOD.Studio.EventInstance reloadSoundInstance; // Declare an FMOD event instance for reload sound


    /// <summary>
    /// Handles the reload process by refilling ammo and updating the UI.
    /// </summary>
    private IEnumerator Reload()
    {
        // Check if the weapon is active before starting the reload
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Cannot reload because the weapon is inactive.");
            yield break;  // Exit the coroutine early if the weapon is inactive
        }

        // Prevent reloading if already reloading
        if (isReloading) yield break;

        // Prevent reloading if there's no reserve ammo left and no clips remaining
        if (bulletsRemaining <= 0 && clipsRemaining <= 0)
        {
            Debug.LogWarning("No ammo left to reload!");
            yield break;  // Exit if no ammo to reload
        }

        isReloading = true;
        Debug.Log("Reloading...");

        reloadSoundInstance = AudioManager.instance.PlayOneShot(gunReload, this.transform.position);

        yield return new WaitForSeconds(reloadTime);  // Wait for reload to complete

        if (infiniteAmmo)
        {
            currentClipAmmo = ammoPerClip; // Set the current clip ammo to the maximum per clip
            Debug.Log($"Reloaded with infinite ammo! Current Clip Ammo: {currentClipAmmo}");
            shotsFired = 0; // Reset shots fired count
        }
        else
        {
            // How much ammo we need to fill the clip
            int ammoNeededForClip = ammoPerClip - currentClipAmmo;

            // If reserve ammo is less than ammo needed to fill the clip, use whatever is available
            int ammoToLoad = Mathf.Min(ammoNeededForClip, bulletsRemaining);

            // Add the ammo to the current clip
            currentClipAmmo += ammoToLoad;

            // Subtract the ammo we loaded from the remaining bullets
            bulletsRemaining -= ammoToLoad;

            // If there are any remaining bullets, calculate how many full clips are left
            clipsRemaining = bulletsRemaining / ammoPerClip;

            Debug.Log($"Reloaded! Current Clip Ammo: {currentClipAmmo}, Bullets Remaining: {bulletsRemaining}, Clips Remaining: {clipsRemaining}");

            // If ammo is less than required to fill the clip, it will just fill with the remaining reserve ammo
            if (ammoToLoad <= 0)
            {
                Debug.LogWarning("Not enough ammo to reload.");
            }
        }

        // Update the WeaponManager with the new ammo state
        if (gunIndex != -1)
        {
            WeaponManager.instance.SetGunBulletsRemaining(gunIndex, bulletsRemaining);
            WeaponManager.instance.SetGunClipsRemaining(gunIndex, clipsRemaining);
            WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);
        }

        reloadSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        reloadSoundInstance.release();

        UpdateAmmoState();  // Update UI after reloading

        isReloading = false;
    }


    /// <summary>
    /// Stops the reload sound if the reload is interrupted or canceled.
    /// </summary>
    public void StopReloadSound()
    {
        if (reloadSoundInstance.isValid())
        {
            reloadSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); // Stop the reload sound immediately
        }
    }

    /// <summary>
    /// Restores ammo to the current weapon, either with infinite ammo or normal ammo logic.
    /// </summary>
    public void RestoreAmmo()
    {
        // If infinite ammo is not enabled, restore to full clip and adjust reserve ammo
        if (!infiniteAmmo)
        {
            // Fill the current clip to its maximum
            currentClipAmmo = ammoPerClip;

            // Set bullets remaining to the maximum possible minus the ammo in the clip
            bulletsRemaining = maxAmmo - currentClipAmmo;

            // Calculate how many clips can fit into the remaining bullets
            clipsRemaining = bulletsRemaining / ammoPerClip;

            // Clamp bulletsRemaining to ensure it doesn't go negative
            bulletsRemaining = Mathf.Max(bulletsRemaining, 0);

            Debug.Log($"Ammo Restored - CurrentClipAmmo: {currentClipAmmo}, BulletsRemaining: {bulletsRemaining}, ClipsRemaining: {clipsRemaining}");
        }
        else
        {
            // If infinite ammo is enabled, simply reset the clip ammo to full
            currentClipAmmo = ammoPerClip;
        }

        // Update the WeaponManager with the new ammo state
        if (gunIndex != -1)
        {
            WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);
            WeaponManager.instance.SetGunBulletsRemaining(gunIndex, bulletsRemaining);
            WeaponManager.instance.SetGunClipsRemaining(gunIndex, clipsRemaining);
        }

        // Update the UI to reflect the current state
        UpdateAmmoState(); // Ensure UI is updated when ammo is restored
    }

    /// <summary>
    /// Restores ammo from a pickup, a smaller amount.
    /// </summary>
    public void RestoreAmmoFromPickup()
    {
        // Calculate the amount of ammo to restore (1/4 of the max ammo)
        int ammoToRestore = Mathf.FloorToInt(maxAmmo / 4f);

        if (!infiniteAmmo)
        {
            // Add the ammo to the bullets remaining, but cap it at maxAmmo
            bulletsRemaining = Mathf.Min(bulletsRemaining + ammoToRestore, maxAmmo);

            // Update clips remaining based on the updated bullets remaining
            clipsRemaining = bulletsRemaining / ammoPerClip;

            // Ensure that the current clip ammo does not exceed the ammo per clip and doesn't overflow
            currentClipAmmo = Mathf.Min(currentClipAmmo + ammoToRestore, ammoPerClip);

            // If after restoring ammo the clip is still not full, refill it from the reserve bullets
            if (currentClipAmmo < ammoPerClip && bulletsRemaining > 0)
            {
                int ammoNeededForClip = ammoPerClip - currentClipAmmo;
                int ammoToLoad = Mathf.Min(ammoNeededForClip, bulletsRemaining);

                currentClipAmmo += ammoToLoad;
                bulletsRemaining -= ammoToLoad;
            }

            // Ensure that the total ammo does not exceed maxAmmo (reserve + clip)
            bulletsRemaining = Mathf.Min(bulletsRemaining, maxAmmo - currentClipAmmo);

            Debug.Log($"Ammo Restored by Pickup - CurrentClipAmmo: {currentClipAmmo}, BulletsRemaining: {bulletsRemaining}, ClipsRemaining: {clipsRemaining}");
        }
        else
        {
            // If infinite ammo is enabled, no need to change the reserve ammo count
            Debug.Log("Ammo is infinite; no bullets were restored.");
        }

        // Update the WeaponManager with the new ammo state
        if (gunIndex != -1)
        {
            WeaponManager.instance.SetGunBulletsRemaining(gunIndex, bulletsRemaining);
            WeaponManager.instance.SetGunClipsRemaining(gunIndex, clipsRemaining);
            WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);
        }

        // Update the UI to reflect the current state
        UpdateAmmoState(); // Ensure UI is updated when ammo is restored
    }

    /// <summary>
    /// Updates the UI to reflect the current ammo state.
    /// </summary>
    void UpdateAmmoState()
    {
        // Check if the instance is initialized
        if (GunUIManager.instance != null)
        {
            GunUIManager.instance.UpdateUI(); // Only call if instance exists
        }
        else
        {
            Debug.LogWarning("GunUIManager.instance is null! UI will not be updated.");
        }
    }

    /// <summary>
    /// Resets the reloading state when switching weapons.
    /// </summary>
    public void ResetReloadingState()
    {
        StopReloadSound();
        isReloading = false;
    }
}