using UnityEngine;
using System.Collections;

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

    [HideInInspector] public int currentClipAmmo; // Current ammo in the clip
    [HideInInspector] public int bulletsRemaining; // Total bullets remaining
    [HideInInspector] public int clipsRemaining; // Total clips remaining

    private bool isReloading = false; // Is the gun currently reloading?
    private float nextFireTime = 0f; // Time when the gun can fire again
    private bool facingRight = true; // Is the character facing right?
    private int gunIndex; // Index of the gun in the weapon manager
    private int shotsFired; // Counter for shots fired when infinite ammo is enabled
    private const int maxShotsWithInfiniteAmmo = 12; // Max bullets before needing to reload

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

            // Ensure we have ammo to start
            if (currentClipAmmo <= 0 || infiniteAmmo)
            {
                currentClipAmmo = ammoPerClip;
            }
            WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);

          //  Debug.Log($"Gun Initialized: GunIndex={gunIndex}, CurrentClipAmmo={currentClipAmmo}, BulletsRemaining={bulletsRemaining}, ClipsRemaining={clipsRemaining}");
            UpdateAmmoState(); // Initialize the UI state
        }
        else
        {
            Debug.LogWarning("Gun index not found in WeaponManager");
        }
    }

    void Update()
    {
        // Check if the game is paused; if so, skip any firing logic
        if (FindObjectOfType<PauseMenu>().IsPaused) // Check pause state
            return;

        if (isReloading)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (clipsRemaining > 0 || infiniteAmmo)
            {
                StartCoroutine(Reload());
            }
        }

        if (isAutomatic)
        {
            if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
            {
                AttemptToFire();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
            {
                AttemptToFire();
            }
        }
    }


    void AttemptToFire()
    {
        // Check if the player is currently attacking
        if (PlayerController.instance != null && PlayerController.instance.isAttacking)
        {
            // If attacking, do not fire
            return;
        }

        if (infiniteAmmo || currentClipAmmo > 0)
        {
            Fire();
            nextFireTime = Time.time + fireRate; // Set the next fire time
            UpdateAmmoState(); // Update UI after firing
        }
        else if (clipsRemaining > 0)
        {
            StartCoroutine(Reload());
        }
    }


    void Fire()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 shootDirection = mousePosition - firePoint.position;
        shootDirection.z = 0;
        shootDirection = shootDirection.normalized;

        // Determine if the gun should flip based on shoot direction


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
            // Decrement ammo for normal guns
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
            // Infinite ammo handling
            if (currentClipAmmo > 0)
            {
                // Decrement for display purposes
                currentClipAmmo--; // Decrement the visual clip ammo
                shotsFired++; // Increase shots fired count
                              //  Debug.Log($"Fired! Current Clip Ammo: {currentClipAmmo}");

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

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Vector2 velocity = shootDirection * projectileSpeed;
            rb.velocity = velocity;

            // Flip projectile based on facing direction
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


    IEnumerator Reload()
    {
        if (isReloading) yield break;

        isReloading = true;
        Debug.Log("Reloading...");

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

        UpdateAmmoState(); // Update UI after reloading
        isReloading = false;
    }

    public void RestoreAmmo()
    {
        if (!infiniteAmmo)
        {
            // Set the current clip ammo to the maximum per clip
            currentClipAmmo = ammoPerClip;

            // Calculate the total bullets remaining correctly
            bulletsRemaining = (clipsRemaining * ammoPerClip) + bulletsRemaining - ammoPerClip; // Adjusting for the clip we just set

            // Update clips remaining based on the new bullets remaining
            clipsRemaining = Mathf.Clamp((maxAmmo - bulletsRemaining) / ammoPerClip, 0, maxAmmo / ammoPerClip); // Clamp to ensure it doesn't go negative
            Debug.Log($"Ammo Restored - CurrentClipAmmo: {currentClipAmmo}, BulletsRemaining: {bulletsRemaining}, ClipsRemaining: {clipsRemaining}");
        }
        else
        {
            currentClipAmmo = ammoPerClip;
        }

        if (gunIndex != -1)
        {
            WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);
            WeaponManager.instance.SetGunBulletsRemaining(gunIndex, bulletsRemaining);
            WeaponManager.instance.SetGunClipsRemaining(gunIndex, clipsRemaining);
        }

        UpdateAmmoState(); // Ensure UI is updated when ammo is restored
    }

    public void UpdateAmmoState()
    {
        if (GunUIManager.instance != null)
        {
            GunUIManager.instance.UpdateUI();
        }
    }

    // This method can be called when switching to this weapon to reset states
    public void ResetReloadingState()
    {
        isReloading = false;
    }
}