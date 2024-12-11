using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;

public class EnemyAI : MonoBehaviour
{
    [SerializeField]
    private List<SteeringBehaviour> steeringBehaviours; // List of steering behaviours used by the AI

    [SerializeField]
    private List<Detector> detectors; // List of detectors to detect targets

    [SerializeField]
    private AIData aiData;  // AI data that contains target and detection information

    [SerializeField]
    private float detectionDelay = 0.05f, aiUpdateDelay = 0.06f, attackDelay = 1f;

    [SerializeField]
    private float attackDistance = 0.5f;

    [SerializeField]
    private float chaseRadius = 10f;

    [Header("Music Change Settings")]
    [SerializeField] private MusicType musicType;

    public UnityEvent OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;

    [SerializeField]
    private Vector2 movementInput;

    [SerializeField]
    private ContextSolver movementDirectionSolver;

    bool following = false;

    [SerializeField]
    private EventReference movementSoundEvent;

    private FMOD.Studio.EventInstance movementSoundInstance;

    private Health healthScript; // Reference to the health script

    /// <summary>
    /// Initializes detection, movement sound, health events, and enemy registration.
    /// </summary>
    private void Start()
    {
        // Perform detection at regular intervals
        InvokeRepeating("PerformDetection", 0, detectionDelay);
        movementSoundInstance = RuntimeManager.CreateInstance(movementSoundEvent);

        // Get the Health script and attach the death event
        healthScript = GetComponent<Health>();
        if (healthScript != null)
        {
            healthScript.OnDeathWithReference.AddListener(OnEnemyDeath);
        }

        // Register the enemy with the manager
        EnemyManager.instance?.RegisterEnemy(this);
    }

    /// <summary>
    /// Periodically performs detection using all detectors.
    /// </summary>
    private void PerformDetection()
    {
        foreach (Detector detector in detectors)
        {
            detector.Detect(aiData);
        }
    }

    /// <summary>
    /// Handles target detection, movement, sound, and attack logic.
    /// </summary>
    private void Update()
    {
        if (healthScript != null && healthScript.isDead)
        {
            // If the enemy is already dead, stop further updates
            return;
        }

        if (aiData.currentTarget != null)
        {
            float distanceToPlayer = Vector2.Distance(aiData.currentTarget.position, transform.position); // Calculate the distance to the target

            if (distanceToPlayer > chaseRadius) // Player is out of chase radius
            {
                StopChasing();
            }
            else
            {
                // Start chasing if not already following
                if (!following)
                {
                    following = true;
                    StartCoroutine(ChaseAndAttack());
                    PlayMovementSound();
                    ChangeMusic();
                }
                // Update the pointer input for targeting the player
                BoxCollider2D playerCollider = aiData.currentTarget.GetComponent<BoxCollider2D>();
                if (playerCollider != null)
                {
                    Vector3 targetCenter = aiData.currentTarget.position;
                    targetCenter.y -= (playerCollider.size.y / 2) + 1.5f;

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
            if (aiData.GetTargetsCount() > 0)
            {
                aiData.currentTarget = aiData.targets[0];
            }
            else
            {
                StopChasing();
            }
        }

        OnMovementInput?.Invoke(movementInput);
        // Handle movement and sound playing based on movement state
        AgentMover agentMover = GetComponent<AgentMover>();
        if (agentMover != null)
        {
            if (agentMover.IsMoving())
            {
                PlayMovementSound();
            }
            else
            {
                StopMovementSound();
            }
        }
    }

    /// <summary>
    /// Stops chasing the player and resets movement state.
    /// </summary>
    private void StopChasing()
    {
        if (following)
        {
            following = false; // Set the flag to false to stop chasing
            movementInput = Vector2.zero; // Stop movement when the player is out of range
            StopMovementSound(); // Stop sound when no longer chasing
            ChangeMusic(); // Notify the manager
        }
    }
    /// <summary>
    /// Starts playing the movement sound if it's not already playing.
    /// </summary>
    private void PlayMovementSound()
    {
        if (!IsSoundPlaying())
        {
            movementSoundInstance.start();
        }
    }
    /// <summary>
    /// Stops the movement sound if it's currently playing.
    /// </summary>
    private void StopMovementSound()
    {
        if (IsSoundPlaying())
        {
            movementSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
    /// <summary>
    /// Checks whether the movement sound is currently playing.
    /// </summary>
    /// <returns>True if the sound is playing, false otherwise.</returns>
    private bool IsSoundPlaying()
    {
        FMOD.Studio.PLAYBACK_STATE playbackState;
        movementSoundInstance.getPlaybackState(out playbackState);
        return playbackState == FMOD.Studio.PLAYBACK_STATE.PLAYING;
    }

    private void OnDestroy()
    {
        // Notify the EnemyManager that this enemy is no longer active
        EnemyManager.instance?.NotifyEnemyStoppedChasing(this);

        // Stop the FMOD event when the object is destroyed
        if (IsSoundPlaying())
        {
            movementSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            movementSoundInstance.release();
        }
    }

    /// <summary>
    /// Handles the chase and attack behavior of the enemy.
    /// If the target is within attack distance, attacks; otherwise, moves towards the target.
    /// </summary>
    private IEnumerator ChaseAndAttack()
    {
        if (aiData.currentTarget == null)
        { 
            movementInput = Vector2.zero; // Stop movement if no target
            following = false;
            if (IsSoundPlaying())
            {
                movementSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); // Stop sound immediately
            }
            yield break;
        }
        else
        {
            float distance = Vector2.Distance(aiData.currentTarget.position, transform.position);
             
            if (distance < attackDistance) // If the player is within attack distance
            {
                movementInput = Vector2.zero; // Stop movement 
                OnAttackPressed?.Invoke(); // Trigger the attack event
                yield return new WaitForSeconds(attackDelay); // Wait for attack delay before rechecking
                StartCoroutine(ChaseAndAttack());  // Continue chasing and attacking
            }
            else
            {
                movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData); // Move towards the target
                yield return new WaitForSeconds(aiUpdateDelay); // Wait for the AI update delay before rechecking
                StartCoroutine(ChaseAndAttack()); // Continue chasing and attacking
            }
        }
    }

    /// <summary>
    /// Changes the background music based on whether the enemy is chasing the player.
    /// </summary>
    private void ChangeMusic()
    {
        if (following)
        {
            // Notify the manager that this enemy is chasing
            EnemyManager.instance?.NotifyEnemyChasing(this);
        }
        else
        {
            // Notify the manager that this enemy stopped chasing
            EnemyManager.instance?.NotifyEnemyStoppedChasing(this);
        }
    }



    /// <summary>
    /// Triggered when the enemy dies. Stops movement sound and notifies the enemy manager.
    /// </summary>
    private void OnEnemyDeath(GameObject sender)
    {
        StopMovementSound(); // Stop any movement sound
        EnemyManager.instance?.NotifyEnemyStoppedChasing(this); // Notify the manager about the death
    }

}
