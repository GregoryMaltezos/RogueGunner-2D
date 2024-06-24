using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float throwForce = 10f;
    public float explosionDelay = 4f;
    public float spinForce = 100f; // Adjust this value to control the spin
    public GameObject explosionEffectPrefab;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * throwForce; // Initial velocity in the direction it's facing
        rb.angularVelocity = spinForce; // Apply spin force
        Invoke("Explode", explosionDelay);
    }

    void Explode()
    {
        GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        Destroy(explosionEffect, 0.6f); // Destroy the explosion effect after 2 seconds (adjust as needed)

        Destroy(gameObject); // Destroy the grenade
    }
}
