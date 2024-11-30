using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0, 1)]
    public float masterVolume = 1;
    [Range(0, 1)]
    public float musicVolume = 1;
    [Range(0, 1)]
    public float sfxVolume = 1;

    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;

    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;
    private EventInstance ambianceEventInstance;
    private EventInstance musicEventInstance;
    public static AudioManager instance { get; private set; }

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void InitializeAmbiance(EventReference ambianceEventReference)
    {
        ambianceEventInstance = CreateInstance(ambianceEventReference);
        ambianceEventInstance.start();
    }

    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey(MASTER_VOLUME_KEY))
        {
            masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY);
        }
        if (PlayerPrefs.HasKey(MUSIC_VOLUME_KEY))
        {
            musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY);
        }
        if (PlayerPrefs.HasKey(SFX_VOLUME_KEY))
        {
            sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY);
        }
    }

    private void Start()
    {
        LoadVolumeSettings();
        InitializeAmbiance(FMODEvents.instance.ambiance);
        InitializeMusic(FMODEvents.instance.music);
    }
    public void SetMusicArea(MusicType type)
    {
        musicEventInstance.setParameterByName("Combat", (float)type);
    }
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found multiple AudioManagers in scene");
        }
        instance = this;
        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }

    private void Update()
    {
        // Continuously update the volume levels
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        sfxBus.setVolume(sfxVolume);
    }

    // Method to save volume settings to PlayerPrefs
    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save(); // Ensure the changes are written to disk
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
        return emitter;
    }

    private void InitializeMusic(EventReference musicEventReference)
    {
        musicEventInstance = CreateInstance(musicEventReference);
        musicEventInstance.start();
    }

    public EventInstance PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        EventInstance instance = RuntimeManager.CreateInstance(sound);
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(worldPos));
        instance.start();
        return instance;  // Return the EventInstance so you can manage it later if needed
    }


    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    private void CleanUp()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

        foreach (StudioEventEmitter emitter in eventEmitters)
        {
            emitter.Stop();
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
