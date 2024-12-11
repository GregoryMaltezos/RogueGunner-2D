using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    private List<EnemyAI> allEnemies = new List<EnemyAI>(); // Keep track of all enemies
    private List<EnemyAI> activeEnemies = new List<EnemyAI>(); // Track actively chasing enemies

    /// <summary>
    /// Ensures that there is only one instance of EnemyManager in the scene. 
    /// If an instance already exists, it destroys this one to maintain the singleton pattern.
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure there's only one EnemyManager in the scene
        }
    }

    /// <summary>
    /// Registers a new enemy in the manager to keep track of it.
    /// </summary>
    /// <param name="enemy">The enemy object to register.</param>
    public void RegisterEnemy(EnemyAI enemy)
    {
        if (!allEnemies.Contains(enemy)) // Prevent adding duplicates
        {
            allEnemies.Add(enemy); // Add the enemy to the list of all enemies
        }
    }

    /// <summary>
    /// Deregisters an enemy from the manager when it is destroyed.
    /// Also stops tracking it as an active enemy if it's chasing.
    /// </summary>
    /// <param name="enemy">The enemy object to deregister.</param>
    public void DeregisterEnemy(EnemyAI enemy)
    {
        allEnemies.Remove(enemy); // Remove the enemy from the list of all enemies
        activeEnemies.Remove(enemy); // Remove the enemy from the active chasing list

        // If no active enemies remain, revert to peaceful music
        if (activeEnemies.Count == 0)
        {
            AudioManager.instance.SetMusicArea(MusicType.Peacefull); // Switch to peaceful music
        }
    }

    /// <summary>
    /// Registers an enemy as actively chasing the player and switches to combat music if it's the first enemy.
    /// </summary>
    /// <param name="enemy">The enemy that is now chasing the player.</param>
    public void NotifyEnemyChasing(EnemyAI enemy)
    {
        if (!activeEnemies.Contains(enemy)) // Prevent adding duplicates
        {
            activeEnemies.Add(enemy); // Add the enemy to the active chasing list

            // If this is the first active enemy, start combat music
            if (activeEnemies.Count == 1)
            {
                AudioManager.instance.SetMusicArea(MusicType.Combat); // Switch to combat music
            }
        }
    }

    /// <summary>
    /// Deregisters an enemy when it stops chasing the player. 
    /// If no enemies remain chasing, it switches back to peaceful music.
    /// </summary>
    /// <param name="enemy">The enemy that stopped chasing.</param>
    public void NotifyEnemyStoppedChasing(EnemyAI enemy)
    {
        if (activeEnemies.Contains(enemy)) // Check if the enemy is currently chasing
        {
            activeEnemies.Remove(enemy); // Remove the enemy from the active chasing list

            // If no active enemies remain, revert to peaceful music
            if (activeEnemies.Count == 0)
            {
                AudioManager.instance.SetMusicArea(MusicType.Peacefull); // If no active enemies remain, switch back to peaceful music
            }
        }
    }

    /// <summary>
    /// Checks if there are any active enemies currently chasing the player.
    /// </summary>
    /// <returns>True if there are active enemies, otherwise false.</returns>
    public bool AreEnemiesActive()
    {
        return activeEnemies.Count > 0; // Return true if there are active enemies
    }
}
