using UnityEngine;
using UnityEngine.SceneManagement; // For loading the main menu
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; // For the new input system
using FMODUnity; // For FMOD integration
using FMOD.Studio; // For controlling EventInstance

public class bossPortal : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private CorridorFirstDungeonGenerator dungeonGenerator;
    private FadeManager fadeManager;
    private Collider2D portalCollider;

    // Cooldown variables
    private bool canInteract = true;
    private float interactionCooldown = 1.5f;

    // FMOD Event Reference
    [SerializeField]
    private string portalSpawnSound = "event:/PortalSpawn"; // FMOD event path for the spawn sound
    private EventInstance portalSoundInstance; // FMOD EventInstance for controlling the sound

    // UI Elements
    private Canvas[] canvasesToDisable;
    private Canvas thanksCanvas; // The canvas that holds the Thanks message
    private TextMeshProUGUI thanksForPlayingText; // The specific text object
    private GameObject blackBackground; // The black background behind the text

    // Reference to the InputAction for interacting with the portal
    private InputAction interactAction;

    private void Start()
    {
        // Automatically find the player by tag
        if (GameObject.FindWithTag("Player") == null)
        {
            Debug.LogError("Player not found. Make sure the Player has the 'Player' tag.");
        }

        dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>();
        if (dungeonGenerator == null)
        {
            Debug.LogError("Dungeon Generator not found in the scene.");
        }

        fadeManager = FindObjectOfType<FadeManager>();
        if (fadeManager == null)
        {
            Debug.LogError("FadeManager not found in the scene.");
        }

        portalCollider = GetComponent<Collider2D>();
        if (portalCollider == null)
        {
            Debug.LogError("Portal Collider not found. Please attach a Collider2D component to the portal.");
        }

        // Find canvases and the message text in the scene
        canvasesToDisable = FindObjectsOfType<Canvas>(); // Find all Canvas objects in the scene.
        thanksCanvas = GameObject.Find("ThanksCanvas")?.GetComponent<Canvas>(); // Find ThanksCanvas
        thanksForPlayingText = thanksCanvas?.transform.Find("ThanksForPlayingText")?.GetComponent<TextMeshProUGUI>(); // Find ThanksForPlayingText
        blackBackground = thanksCanvas?.transform.Find("BlackBackground")?.gameObject; // Find BlackBackground panel

        // If thanksCanvas or thanksForPlayingText are not found, log an error
        if (thanksCanvas == null)
        {
            Debug.LogError("ThanksCanvas not found in the scene.");
        }
        if (thanksForPlayingText == null)
        {
            Debug.LogError("ThanksForPlayingText not found in the ThanksCanvas.");
        }
        if (blackBackground == null)
        {
            Debug.LogError("BlackBackground panel not found in the ThanksCanvas.");
        }

        // Initialize the InputAction and bind to the interact method
        var playerInput = new NewControls(); // Assuming NewControls is your input action asset
        interactAction = playerInput.PlayerInput.Interact; // Assuming 'Interact' is the action name
        interactAction.Enable();

        // Start playing the portal spawn sound
        PlayPortalSpawnSound();
    }

    private void OnEnable()
    {
        // Enable the input action when the object is enabled
        interactAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the input action when the object is disabled
        interactAction.Disable();
    }

    private void Update()
    {
        // Check if player is near and can interact
        if (isPlayerNearby && canInteract)
        {
            if (interactAction.triggered) // Check if the interact action was triggered
            {
                // Stop the portal sound
                StopPortalSound();

                if (dungeonGenerator.currentFloor == 4) // Check if the player is on the 4th floor
                {
                    StartCoroutine(ShowThanksMessageAndExit());
                }
                else
                {
                    // Proceed with regular portal interaction logic
                    StartCoroutine(FadeToBlackAndProceed());
                }

                // Start cooldown after interaction
                StartCoroutine(InteractionCooldown());
            }
        }
    }

    private IEnumerator ShowThanksMessageAndExit()
    {
        // Disable all canvases except ThanksCanvas
        foreach (var canvas in canvasesToDisable)
        {
            if (canvas != thanksCanvas) // Exclude ThanksCanvas
            {
                canvas.gameObject.SetActive(false);
            }
        }

        // Show the "Thanks for playing" message and black background
        if (thanksForPlayingText != null && blackBackground != null)
        {
            thanksForPlayingText.gameObject.SetActive(true); // Enable the text
            blackBackground.SetActive(true); // Enable the black background
            thanksForPlayingText.text = "Thanks for playing!"; // Set the message
        }

        // Wait for the player to see the message
        yield return new WaitForSeconds(3f); // Message duration (can be adjusted)

        // After message, re-enable canvases
        foreach (var canvas in canvasesToDisable)
        {
            if (canvas != thanksCanvas) // Exclude ThanksCanvas
            {
                canvas.gameObject.SetActive(true);
            }
        }

        // Load the Main Menu Scene
        SceneManager.LoadScene("MainMenuScene");
    }

    private IEnumerator FadeToBlackAndProceed()
    {
        // Stop the footstep sound when transitioning to a new floor
        PlayerController.instance.StopFootstepSound();

        if (fadeManager != null && fadeManager.transitionCanvas != null)
        {
            fadeManager.transitionCanvas.gameObject.SetActive(true);
        }

        // Fade to black
        if (fadeManager != null)
        {
            yield return fadeManager.FadeToBlack();
        }

        // Proceed with floor transition
        GoToNextFloor();

        // Wait before fading back in
        yield return new WaitForSeconds(1f);

        // Fade back in
        if (fadeManager != null)
        {
            yield return fadeManager.FadeToClear();
        }

        Destroy(gameObject); // Destroy portal after transition
    }


    private void GoToNextFloor()
    {
        if (dungeonGenerator != null)
        {
            dungeonGenerator.OnBossDefeated();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    private IEnumerator InteractionCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }

    private void PlayPortalSpawnSound()
    {
        if (!string.IsNullOrEmpty(portalSpawnSound))
        {
            portalSoundInstance = RuntimeManager.CreateInstance(portalSpawnSound);
            portalSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            portalSoundInstance.start();
        }
    }

    private void StopPortalSound()
    {
        if (portalSoundInstance.isValid())
        {
            portalSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); // Graceful stop
            portalSoundInstance.release(); // Release the event instance
        }
    }
}
