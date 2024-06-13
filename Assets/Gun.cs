using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject projectilePrefab; // The projectile prefab to instantiate
    public Transform firePoint; // The point where the projectile will be spawned
    public float projectileSpeed = 20f; // Speed of the projectile (adjustable in Inspector)
    public bool isShotgun = false; // Indicates whether the gun should fire as a shotgun
    public int shotgunPelletCount = 5; // Number of pellets for shotgun weapons (adjustable in Inspector)
    public float shotgunSpreadAngle = 20f; // Spread angle for shotgun weapons (adjustable in Inspector)
    private bool facingRight = true; // Variable to track character's facing direction

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Fire(); // Call the Fire method
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
            FireProjectile(shootDirection, Quaternion.identity);
        }
        else
        {
            // Fire multiple projectiles with spread for shotgun weapons
            for (int i = 0; i < shotgunPelletCount; i++)
            {
                // Calculate spread angle for each pellet
                float spreadAngle = Random.Range(-shotgunSpreadAngle, shotgunSpreadAngle);
                Quaternion spreadRotation = Quaternion.Euler(0f, 0f, spreadAngle);

                // Fire projectile with spread
                FireProjectile(shootDirection, spreadRotation);
            }
        }
    }

    void FireProjectile(Vector3 shootDirection, Quaternion spreadRotation)
    {
        // Instantiate the projectile
        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation * spreadRotation);
        Rigidbody2D rb = projectileInstance.GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component

        if (rb != null) // Ensure Rigidbody2D component exists
        {
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
