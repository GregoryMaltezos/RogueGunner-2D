using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject settingsPanel;
    public GameObject challengesPanel; // The panel where challenges will be listed
    public GameObject otherCanvas;
    public GameObject challengePrefab; // Prefab for displaying each challenge
    private bool isPaused = false;
    private GunController gunController;
    private PlayerController playerController;

    private GameObject previousPanel; // To track the previous panel for back navigation
    public InputActionReference pauseAction; // Bind this to the Pause button (e.g., Start)
    public InputActionReference navigateAction; // Bind this to the left stick or d-pad
    public InputActionReference submitAction; // Bind this to A button or Enter
    public InputActionReference cancelAction; // Bind this to B button or Escape
    public bool IsPaused
    {
        get { return isPaused; }
    }

    void Start()
    {
        gunController = FindObjectOfType<GunController>();
        playerController = FindObjectOfType<PlayerController>();
        pauseAction.action.Enable();
        navigateAction.action.Enable();
        submitAction.action.Enable();
        cancelAction.action.Enable();
    }

    void OnDisable()
    {
        // Disable the input actions when not in use
        pauseAction.action.Disable();
        navigateAction.action.Disable();
        submitAction.action.Disable();
        cancelAction.action.Disable();
    }

    void Update()
    {
        // Check for Pause button press (e.g., Start on gamepad)
        if (pauseAction.action.triggered)
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        // Handle menu navigation
        HandleNavigation();

        // Handle selection (submit action)
        if (submitAction.action.triggered)
        {
            // Trigger click on the button under the mouse cursor
            HandleSubmit();
        }

        // Handle cancel (back action)
        if (cancelAction.action.triggered)
        {
            HandleCancel();
        }
    }

    private void HandleNavigation()
    {
        // Get the left stick or d-pad input to navigate the UI
        Vector2 navigationInput = navigateAction.action.ReadValue<Vector2>();

        if (navigationInput.y > 0) // Up direction
        {
            // Navigate up in the menu (you can move UI elements or buttons here)
        }
        else if (navigationInput.y < 0) // Down direction
        {
            // Navigate down in the menu
        }

        if (navigationInput.x != 0) // Left or right direction
        {
            // Handle horizontal navigation if needed
        }
    }

    private void HandleSubmit()
    {
        // Create PointerEventData to simulate a mouse click
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue() // Get current mouse position
        };

        // Create a list to store all raycast results
        List<RaycastResult> raycastResults = new List<RaycastResult>();

        // Perform the raycast to detect all UI elements under the cursor
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        // Check if any UI elements were found under the cursor
        foreach (RaycastResult result in raycastResults)
        {
            // Check if the UI element is a button
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                // If it's a button, simulate a click
                button.onClick.Invoke();
                Debug.Log("Button clicked: " + button.name);
                return; // Exit once the button click is handled
            }
        }

        // If no button is found under the mouse cursor, log a message
        Debug.Log("No button under mouse cursor.");
    }

    private void HandleCancel()
    {
        // If there's a previous panel, go back to it
        if (previousPanel != null)
        {
            // Deactivate the current panel (settings or challenges)
            if (settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
            }
            if (challengesPanel.activeSelf)
            {
                challengesPanel.SetActive(false);
            }

            // Activate the previous panel (pause menu or whatever the previous panel was)
            previousPanel.SetActive(true);

            // Reset the previous panel reference, since we've navigated away
            previousPanel = null; // Optionally set this to another panel, if you want to allow nested navigation
        }
        else
        {
            // No previous panel, resume the game
            Resume();
        }
    }


    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(false);
        challengesPanel.SetActive(false); // Hide Challenges menu
        Time.timeScale = 1f;
        isPaused = false;
        SetCursorState(false);
        EnableGunController(true);
        EnablePlayerController(true);
        if (otherCanvas != null) otherCanvas.SetActive(true);
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        settingsPanel.SetActive(false);
        challengesPanel.SetActive(false); // Ensure challenges panel is hidden
        Time.timeScale = 0f;
        isPaused = true;
        SetCursorState(true);
        EnableGunController(false);
        EnablePlayerController(false);
        if (otherCanvas != null) otherCanvas.SetActive(false);

        // Set the default button to be selected when the pause menu opens
        EventSystem.current.SetSelectedGameObject(pauseMenuUI.transform.GetChild(0).gameObject); // Assuming the first child is a button
    }

    public void OpenSettings()
    {
        previousPanel = pauseMenuUI.activeSelf ? pauseMenuUI : null;
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(true);

        // Set the default button to be selected when the settings panel opens
        EventSystem.current.SetSelectedGameObject(settingsPanel.transform.GetChild(0).gameObject); // Assuming the first child is a button
    }

    public void BackToPauseMenu()
    {
        settingsPanel.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    private void SetCursorState(bool isVisible)
    {
        Cursor.visible = isVisible;
    }

    private void EnableGunController(bool enable)
    {
        if (gunController != null)
        {
            gunController.enabled = enable;
        }
    }

    private void EnablePlayerController(bool enable)
    {
        if (playerController != null)
        {
            playerController.enabled = enable;
        }
    }

    public void OpenChallenges()
    {
        // Save the current panel to return to later
        previousPanel = pauseMenuUI.activeSelf ? pauseMenuUI : settingsPanel;

        pauseMenuUI.SetActive(false); // Hide Pause Menu
        settingsPanel.SetActive(false); // Hide Settings Panel
        challengesPanel.SetActive(true); // Show Challenges Menu

        DisplayChallenges(); // Populate challenges UI with current data

        // Set the default button to be selected when the challenges panel opens
        EventSystem.current.SetSelectedGameObject(challengesPanel.transform.GetChild(0).gameObject); // Assuming the first child is a button
    }

    public void BackToPreviousMenu()
    {
        // Hide the challenges panel
        challengesPanel.SetActive(false);

        // Reactivate the previous panel
        if (previousPanel != null)
        {
            previousPanel.SetActive(true);
        }
    }

    private void DisplayChallenges()
    {
        List<ChallengeManager.Challenge> challenges = ChallengeManager.instance.challenges;

        // Ensure the Back button remains visible (if it's part of the challengesPanel)
        Transform backButtonTransform = challengesPanel.transform.Find("BackButton");
        if (backButtonTransform != null)
        {
            backButtonTransform.gameObject.SetActive(true);
        }

        // Clear all previous challenge entries to ensure a fresh display every time
        foreach (Transform child in challengesPanel.transform)
        {
            // Skip the Back button to prevent it from being destroyed
            if (child.name != "BackButton") // Replace with your actual button's name
            {
                Destroy(child.gameObject);
            }
        }

        // Instantiate new entries for each challenge
        for (int i = 0; i < challenges.Count; i++)
        {
            GameObject challengeTextObject = Instantiate(challengePrefab, challengesPanel.transform);

            TMP_Text challengeText = challengeTextObject.GetComponent<TMP_Text>();
            if (challengeText != null)
            {
                // Set the challenge status text with colored tags
                string status = challenges[i].completed ? "<color=green>Completed</color>" : "<color=red>Not Completed</color>";
                challengeText.text = $"{challenges[i].challengeId}: {status}\nDescription: {challenges[i].description}";
            }

            Debug.Log($"Challenge {i} displayed with ID: {challenges[i].challengeId} - {challenges[i].description}");
        }

        // Force layout update to refresh panel display
        LayoutRebuilder.ForceRebuildLayoutImmediate(challengesPanel.GetComponent<RectTransform>());
    }
}