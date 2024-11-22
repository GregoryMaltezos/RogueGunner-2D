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

    [SerializeField] private EventReference gunFired;
    [SerializeField] private EventReference gunReload;
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
            bulletsRemaining = WeaponManager.instance.GetGunBulletsRemaining(gunIndex);
            clipsRemaining = WeaponManager.instance.GetGunClipsRemaining(gunIndex);
            currentClipAmmo = WeaponManager.instance.GetGunClipAmmo(gunIndex);

            if (currentClipAmmo <= 0 || infiniteAmmo)
            {
                currentClipAmmo = ammoPerClip;
            }
            WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);

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

    void OnDestroy()
    {
        reloadAction.performed -= OnReloadPerformed;
        fireAction.performed -= OnFirePerformed;
        fireAction.canceled -= OnFireCanceled; // Unsubscribe from stopping fire
        inputActions.Disable();
    }


    private void OnFirePerformed(InputAction.CallbackContext context)
    {
        isFiring = true;

        // Start firing immediately for non-automatic guns
        if (!isAutomatic && Time.time >= nextFireTime)
        {
            AttemptToFire();
        }
    }

    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        isFiring = false; // Stop firing when input is released
    }


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

    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        // Trigger reload if the action is performed
        if (clipsRemaining > 0 || infiniteAmmo)
        {
            StartCoroutine(Reload());
        }
    }
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



    void Fire()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 shootDirection = mousePosition - firePoint.position;
        shootDirection.z = 0;
        shootDirection = shootDirection.normalized;

        // Fire the projectile(s)
        if (!isShotgun)
        {
            FireProjectile(shootDirection);
        }
        else
        {
            for (int i = 0; i < shotgunPelletCount; i++)
            {
                float spreadAngle = Random.Range(-shotgunSpreadAngle / 2f, shotgunSpreadAngle / 2f);
                Vector3 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * shootDirection;
                FireProjectile(spreadDirection);
            }
        }

        // Handle ammo decrementing logic
        if (!infiniteAmmo)
        {
            if (currentClipAmmo > 0)
            {
                currentClipAmmo--; // Decrement once per shot
                Debug.Log($"Fired! Current Clip Ammo: {currentClipAmmo}");

                // Update the WeaponManager with the new ammo state
                WeaponManager.instance.SetGunClipAmmo(WeaponManager.instance.GetCurrentGunIndex(), currentClipAmmo);
            }

            // Check if we need to reload
            if (currentClipAmmo <= 0 && clipsRemaining > 0)
            {
                StartCoroutine(Reload());
            }
        }
        else
        {
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

    void FireProjectile(Vector3 shootDirection)
    {
        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = projectileInstance.GetComponent<Rigidbody2D>();
        AudioManager.instance.PlayOneShot(gunFired, this.transform.position);
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Vector2 velocity = shootDirection * projectileSpeed;
            rb.velocity = velocity;

            // Flip projectile based on the player's facing direction
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

    private FMOD.Studio.EventInstance reloadSoundInstance;
    IEnumerator Reload()
    {
        if (isReloading) yield break;

        isReloading = true;
        Debug.Log("Reloading...");
        reloadSoundInstance = AudioManager.instance.PlayOneShot(gunReload, this.transform.position);
        yield return new WaitForSeconds(reloadTime);

        if (infiniteAmmo)
        {
            currentClipAmmo = ammoPerClip; // Reset currentClipAmmo to the maximum per clip
            Debug.Log($"Reloaded! Current Clip Ammo: {currentClipAmmo}");
            shotsFired = 0; // Reset shots fired count
        }
        else
        {
            if (clipsRemaining > 0)
            {
                // Save the current clip ammo before reloading
                int leftoverAmmo = currentClipAmmo;

                // Determine how much ammo to load into the clip
                int ammoToLoad = Mathf.Min(ammoPerClip, bulletsRemaining + leftoverAmmo);
                currentClipAmmo = ammoToLoad;

                // Calculate new bullets remaining
                if (leftoverAmmo > 0)
                {
                    // Only add the leftover ammo if it's greater than zero
                    bulletsRemaining += leftoverAmmo;
                }

                bulletsRemaining -= ammoToLoad; // Subtract loaded ammo from total
                clipsRemaining = bulletsRemaining / ammoPerClip; // Update clips remaining
                Debug.Log($"Reloaded! Current Clip Ammo: {currentClipAmmo}, Bullets Remaining: {bulletsRemaining}, Clips Remaining: {clipsRemaining}");
            }
            else
            {
                Debug.Log("No clips remaining to reload.");
            }
        }

        // Update weapon manager as before...
        if (gunIndex != -1)
        {
            WeaponManager.instance.SetGunBulletsRemaining(gunIndex, bulletsRemaining);
            WeaponManager.instance.SetGunClipsRemaining(gunIndex, clipsRemaining);
            WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);
        }
        reloadSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        reloadSoundInstance.release();
        UpdateAmmoState(); // Update UI after reloading
        isReloading = false;
    }
    public void StopReloadSound()
    {
        if (reloadSoundInstance.isValid())
        {
            reloadSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); // Stop the reload sound immediately
        }
    }
    public void RestoreAmmo()
    {
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

    public void RestoreAmmoFromPickup()
    {
        // Calculate the amount of ammo to restore (1/4 of the max ammo)
        int ammoToRestore = Mathf.FloorToInt(maxAmmo / 4f);

        if (!infiniteAmmo)
        {
            // Restore the ammo to bullets remaining, ensuring it does not exceed the maximum ammo
            bulletsRemaining = Mathf.Min(bulletsRemaining + ammoToRestore, maxAmmo);

            // Update clips remaining based on updated bullets remaining
            clipsRemaining = bulletsRemaining / ammoPerClip;

            Debug.Log($"Ammo Restored by Pickup - BulletsRemaining: {bulletsRemaining}, ClipsRemaining: {clipsRemaining}");
        }
        else
        {
            // If infinite ammo is enabled, no need to change bullets remaining
            Debug.Log("Ammo is infinite; no bullets were restored.");
        }

        // Update the WeaponManager with the new ammo state
        if (gunIndex != -1)
        {
            WeaponManager.instance.SetGunBulletsRemaining(gunIndex, bulletsRemaining);
            WeaponManager.instance.SetGunClipsRemaining(gunIndex, clipsRemaining);
        }

        // Update the UI to reflect the current state
        UpdateAmmoState(); // Ensure UI is updated when ammo is restored
    }

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

    // This method can be called when switching to this weapon to reset states
    public void ResetReloadingState()
    {
        StopReloadSound();
        isReloading = false;
    }
}