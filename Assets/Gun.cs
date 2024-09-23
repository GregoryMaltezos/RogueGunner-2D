using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 20f;
    public bool isShotgun = false;
    public int shotgunPelletCount = 5;
    public float shotgunSpreadAngle = 20f;
    public bool isAutomatic = false;
    public float fireRate = 0.1f;
    public int maxAmmo = 90;
    public int ammoPerClip = 30;
    public float reloadTime = 2f;
    public bool infiniteAmmo = false;

    [HideInInspector] public int currentClipAmmo;
    [HideInInspector] public int bulletsRemaining;
    [HideInInspector] public int clipsRemaining;

    private bool isReloading = false;
    private float nextFireTime = 0f;
    private bool facingRight = true;
    private int gunIndex;

    void Start()
    {
        gunIndex = WeaponManager.instance.GetGunIndex(this.gameObject);
        if (gunIndex != -1)
        {
            bulletsRemaining = WeaponManager.instance.GetGunBulletsRemaining(gunIndex);
            clipsRemaining = WeaponManager.instance.GetGunClipsRemaining(gunIndex);
            currentClipAmmo = WeaponManager.instance.GetGunClipAmmo(gunIndex);

            if (currentClipAmmo <= 0 && !infiniteAmmo)
            {
                currentClipAmmo = ammoPerClip;
                WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);
            }

            Debug.Log($"Gun Initialized: GunIndex={gunIndex}, CurrentClipAmmo={currentClipAmmo}, BulletsRemaining={bulletsRemaining}, ClipsRemaining={clipsRemaining}");
        }
        else
        {
            Debug.LogWarning("Gun index not found in WeaponManager");
        }
    }

    void Update()
    {
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
                if (infiniteAmmo || currentClipAmmo > 0)
                {
                    nextFireTime = Time.time + fireRate;
                    Fire();
                }
                else if (clipsRemaining > 0)
                {
                    StartCoroutine(Reload());
                }
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (infiniteAmmo || currentClipAmmo > 0)
                {
                    Fire();
                }
                else if (clipsRemaining > 0)
                {
                    StartCoroutine(Reload());
                }
            }
        }
    }

    void Fire()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 shootDirection = mousePosition - firePoint.position;
        shootDirection.z = 0;
        shootDirection = shootDirection.normalized;

        if (shootDirection.x > 0 && !facingRight)
        {
            FlipCharacter();
        }
        else if (shootDirection.x < 0 && facingRight)
        {
            FlipCharacter();
        }

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

        if (!infiniteAmmo)
        {
            currentClipAmmo--;
            Debug.Log($"Fired! Current Clip Ammo: {currentClipAmmo}");

            if (currentClipAmmo <= 0 && clipsRemaining > 0)
            {
                StartCoroutine(Reload());
            }
        }
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

            if (!facingRight)
            {
                Vector3 projectileScale = projectileInstance.transform.localScale;
                projectileScale.x = Mathf.Abs(projectileScale.x) * -1;
                projectileInstance.transform.localScale = projectileScale;
            }
        }
        else
        {
            Debug.LogError("Rigidbody2D component not found on the projectile prefab!");
        }
    }

    void FlipCharacter()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.parent.localScale;
        theScale.x *= -1;
        transform.parent.localScale = theScale;
    }

    IEnumerator Reload()
    {
        if (isReloading) yield break;

        isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);

        if (infiniteAmmo)
        {
            currentClipAmmo = ammoPerClip;
            Debug.Log($"Reloaded! Current Clip Ammo: {currentClipAmmo}");
        }
        else
        {
            if (clipsRemaining > 0)
            {
                int ammoToLoad = Mathf.Min(ammoPerClip, bulletsRemaining);
                currentClipAmmo = ammoToLoad;
                bulletsRemaining -= ammoToLoad;
                clipsRemaining = bulletsRemaining / ammoPerClip;
                Debug.Log($"Reloaded! Current Clip Ammo: {currentClipAmmo}, Bullets Remaining: {bulletsRemaining}, Clips Remaining: {clipsRemaining}");
            }
            else
            {
                Debug.Log("No clips remaining to reload.");
            }
        }

        if (gunIndex != -1)
        {
            WeaponManager.instance.SetGunBulletsRemaining(gunIndex, bulletsRemaining);
            WeaponManager.instance.SetGunClipsRemaining(gunIndex, clipsRemaining);
            WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);
        }

        isReloading = false;
    }

    void OnDisable()
    {
        // Store ammo when the gun is disabled
        if (gunIndex != -1)
        {
            WeaponManager.instance.SetGunBulletsRemaining(gunIndex, bulletsRemaining);
            WeaponManager.instance.SetGunClipsRemaining(gunIndex, clipsRemaining);
            WeaponManager.instance.SetGunClipAmmo(gunIndex, currentClipAmmo);
        }
    }

    public void RestoreAmmo()
    {
        if (!infiniteAmmo)
        {
            // Set the current clip ammo to the maximum per clip
            currentClipAmmo = ammoPerClip;

            // Calculate the total bullets remaining correctly
            // Assuming bulletsRemaining should not be recalculated like this:
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
    }


    // This method can be called when switching to this weapon to reset states
    public void ResetReloadingState()
    {
        isReloading = false;
    }
}
