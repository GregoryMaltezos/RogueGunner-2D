using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentAnimations : MonoBehaviour
{
    private Animator animator;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void RotateToPointer(Vector2 lookDirection)
    {
        if (isDead) return; // Prevent rotation when dead

        Vector3 scale = transform.localScale;
        if (lookDirection.x > 0)
        {
            scale.x = 1;
        }
        else if (lookDirection.x < 0)
        {
            scale.x = -1;
        }
        transform.localScale = scale;
    }

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
        }
    }

    public void TriggerDeathAnimation()
    {
        if (isDead) return; // Prevent re-triggering death animation

        isDead = true; // Set the dead flag
        animator.SetTrigger("Die"); // Trigger the death animation
    }

    public void TriggerHitAnimation()
    {
        if (isDead) return; // Prevent hit animation when dead

        animator.SetTrigger("Hit");
    }
}
