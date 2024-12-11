using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerMouse : MonoBehaviour
{
    [SerializeField] private InputActionReference rightStickAction; // Reference to the Input Action
    [SerializeField] private float mouseSpeed = 100f; // Speed multiplier for mouse movement

    private Vector2 virtualMousePosition; // Virtual position to track the mouse

    /// <summary>
    /// Enables the input action and initializes the virtual mouse position when the object is enabled.
    /// If the mouse is available, it starts from the current position; otherwise, it initializes at the center of the screen.
    /// </summary>
    void OnEnable()
    {
        rightStickAction.action.Enable(); // Enable the right stick action input

        // Initialize the virtual mouse position based on the current mouse position
        if (Mouse.current != null)
        {
            virtualMousePosition = Mouse.current.position.ReadValue();
        }
        else
        {
            virtualMousePosition = new Vector2(Screen.width / 2f, Screen.height / 2f); // If mouse is not found, set the initial position to the center of the screen
        }
    }
    /// <summary>
    /// Disables the input action when the object is disabled.
    /// </summary>
    void OnDisable()
    {
        rightStickAction.action.Disable(); // Disable the right stick action input
    }

    /// <summary>
    /// Updates the virtual mouse position based on right stick input and applies mouse movement with a defined speed.
    /// The mouse movement is clamped within the screen boundaries.
    /// </summary>
    void Update()
    {
        
        Vector2 rightStickInput = rightStickAction.action.ReadValue<Vector2>(); // Get the current input from the right stick (2D vector)

        // Only process movement if there is significant input
        if (rightStickInput.sqrMagnitude > 0.01f)
        {
            // Calculate the mouse delta movement based on right stick input and mouse speed
            Vector2 mouseDelta = rightStickInput * mouseSpeed * Time.unscaledDeltaTime;

            // Update the virtual mouse position
            virtualMousePosition += mouseDelta;

            // Clamp the virtual position to ensure it stays within screen boundaries
            virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, 0, Screen.width);
            virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, 0, Screen.height);

            // Set the new mouse position
            Mouse.current.WarpCursorPosition(virtualMousePosition);
        }
    }
}
