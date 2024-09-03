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

    private void Update()
    {
        // pointerInput = GetPointerInput();
        // movementInput = movement.action.ReadValue<Vector2>().normalized;

        agentMover.MovementInput = MovementInput;

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
    }

    private void AnimateCharacter()
    {
        Vector2 lookDirection = pointerInput - (Vector2)transform.position;
        agentAnimations.RotateToPointer(lookDirection);
        agentAnimations.PlayAnimation(movementInput); // This will set the "Run" trigger based on movement input
    }
}
