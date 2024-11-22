using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;  // Ensure this is included for Button functionality
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainMenuUI; // The root of the Main Menu UI
    public GameObject playButton; // Reference to the Play button in the Main Menu
    public GameObject quitButton; // Reference to the Quit button in the Main Menu

    public InputActionReference navigateAction; // Bind this to the left stick or d-pad
    public InputActionReference submitAction;   // Bind this to South button (A button or Enter)
    public InputActionReference cancelAction;   // Bind this to B button or Escape

    private void Start()
    {
        // Enable the input actions
        navigateAction.action.Enable();
        submitAction.action.Enable();
        cancelAction.action.Enable();

        // Set the default button to be selected when the menu opens
        EventSystem.current.SetSelectedGameObject(mainMenuUI.transform.GetChild(0).gameObject); // First child is the first button (Play)
    }

    private void OnDisable()
    {
        // Disable the actions when not in use
        navigateAction.action.Disable();
        submitAction.action.Disable();
        cancelAction.action.Disable();
    }

    private void Update()
    {
        // Handle menu navigation
        HandleNavigation();

        // Handle selection (submit action)
        if (submitAction.action.triggered)
        {
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
        // Get the navigation input (left stick or D-pad)
        Vector2 navigationInput = navigateAction.action.ReadValue<Vector2>();

        // Handle vertical navigation (up/down)
        if (navigationInput.y > 0) // Up direction
        {
            MoveSelectionUp();
        }
        else if (navigationInput.y < 0) // Down direction
        {
            MoveSelectionDown();
        }

        // Handle horizontal navigation (left/right) if needed (skipped for simplicity)
    }

    private void MoveSelectionUp()
    {
        // Navigate up in the menu
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
        // Navigate down in the menu
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

    private void HandleSubmit()
    {
        // Check if the selected UI element is a button and invoke the click
        Button selectedButton = EventSystem.current.currentSelectedGameObject?.GetComponent<Button>();
        if (selectedButton != null)
        {
            selectedButton.onClick.Invoke(); // Simulate the button click
            Debug.Log("Button clicked: " + selectedButton.name);
        }
    }

    private void HandleCancel()
    {
        // Handle cancel (back action), like quitting or doing other tasks
        Debug.Log("Cancel action triggered.");
        QuitGame();
    }

    // Function to start the game
    public void StartGame()
    {
        Debug.Log("Starting Game...");
        // Replace "YourGameScene" with your actual scene name
        SceneManager.LoadScene("RoomContent");
    }

    // Function to quit the game
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
