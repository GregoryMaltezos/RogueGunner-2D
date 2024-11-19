using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }


    [field: Header("Pistol SFX")]
    [field: SerializeField] public EventReference pistolFired {  get; private set; }

    [field: Header("ChestIdle SFX")]
    [field: SerializeField] public EventReference chestIdle { get; private set; }

    [field: Header("Ambiance")]
    [field: SerializeField] public EventReference ambiance { get; private set; }

    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; private set; }

    public static FMODEvents instance {  get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one FMOD Events instance in scene");
        }
        instance = this;
    }
}
