using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerMouse : MonoBehaviour
{
    [SerializeField] private InputActionReference rightStickAction; // Reference to the Input Action
    [SerializeField] private float mouseSpeed = 100f; // Speed multiplier for mouse movement

    private Vector2 virtualMousePosition; // Virtual position to track the mouse

    void OnEnable()
    {
        rightStickAction.action.Enable();

        // Initialize the virtual mouse position to the current mouse position, or center of the screen
        if (Mouse.current != null)
        {
            virtualMousePosition = Mouse.current.position.ReadValue();
        }
        else
        {
            virtualMousePosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        }
    }

    void OnDisable()
    {
        rightStickAction.action.Disable();
    }

    void Update()
    {
        // Get the right stick input
        Vector2 rightStickInput = rightStickAction.action.ReadValue<Vector2>();

        // Only process movement if there is significant input
        if (rightStickInput.sqrMagnitude > 0.01f)
        {
            // Calculate the mouse delta
            Vector2 mouseDelta = rightStickInput * mouseSpeed * Time.unscaledDeltaTime; // Use Time.unscaledDeltaTime to ignore pause state

            // Update the virtual mouse position
            virtualMousePosition += mouseDelta;

            // Clamp the virtual position to the screen boundaries
            virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, 0, Screen.width);
            virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, 0, Screen.height);

            // Set the new mouse position
            Mouse.current.WarpCursorPosition(virtualMousePosition);
        }
    }
}
