using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;  // Required for accessing playback state

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

    [SerializeField]
    private float chaseRadius = 10f;  // New chase radius

    // Inputs sent from the Enemy AI to the Enemy controller
    public UnityEvent OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;

    [SerializeField]
    private Vector2 movementInput;

    [SerializeField]
    private ContextSolver movementDirectionSolver;

    bool following = false;

    [SerializeField]
    private EventReference movementSoundEvent; // FMOD Event Reference (assign in inspector)

    private FMOD.Studio.EventInstance movementSoundInstance;

    private void Start()
    {
        // Detecting Player and Obstacles around
        InvokeRepeating("PerformDetection", 0, detectionDelay);
        movementSoundInstance = RuntimeManager.CreateInstance(movementSoundEvent);
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
            // Calculate the distance between the enemy and the player (target)
            float distanceToPlayer = Vector2.Distance(aiData.currentTarget.position, transform.position);

            // Check if the target is within the chase radius
            if (distanceToPlayer > chaseRadius)  // Player is out of chase radius
            {
                StopChasing(); // Stop chasing if the player is out of range
            }
            else
            {
                // If we're within chase radius, ensure the enemy starts or continues chasing
                if (!following)
                {
                    following = true;
                    StartCoroutine(ChaseAndAttack());  // Start chasing if it's not already chasing
                    PlayMovementSound(); // Play the movement sound when the enemy starts chasing
                }

                // Calculate and adjust the target position to aim slightly below the player's collider center
                BoxCollider2D playerCollider = aiData.currentTarget.GetComponent<BoxCollider2D>();
                if (playerCollider != null)
                {
                    Vector3 targetCenter = aiData.currentTarget.position;
                    targetCenter.y -= (playerCollider.size.y / 2) + 1.5f; // Adjust this value to set the desired offset

                    OnPointerInput?.Invoke(targetCenter);
                }
                else
                {
                    OnPointerInput?.Invoke(aiData.currentTarget.position);
                }
            }
        }
        else
        {
            // If there is no current target and we have a list of targets, pick the first one
            if (aiData.GetTargetsCount() > 0)
            {
                aiData.currentTarget = aiData.targets[0];
            }
            else
            {
                // If no targets are found, stop chasing
                StopChasing();
            }
        }

        // Moving the Agent
        OnMovementInput?.Invoke(movementInput);

        // Control sound based on whether the agent is moving
        AgentMover agentMover = GetComponent<AgentMover>();
        if (agentMover != null)
        {
            if (agentMover.IsMoving())  // Check if the agent is actually moving
            {
                // Play sound if it's not already playing
                PlayMovementSound();
            }
            else
            {
                // Stop sound if it's playing
                StopMovementSound();
            }
        }
    }

    // Stop chasing and halt movement
    private void StopChasing()
    {
        if (following)
        {
            following = false; // Set the flag to false to stop chasing
            movementInput = Vector2.zero; // Stop movement when the player is out of range
            StopMovementSound(); // Stop sound when no longer chasing
        }
    }

    // Play the movement sound
    private void PlayMovementSound()
    {
        if (!IsSoundPlaying()) // Check if the sound isn't already playing
        {
            movementSoundInstance.start();  // Start the movement sound
        }
    }

    // Stop the movement sound
    private void StopMovementSound()
    {
        if (IsSoundPlaying())  // Stop the sound if it is currently playing
        {
            movementSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    private bool IsSoundPlaying()
    {
        FMOD.Studio.PLAYBACK_STATE playbackState;
        movementSoundInstance.getPlaybackState(out playbackState);
        return playbackState == FMOD.Studio.PLAYBACK_STATE.PLAYING;
    }

    private void OnDestroy()
    {
        // Stop the FMOD event when the object is destroyed
        if (IsSoundPlaying())
        {
            movementSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            movementSoundInstance.release();
        }
    }

    private IEnumerator ChaseAndAttack()
    {
        if (aiData.currentTarget == null)
        {
            // Stopping Logic
            Debug.Log("Stopping");
            movementInput = Vector2.zero;
            following = false;
            if (IsSoundPlaying())
            {
                movementSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
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
