using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CthuluProj : MonoBehaviour
{
    public float lifetime = 7f; // Time before the projectile is destroyed

    private void Start()
    {
        Destroy(gameObject, lifetime); // Destroy the projectile after its lifetime
    }

    private void Update()
    {
        // Optional: Check if the projectile is off-screen and destroy it
        if (transform.position.y < -11f) // Adjust based on your game view
        {
            Destroy(gameObject);
        }
    }
}