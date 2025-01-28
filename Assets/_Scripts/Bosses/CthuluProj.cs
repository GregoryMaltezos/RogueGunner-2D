using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CthuluProj : MonoBehaviour
{
    public float lifetime = 7f; // Time before the projectile is destroyed

    /// <summary>
    /// Starts a timer to destroy the projectile after its lifetime expires.
    /// </summary>
    private void Start()
    {
        Destroy(gameObject, lifetime); // Destroy the projectile after its lifetime
    }

    private void Update()
    {
        //Check if the projectile is off-screen and destroy it
        if (transform.position.y < -11f) // Adjust based on your game view
        {
            Destroy(gameObject);
        }
    }
}