using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentAnimations : MonoBehaviour
{
    private Animator animator;
    private bool isDead = false;


    /// <summary>
    /// Initializes the Animator reference.
    /// </summary>
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Rotates the agent to face the direction of the pointer or look direction.
    /// Prevents rotation when the agent is dead.
    /// </summary>
    /// <param name="lookDirection">The direction to look at.</param>
    public void RotateToPointer(Vector2 lookDirection)
    {
        if (isDead) return; // Prevent rotation when dead

        Vector3 scale = transform.localScale; // Get the current scale of the agent
        if (lookDirection.x > 0) // Check the x direction of the look direction and flip the scale accordingly
        {
            scale.x = 1; // Face right
        }
        else if (lookDirection.x < 0) 
        {
            scale.x = -1; // Face left
        }
        transform.localScale = scale; // Apply the new scale to the agent
    }


    /// <summary>
    /// Plays the movement animation based on the movement input.
    /// Triggers the "Run" animation if movement is detected, otherwise triggers the "Idle" animation.
    /// </summary>
    /// <param name="movementInput">The movement input vector (e.g., from player input).</param>
    public void PlayAnimation(Vector2 movementInput)
    {
        if (isDead) return; // Prevent animation when dead

        // Set the "Run" trigger if movement is detected
        if (movementInput.magnitude > 0)
        {
            animator.SetTrigger("Run");
        }
        else
        {
            // Reset the "Run" trigger if not moving
            animator.ResetTrigger("Run");
            // Trigger idle animation
            animator.SetTrigger("Idle"); // Transition to idle
        }
    }

    /// <summary>
    /// Triggers the death animation and prevents retriggering once the agent is dead.
    /// </summary>
    public void TriggerDeathAnimation()
    {
        if (isDead) return; // Prevent re-triggering death animation

        isDead = true; // Set the dead flag
        animator.SetTrigger("Die"); // Trigger the death animation
    }

    /// <summary>
    /// Triggers the hit animation when the agent is hit and prevents retriggering once the agent is dead.
    /// transitions to idle after the hit animation finishes.
    /// </summary>
    public void TriggerHitAnimation()
    {
        if (isDead) return; // Prevent hit animation when dead

        animator.SetTrigger("Hit");

        // wait for the hit animation duration before transitioning to idle
        StartCoroutine(TransitionToIdleAfterHit());
    }

    /// <summary>
    /// Coroutine to transition the agent to idle after the hit animation.
    /// Waits for the hit animation to play before transitioning to idle.
    /// </summary>
    private IEnumerator TransitionToIdleAfterHit()
    {
        // Assuming the hit animation duration is set to 0.5 seconds in the Animator
        yield return new WaitForSeconds(0.2f); // Wait for the duration of the hit animation
        animator.SetTrigger("IdleD"); // Transition to idle
    }
}
