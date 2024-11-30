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

    private void Start()
    {
        // Enable the input actions
        navigateAction.action.Enable();
        submitAction.action.Enable();
        cancelAction.action.Enable();

        // Set the default selected button
        EventSystem.current.SetSelectedGameObject(playButton);
    }

    private void OnDisable()
    {
        navigateAction.action.Disable();
        submitAction.action.Disable();
        cancelAction.action.Disable();
    }

 

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

    private void MoveSelectionUp()
    {
        GameObject previousSelected = EventSystem.current.currentSelectedGameObject;
        if (previousSelected != null)
        {
            int previousIndex = previousSelected.transform.GetSiblingIndex();
            if (previousIndex > 0)
            {
                EventSystem.current.SetSelectedGameObject(previousSelected.transform.parent.GetChild(previousIndex - 1).gameObject);
            }
        }
    }

    private void MoveSelectionDown()
    {
        GameObject previousSelected = EventSystem.current.currentSelectedGameObject;
        if (previousSelected != null)
        {
            int previousIndex = previousSelected.transform.GetSiblingIndex();
            if (previousIndex < previousSelected.transform.parent.childCount - 1)
            {
                EventSystem.current.SetSelectedGameObject(previousSelected.transform.parent.GetChild(previousIndex + 1).gameObject);
            }
        }
    }

    private void Update()
    {
        HandleNavigation();

        if (submitAction.action.triggered)
        {
            Debug.Log("Submit action triggered");
            HandleSubmit();
        }

        if (cancelAction.action.triggered)
        {
            Debug.Log("Cancel action triggered");
            HandleCancel();
        }
    }

    private void HandleSubmit()
    {
        GameObject selectedGameObject = EventSystem.current.currentSelectedGameObject;
        if (selectedGameObject != null)
        {
            Button selectedButton = selectedGameObject.GetComponent<Button>();
            if (selectedButton != null)
            {
                Debug.Log("Selected button: " + selectedButton.name);
                if (selectedButton.name == "ControlsButton")
                {
                    OpenControlsPanel();
                }
                else
                {
                    selectedButton.onClick.Invoke(); // Simulate button click
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


    public void OpenControlsPanel()
    {
        previousPanel = mainMenuUI; // Save the current panel
        mainMenuUI.SetActive(false); // Hide Main Menu
        controlsPanel.SetActive(true); // Show Controls Panel

        // Set the default selected button in the controls panel
        EventSystem.current.SetSelectedGameObject(null); // Clear selection first
        EventSystem.current.SetSelectedGameObject(backButton); // Set the Back button as selected

        Debug.Log("Opened Controls Panel");
    }

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

    // Function to start the game
    public void StartGame()
    {
        Debug.Log("Starting Game...");
        SceneManager.LoadScene("RoomContent");
    }

    // Function to quit the game
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }



    // Go back to the Main Menu
    public void BackToMainMenu()
    {
        controlsPanel.SetActive(false); // Hide Controls Panel
        mainMenuUI.SetActive(true); // Show Main Menu

        // Set the default selected button in the main menu
        EventSystem.current.SetSelectedGameObject(playButton);
    }
}
