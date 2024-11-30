using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;

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

    private void Start()
    {
        // Detecting Player and Obstacles around
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


    private void PerformDetection()
    {
        foreach (Detector detector in detectors)
        {
            detector.Detect(aiData);
        }
    }

    private void Update()
    {
        if (healthScript != null && healthScript.isDead)
        {
            // If the enemy is already dead, stop further updates
            return;
        }

        if (aiData.currentTarget != null)
        {
            float distanceToPlayer = Vector2.Distance(aiData.currentTarget.position, transform.position);

            if (distanceToPlayer > chaseRadius) // Player is out of chase radius
            {
                StopChasing();
            }
            else
            {
                if (!following)
                {
                    following = true;
                    StartCoroutine(ChaseAndAttack());
                    PlayMovementSound();
                    ChangeMusic();
                }

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

    private void PlayMovementSound()
    {
        if (!IsSoundPlaying())
        {
            movementSoundInstance.start();
        }
    }

    private void StopMovementSound()
    {
        if (IsSoundPlaying())
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
        // Notify the EnemyManager that this enemy is no longer active
        EnemyManager.instance?.NotifyEnemyStoppedChasing(this);

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
                movementInput = Vector2.zero;
                OnAttackPressed?.Invoke();
                yield return new WaitForSeconds(attackDelay);
                StartCoroutine(ChaseAndAttack());
            }
            else
            {
                movementInput = movementDirectionSolver.GetDirectionToMove(steeringBehaviours, aiData);
                yield return new WaitForSeconds(aiUpdateDelay);
                StartCoroutine(ChaseAndAttack());
            }
        }
    }

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



    // Triggered when the enemy dies
    private void OnEnemyDeath(GameObject sender)
    {
        StopMovementSound(); // Stop any movement sound
        EnemyManager.instance?.NotifyEnemyStoppedChasing(this); // Notify the manager about the death
    }

}
