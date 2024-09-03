using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    private List<SteeringBehaviour> steeringBehaviours;

    [SerializeField]
    private List<Detector> detectors;

    [SerializeField]
    private AIData aiData;

    [SerializeField]
    private float detectionDelay = 0.05f, aiUpdateDelay = 0.06f, attackDelay = 1f;

    [SerializeField]
    private float attackDistance = 0.5f;

    // Inputs sent from the Enemy AI to the Enemy controller
    public UnityEvent OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;

    [SerializeField]
    private Vector2 movementInput;

    [SerializeField]
    private ContextSolver movementDirectionSolver;

    bool following = false;

    private void Start()
    {
        // Detecting Player and Obstacles around
        InvokeRepeating("PerformDetection", 0, detectionDelay);
    }

    private void PerformDetection()
    {
        foreach (Detector detector in detectors)
        {
            detector.Detect(aiData);
        }
    }

    private void Update()
    {
        if (aiData.currentTarget != null)
        {
            // Calculate and adjust the target position to aim slightly below the player's collider center
            BoxCollider2D playerCollider = aiData.currentTarget.GetComponent<BoxCollider2D>();
            if (playerCollider != null)
            {
                // Calculate the center of the player's collider and apply the offset
                Vector3 targetCenter = aiData.currentTarget.position;
                targetCenter.y -= (playerCollider.size.y / 2) + 1.5f; // Adjust this value to set the desired offset

                OnPointerInput?.Invoke(targetCenter);
            }
            else
            {
                // Fallback to the default position if the collider is not found
                OnPointerInput?.Invoke(aiData.currentTarget.position);
            }

            if (!following)
            {
                following = true;
                StartCoroutine(ChaseAndAttack());
            }
        }
        else if (aiData.GetTargetsCount() > 0)
        {
            // Target acquisition logic
            aiData.currentTarget = aiData.targets[0];
        }

        // Moving the Agent
        OnMovementInput?.Invoke(movementInput);
    }

    private IEnumerator ChaseAndAttack()
    {
        if (aiData.currentTarget == null)
        {
            // Stopping Logic
            Debug.Log("Stopping");
            movementInput = Vector2.zero;
            following = false;
            yield break;
        }
        else
        {
            float distance = Vector2.Distance(aiData.currentTarget.position, transform.position);

            if (distance < attackDistance)
            {
                // Attack logic
                movementInput = Vector2.zero;
                OnAttackPressed?.Invoke();
                yield return new WaitForSeconds(attackDelay);
                StartCoroutine(ChaseAndAttack());
            }
            else
            {
                // Chase logic
                movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                yield return new WaitForSeconds(aiUpdateDelay);
                StartCoroutine(ChaseAndAttack());
            }
        }
    }

    // For debugging: visualize the target aiming in the Scene view
    void OnDrawGizmos()
    {
        if (aiData.currentTarget != null)
        {
            BoxCollider2D playerCollider = aiData.currentTarget.GetComponent<BoxCollider2D>();
            if (playerCollider != null)
            {
                Vector3 targetCenter = aiData.currentTarget.position - new Vector3(0, playerCollider.size.y / 2 + 1.5f, 0);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, targetCenter);
            }
        }
    }
}
