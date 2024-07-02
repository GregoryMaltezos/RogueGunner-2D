using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject projectilePrefab; // The projectile prefab to instantiate
    public Transform firePoint; // The point where the projectile will be spawned
    public float projectileSpeed = 20f; // Speed of the projectile (adjustable in Inspector)
    public bool isShotgun = false; // Indicates whether the gun should fire as a shotgun
    public int shotgunPelletCount = 5; // Number of pellets for shotgun weapons (adjustable in Inspector)
    public float shotgunSpreadAngle = 20f; // Spread angle for shotgun weapons (adjustable in Inspector)
    public bool isAutomatic = false; // Indicates whether the gun should be automatic
    public float fireRate = 0.1f; // Rate of fire for automatic weapons (adjustable in Inspector)
    private bool facingRight = true; // Variable to track character's facing direction
    private float nextFireTime = 0f; // Time until the next shot can be fired

    void Update()
    {
        if (isAutomatic)
        {
            if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Fire(); // Call the Fire method
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Fire(); // Call the Fire method
            }
        }
    }

    void Fire()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 shootDirection = mousePosition - firePoint.position;
        shootDirection.z = 0; // Ensure the direction is 2D
        shootDirection = shootDirection.normalized; // Normalize the shootDirection

        // Determine if the character should be facing right or left based on the mouse position
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
            // Fire a single projectile without spread
            FireProjectile(shootDirection);
        }
        else
        {
            // Fire multiple projectiles with spread for shotgun weapons
            for (int i = 0; i < shotgunPelletCount; i++)
            {
                // Calculate spread angle for each pellet
                float spreadAngle = Random.Range(-shotgunSpreadAngle / 2f, shotgunSpreadAngle / 2f);
                Vector3 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * shootDirection;

                // Fire projectile with spread
                FireProjectile(spreadDirection);
            }
        }
    }

    void FireProjectile(Vector3 shootDirection)
    {
        // Instantiate the projectile
        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = projectileInstance.GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component

        if (rb != null) // Ensure Rigidbody2D component exists
        {
            // Set collision detection mode to continuous for fast-moving projectiles
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Adjust velocity based on player's facing direction
            Vector2 velocity = shootDirection * projectileSpeed;
            rb.velocity = velocity;

            // Flip the projectile's local scale if the player is facing left
            if (!facingRight)
            {
                Vector3 projectileScale = projectileInstance.transform.localScale;
                projectileScale.x = Mathf.Abs(projectileScale.x) * -1; // Ensure X scale is negative
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
        // Flip the character's facing direction
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1
        Vector3 theScale = transform.parent.localScale;
        theScale.x *= -1;
        transform.parent.localScale = theScale;
    }
}
