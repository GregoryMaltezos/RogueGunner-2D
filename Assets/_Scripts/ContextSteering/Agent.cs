using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private AgentAnimations agentAnimations;
    private AgentMover agentMover;

    private WeaponParent weaponParent;
    private EnemyBodyAttack bodyAttack;

    private Vector2 pointerInput, movementInput;

    public Vector2 PointerInput { get => pointerInput; set => pointerInput = value; }
    public Vector2 MovementInput { get => movementInput; set => movementInput = value; }

    [SerializeField]
    private bool reverseFlipping = false; // Variable to control flipping logic


    /// <summary>
    /// Handles input assignment to movement and pointer, and triggers character animation.
    /// </summary>
    private void Update()
    {
        // Null-checks to avoid errors if components are missing
        if (agentMover != null)
        {
            agentMover.MovementInput = MovementInput;  // Assign movement input to the AgentMover for movement handling
        }

        if (weaponParent != null)
        {
            weaponParent.PointerPosition = pointerInput; // Assign pointer input to WeaponParent to control aiming or shooting
        }
        else if (bodyAttack != null)
        {
            bodyAttack.PointerPosition = pointerInput; // Assign pointer input to EnemyBodyAttack if no weapon is present
        }

        AnimateCharacter(); // Trigger character animation based on movement and pointer input
    }

    /// <summary>
    /// Performs an attack based on the available weapon or body attack system.
    /// </summary>
    public void PerformAttack()
    {
        if (weaponParent != null)  // If a weapon is available, trigger its attack method
        {
            weaponParent.Attack();
        }
        else if (bodyAttack != null) // If no weapon is available, trigger the body attack method
        {
            bodyAttack.Attack();
        }
    }

    /// <summary>
    /// Initializes references to other components and performs null-checks.
    /// Logs warnings if required components are missing from the GameObject.
    /// </summary>
    private void Awake()
    {
        agentAnimations = GetComponentInChildren<AgentAnimations>();
        weaponParent = GetComponentInChildren<WeaponParent>();
        bodyAttack = GetComponentInChildren<EnemyBodyAttack>();
        agentMover = GetComponent<AgentMover>();

        // Debugging help: Log warnings if components are missing
        if (agentMover == null) Debug.LogWarning("AgentMover not assigned or missing!");
        if (weaponParent == null && bodyAttack == null) Debug.LogWarning("WeaponParent and EnemyBodyAttack are both missing!");
    }


    /// <summary>
    /// Animates the agent based on the movement and pointer inputs.
    /// It also flips the character if necessary depending on the `reverseFlipping` flag.
    /// </summary>
    private void AnimateCharacter()
    {
        if (agentAnimations == null) return; // If there are no animations to handle, exit the function

        Vector2 lookDirection = pointerInput - (Vector2)transform.position; // Calculate the direction the agent should face (from agent position to pointer input)

        // Reverse the look direction if reverseFlipping is true
        if (reverseFlipping)
        {
            lookDirection = new Vector2(-lookDirection.x, lookDirection.y);
        }
        // Rotate the character to face the pointer direction and play the movement animation
        agentAnimations.RotateToPointer(lookDirection);
        agentAnimations.PlayAnimation(movementInput);
    }
}
