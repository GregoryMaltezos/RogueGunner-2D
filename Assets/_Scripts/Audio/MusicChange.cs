using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicChange : MonoBehaviour
{
    [Header("Music Change Settings")]
    [SerializeField] private MusicType type;
    [SerializeField] private float detectionRange = 5f; // Range at which the enemy detects the player
    [SerializeField] private LayerMask playerLayer; // Layer mask to detect the player

    private Transform player; // Reference to the player's transform

    private void Start()
    {
        // Assuming the player is tagged as "Player"
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        DetectPlayer();
    }

    private void DetectPlayer()
    {
        // Check if the player is within the detection range
        if (Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            // If the player is detected, change the music
            AudioManager.instance.SetMusicArea(type);
        }
    }
}
