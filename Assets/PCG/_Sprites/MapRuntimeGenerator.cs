using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MapRuntimeGenerator : MonoBehaviour
{
    public UnityEvent OnStart;

    // Remove the Start method to prevent automatic invocation
    // void Start() 
    // {
    //     OnStart?.Invoke();
    // }

    // Method to start dungeon generation, to be called from MainMenu
    public void StartDungeonGeneration()
    {
        // Call the OnStart event when dungeon generation starts
        OnStart?.Invoke();

        // Implement your dungeon generation logic here
        Debug.Log("Dungeon generation started");
    }
}
