using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactNade : MonoBehaviour
{
    public float throwForce = 10f;
    public float spinForce = 100f; // Adjust this value to control the spin
    public GameObject explosionEffectPrefab;
    public LayerMask explosionLayers; // Layers to detect collision with

    private Rigidbody2D rb;
    private bool exploded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * throwForce;
        rb.angularVelocity = spinForce;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!exploded && ShouldExplode(collision.gameObject))
        {
            Explode();
        }
    }

    void Explode()
    {
        exploded = true;

        // Instantiate explosion effect
        GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        // Play any explosion sound or particle system
        if (explosionEffect != null)
        {
            ParticleSystem explosionParticleSystem = explosionEffect.GetComponent<ParticleSystem>();
            if (explosionParticleSystem != null)
            {
                explosionParticleSystem.Play();
            }

            AudioSource explosionAudio = explosionEffect.GetComponent<AudioSource>();
            if (explosionAudio != null)
            {
                explosionAudio.Play();
            }
        }

        // Destroy the explosion effect after a delay
        Destroy(explosionEffect, 0.6f);

        // Destroy the grenade itself
        Destroy(gameObject);
    }

    bool ShouldExplode(GameObject other)
    {
        // Check if the collision is with a valid layer
        return (explosionLayers.value & (1 << other.layer)) != 0;
    }
}
