using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainMenuUI; // Main Menu UI root
    public GameObject controlsPanel; // Reference to the Controls Panel
    public GameObject playButton; // Reference to the Play button
    public GameObject quitButton; // Reference to the Quit button
    public GameObject backButton; // Back button in the controls panel

    public InputActionReference navigateAction; // For navigation
    public InputActionReference submitAction;   // For selection
    public InputActionReference cancelAction;   // For cancel/back actions

    private GameObject previousPanel; // Track the previous active panel for navigation

    /// <summary>
    /// Sets up default input actions and selects the Play button.
    /// </summary>
    private void Start()
    {
        // Enable navigation, submit, and cancel input actions
        navigateAction.action.Enable();
        submitAction.action.Enable();
        cancelAction.action.Enable();

        // Set the Play button as the default selected UI element
        EventSystem.current.SetSelectedGameObject(playButton);
    }

    /// <summary>
    /// Called when the script is disabled. Disables input actions.
    /// </summary>
    private void OnDisable()
    {
        // Disable navigation, submit, and cancel input actions to prevent unintended behavior
        navigateAction.action.Disable();
        submitAction.action.Disable();
        cancelAction.action.Disable();
    }


    /// <summary>
    /// Handles navigation input for moving selection up or down between menu options.
    /// </summary>
    private void HandleNavigation()
    {
        Vector2 navigationInput = navigateAction.action.ReadValue<Vector2>();

        if (navigationInput.y > 0) // Navigate up
        {
            MoveSelectionUp();
        }
        else if (navigationInput.y < 0) // Navigate down
        {
            MoveSelectionDown();
        }
    }

    /// <summary>
    /// Moves the UI selection upward in the menu.
    /// </summary>
    private void MoveSelectionUp()
    {
        GameObject previousSelected = EventSystem.current.currentSelectedGameObject;
        if (previousSelected != null)
        {
            int previousIndex = previousSelected.transform.GetSiblingIndex();
            if (previousIndex > 0) // Ensure index doesn't go below 0
            {
                // Select the previous sibling in the hierarchy
                EventSystem.current.SetSelectedGameObject(previousSelected.transform.parent.GetChild(previousIndex - 1).gameObject);
            }
        }
    }

    /// <summary>
    /// Moves the UI selection downward in the menu.
    /// </summary>
    private void MoveSelectionDown()
    {
        GameObject previousSelected = EventSystem.current.currentSelectedGameObject;
        if (previousSelected != null)
        {
            int previousIndex = previousSelected.transform.GetSiblingIndex();
            if (previousIndex < previousSelected.transform.parent.childCount - 1) // Ensure index doesn't exceed bounds
            {
                // Select the next sibling in the hierarchy
                EventSystem.current.SetSelectedGameObject(previousSelected.transform.parent.GetChild(previousIndex + 1).gameObject);
            }
        }
    }

    /// <summary>
    /// Continuously handles navigation and input actions in the main menu.
    /// </summary>
    private void Update()
    {
        HandleNavigation(); // Process navigation input (up/down movement)

        if (submitAction.action.triggered)  // Handle submit action (e.g., button click simulation)
        {
            Debug.Log("Submit action triggered");
            HandleSubmit();
        }

        if (cancelAction.action.triggered) // Handle cancel action (e.g., going back to a previous menu)
        {
            Debug.Log("Cancel action triggered");
            HandleCancel();
        }
    }

    /// <summary>
    /// Handles the submit action, triggering the selected button's functionality.
    /// </summary>
    private void HandleSubmit()
    {
        GameObject selectedGameObject = EventSystem.current.currentSelectedGameObject;
        if (selectedGameObject != null)
        {
            // Attempt to get the Button component from the selected object
            Button selectedButton = selectedGameObject.GetComponent<Button>();
            if (selectedButton != null) 
            {
                Debug.Log("Selected button: " + selectedButton.name);
                if (selectedButton.name == "ControlsButton") // Special handling for the Controls button
                {
                    OpenControlsPanel();
                }
                else
                {
                    selectedButton.onClick.Invoke(); // Simulate a button click
                }
            }
            else
            {
                Debug.LogWarning("No Button component found on selected GameObject.");
            }
        }
        else
        {
            Debug.LogWarning("No GameObject is currently selected.");
        }
    }


    /// <summary>
    /// Opens the Controls panel and hides the Main Menu UI.
    /// </summary>
    public void OpenControlsPanel()
    {
        previousPanel = mainMenuUI; // Save the current panel
        mainMenuUI.SetActive(false); // Hide Main Menu
        controlsPanel.SetActive(true); // Show Controls Panel

        // Set the Back button in the Controls panel as the default selection
        EventSystem.current.SetSelectedGameObject(null); // Clear selection first
        EventSystem.current.SetSelectedGameObject(backButton); // Set the Back button as selected

        Debug.Log("Opened Controls Panel");
    }

    /// <summary>
    /// Handles the cancel action, navigating back to the Main Menu if on the Controls panel.
    /// </summary>
    private void HandleCancel()
    {
        // If the controls panel is active, go back to the main menu
        if (controlsPanel.activeSelf)
        {
            BackToMainMenu();
        }
        else
        {
            // Default cancel action (e.g., quit game)
            Debug.Log("Cancel action triggered.");
            QuitGame();
        }
    }


    /// <summary>
    /// Starts the game by loading the specified scene.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("Starting Game...");
        SceneManager.LoadScene("RoomContent");
    }

    /// <summary>
    /// Quits the application and logs the action.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }



    /// <summary>
    /// Returns to the Main Menu from the Controls panel.
    /// </summary>
    public void BackToMainMenu()
    {
        controlsPanel.SetActive(false); // Hide Controls Panel
        mainMenuUI.SetActive(true); // Show Main Menu

        // Set the default selected button in the main menu
        EventSystem.current.SetSelectedGameObject(playButton);
    }
}
