using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    private List<EnemyAI> allEnemies = new List<EnemyAI>(); // Keep track of all enemies
    private List<EnemyAI> activeEnemies = new List<EnemyAI>(); // Track actively chasing enemies

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

    // Register an enemy with the manager
    public void RegisterEnemy(EnemyAI enemy)
    {
        if (!allEnemies.Contains(enemy))
        {
            allEnemies.Add(enemy);
        }
    }

    // Deregister an enemy when it is destroyed
    public void DeregisterEnemy(EnemyAI enemy)
    {
        allEnemies.Remove(enemy);
        activeEnemies.Remove(enemy);

        // If no active enemies remain, revert to peaceful music
        if (activeEnemies.Count == 0)
        {
            AudioManager.instance.SetMusicArea(MusicType.Peacefull);
        }
    }

    // Registers an enemy as chasing
    public void NotifyEnemyChasing(EnemyAI enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);

            // Switch to combat music if this is the first active enemy
            if (activeEnemies.Count == 1)
            {
                AudioManager.instance.SetMusicArea(MusicType.Combat);
            }
        }
    }

    // Deregisters an enemy when it stops chasing
    public void NotifyEnemyStoppedChasing(EnemyAI enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);

            // If no active enemies remain, revert to peaceful music
            if (activeEnemies.Count == 0)
            {
                AudioManager.instance.SetMusicArea(MusicType.Peacefull);
            }
        }
    }

    // Check if any enemies are actively chasing
    public bool AreEnemiesActive()
    {
        return activeEnemies.Count > 0;
    }
}
