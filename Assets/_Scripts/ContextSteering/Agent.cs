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

    private void Update()
    {
        // Null-checks to avoid errors if components are missing
        if (agentMover != null)
        {
            agentMover.MovementInput = MovementInput;
        }

        if (weaponParent != null)
        {
            weaponParent.PointerPosition = pointerInput;
        }
        else if (bodyAttack != null)
        {
            bodyAttack.PointerPosition = pointerInput;
        }

        AnimateCharacter();
    }

    public void PerformAttack()
    {
        if (weaponParent != null)
        {
            weaponParent.Attack();
        }
        else if (bodyAttack != null)
        {
            bodyAttack.Attack();
        }
    }

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

    private void AnimateCharacter()
    {
        if (agentAnimations == null) return;

        Vector2 lookDirection = pointerInput - (Vector2)transform.position;

        // Reverse the look direction if reverseFlipping is true
        if (reverseFlipping)
        {
            lookDirection = new Vector2(-lookDirection.x, lookDirection.y);
        }

        agentAnimations.RotateToPointer(lookDirection);
        agentAnimations.PlayAnimation(movementInput);
    }
}
