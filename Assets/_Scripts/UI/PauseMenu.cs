using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using FMODUnity;
using FMOD.Studio; // For FMOD Bus and Event management
public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject settingsPanel;
    public GameObject challengesPanel;
    public GameObject controlsPanel; // Add reference to the Controls Panel
    public GameObject otherCanvas;
    public GameObject challengePrefab;
    private bool isPaused = false;
    private GunController gunController;
    private PlayerController playerController;

    private GameObject previousPanel; // Stores the previous panel before switching to another
    public InputActionReference pauseAction;
    public InputActionReference navigateAction;
    public InputActionReference submitAction;
    public InputActionReference cancelAction;
    private Bus masterBus;
    public bool IsPaused
    {
        get { return isPaused; }
    }

    /// <summary>
    /// Called when the script is first initialized.
    /// Initializes necessary components and enables input actions.
    /// </summary>
    void Start()
    {
        gunController = FindObjectOfType<GunController>();
        playerController = FindObjectOfType<PlayerController>();
        // Enable all input actions for pausing, navigation, submitting, and canceling
        pauseAction.action.Enable();
        navigateAction.action.Enable();
        submitAction.action.Enable();
        cancelAction.action.Enable();
        masterBus = RuntimeManager.GetBus("bus:/");
    }
    /// <summary>
    /// Called when the script is disabled.
    /// Disables input actions to avoid unnecessary processing when the script is no longer in use.
    /// </summary>
    void OnDisable()
    {
        pauseAction.action.Disable();
        navigateAction.action.Disable();
        submitAction.action.Disable();
        cancelAction.action.Disable();
    }
    /// <summary>
    /// Handles inputs for pausing, navigation, submitting, and canceling actions.
    /// </summary>
    void Update()
    {
        if (pauseAction.action.triggered) // Toggle pause when pause action is triggered
        {
            if (isPaused)
            {
                Resume(); // Resume the game if it's paused
            }
            else
            {
                Pause(); // Pause the game if it's running
            }
        }

        HandleNavigation(); 

        if (submitAction.action.triggered)
        {
            HandleSubmit();
        }

        if (cancelAction.action.triggered)
        {
            HandleCancel();
        }
    }

    /// <summary>
    /// Handles navigation input for moving through menu options.
    /// </summary>
    private void HandleNavigation()
    {
        Vector2 navigationInput = navigateAction.action.ReadValue<Vector2>();

        if (navigationInput.y > 0) // Up
        {
            // Navigate up
        }
        else if (navigationInput.y < 0) // Down
        {
            // Navigate down
        }

        if (navigationInput.x != 0) // Left/Right
        {
            // Handle horizontal navigation
        }
    }
    /// <summary>
    /// Handles submit action, which is typically used for clicking buttons.
    /// </summary>
    private void HandleSubmit()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current) // Create a new pointer event to simulate a mouse click
        {
            position = Mouse.current.position.ReadValue() // Get current mouse position
        };
        // List to store the results of raycasting from the pointer event
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);  // Perform raycast to detect UI elements

        foreach (RaycastResult result in raycastResults) // Loop through the raycast results to check if any button was clicked
        {
            Button button = result.gameObject.GetComponent<Button>(); // Get the button component from the result's game object
            if (button != null)
            {
                button.onClick.Invoke(); // If a button was clicked, invoke its onClick event
                Debug.Log("Button clicked: " + button.name);
                return;
            }
        }

        Debug.Log("No button under mouse cursor.");
    }
    /// <summary>
    /// Handles cancel action. Closes the current panel or resumes the game if no panel is active.
    /// </summary>
    private void HandleCancel()
    {
        if (previousPanel != null) // If there is a previous panel, restore it
        {
            // Hide the currently active panels and show the previous one
            if (settingsPanel.activeSelf) settingsPanel.SetActive(false);
            if (challengesPanel.activeSelf) challengesPanel.SetActive(false);
            if (controlsPanel.activeSelf) controlsPanel.SetActive(false);

            previousPanel.SetActive(true);
            previousPanel = null; // Reset the previous panel reference
        }
        else
        {
            Resume(); // If no previous panel, resume the game
        }
    }
    /// <summary>
    /// Resumes the game, hides the pause menu, and restores player control.
    /// </summary>
    public void Resume()
    {
        // Deactivate the pause and other menus
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(false);
        challengesPanel.SetActive(false);
        controlsPanel.SetActive(false); // Ensure controls panel is hidden
        // Resume the game by resetting the time scale
        Time.timeScale = 1f;
        isPaused = false;
        SetCursorState(false);
        EnableGunController(true);
        EnablePlayerController(true);
        if (otherCanvas != null) otherCanvas.SetActive(true);
        masterBus.setPaused(false);
    }
    /// <summary>
    /// Pauses the game and displays the pause menu.
    /// </summary>
    public void Pause()
    {
        // Display the pause menu and hide all other panels
        pauseMenuUI.SetActive(true);
        settingsPanel.SetActive(false);
        challengesPanel.SetActive(false);
        controlsPanel.SetActive(false); // Ensure controls panel is hidden
        // Pause the game by setting the time scale to 0
        Time.timeScale = 0f;
        isPaused = true;
        SetCursorState(true);
        EnableGunController(false);
        EnablePlayerController(false);
        if (otherCanvas != null) otherCanvas.SetActive(false);

        EventSystem.current.SetSelectedGameObject(pauseMenuUI.transform.GetChild(0).gameObject);
        masterBus.setPaused(true);
    }
    /// <summary>
    /// Opens the settings menu and hides the pause menu.
    /// </summary>
    public void OpenSettings()
    {
        // Store the current panel (pause menu) to return to later
        previousPanel = pauseMenuUI.activeSelf ? pauseMenuUI : null;
        // Hide the pause menu and display the settings panel
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(settingsPanel.transform.GetChild(0).gameObject);
    }
    /// <summary>
    /// Opens the controls menu and hides the pause menu.
    /// </summary>
    public void OpenControls()
    {
        // Store the current panel (pause menu) to return to later
        previousPanel = pauseMenuUI.activeSelf ? pauseMenuUI : null;
        // Hide the pause menu and display the controls panel
        pauseMenuUI.SetActive(false);
        controlsPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(controlsPanel.transform.GetChild(0).gameObject);
    }
    /// <summary>
    /// Goes back to the pause menu from the settings or controls panel.
    /// </summary>
    public void BackToPauseMenu()
    {
        // Hide the settings and controls panels and show the pause menu again
        settingsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        pauseMenuUI.SetActive(true);
    }
    /// <summary>
    /// Restarts the current scene, effectively resetting the game.
    /// </summary>
    public void Restart()
    {
        // Reset the time scale to normal speed and reload the current scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    /// <summary>
    /// Quits to the main menu and loads the main menu scene.
    /// </summary>
    public void QuitToMenu()
    {
        // Reset time scale and load the main menu scene
        Time.timeScale = 1f;
        masterBus.setPaused(false);
        SceneManager.LoadScene("MainMenuScene");
    }
    /// <summary>
    /// Sets the visibility of the cursor.
    /// </summary>
    /// <param name="isVisible">True to show the cursor, false to hide it.</param>
    private void SetCursorState(bool isVisible)
    {
        Cursor.visible = isVisible;
    }
    /// <summary>
    /// Enables or disables the gun controller.
    /// </summary>
    /// <param name="enable">True to enable, false to disable.</param>
    private void EnableGunController(bool enable)
    {
        if (gunController != null)
        {
            gunController.enabled = enable;
        }
    }
    /// <summary>
    /// Enables or disables the player controller.
    /// </summary>
    /// <param name="enable">True to enable, false to disable.</param>
    private void EnablePlayerController(bool enable)
    {
        if (playerController != null)
        {
            playerController.enabled = enable;
        }
    }
    /// <summary>
    /// Opens the challenges menu and hides the pause or settings menu.
    /// Displays a list of challenges.
    /// </summary>
    public void OpenChallenges()
    {
        // Store the current panel to return to it later
        previousPanel = pauseMenuUI.activeSelf ? pauseMenuUI : settingsPanel;
        // Hide the pause and settings panels, and display the challenges panel
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(false);
        challengesPanel.SetActive(true);
        // Display the list of challenges
        DisplayChallenges();

        EventSystem.current.SetSelectedGameObject(challengesPanel.transform.GetChild(0).gameObject);
    }
    /// <summary>
    /// Goes back to the previous menu (either settings or pause menu) from the challenges menu.
    /// </summary>
    public void BackToPreviousMenu()
    {
        // Hide the challenges panel and show the previous panel (either settings or pause menu)
        challengesPanel.SetActive(false);

        if (previousPanel != null)
        {
            previousPanel.SetActive(true);
        }
    }
    /// <summary>
    /// Displays a list of challenges in the challenges panel.
    /// </summary>
    private void DisplayChallenges()
    {
        // Get the list of challenges from the ChallengeManager
        List<ChallengeManager.Challenge> challenges = ChallengeManager.instance.challenges;
        // Find and activate the Back button in the challenges panel
        Transform backButtonTransform = challengesPanel.transform.Find("BackButton");
        if (backButtonTransform != null)
        {
            backButtonTransform.gameObject.SetActive(true);
        }
        // Destroy all previous challenge objects in the panel except for the Back button
        foreach (Transform child in challengesPanel.transform)
        {
            if (child.name != "BackButton")
            {
                Destroy(child.gameObject);
            }
        }
        // Instantiate and display each challenge
        for (int i = 0; i < challenges.Count; i++)
        {
            GameObject challengeTextObject = Instantiate(challengePrefab, challengesPanel.transform);

            TMP_Text challengeText = challengeTextObject.GetComponent<TMP_Text>();
            if (challengeText != null)
            {
                string status = challenges[i].completed ? "<color=green>Completed</color>" : "<color=red>Not Completed</color>";
                challengeText.text = $"{challenges[i].challengeId}: {status}\nDescription: {challenges[i].description}";
            }

            Debug.Log($"Challenge {i} displayed with ID: {challenges[i].challengeId} - {challenges[i].description}");
        }
        // Force the layout to rebuild, ensuring the challenges are displayed correctly
        LayoutRebuilder.ForceRebuildLayoutImmediate(challengesPanel.GetComponent<RectTransform>());
    }
}
