using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    public Transform gunTransform; // Reference to the gun's transform
    public GameObject cursorIconPrefab; // Reference to the cursor icon prefab
    private static GameObject cursorIconInstance; // Static instance of the cursor icon to prevent multiple spawns

    private PlayerController playerController; // Reference to the PlayerController

    /// <summary>
    /// Initializes the cursor icon and hides the system cursor.
    /// It also gets a reference to the PlayerController instance.
    /// </summary>
    void Start()
    {
        // Check if the cursor icon instance is already created, if not, instantiate it
        if (cursorIconInstance == null)
        {
            cursorIconInstance = Instantiate(cursorIconPrefab);
            cursorIconInstance.transform.localScale *= 2.5f; // Make it larger
            Cursor.visible = false;
        }

        playerController = PlayerController.instance; // Get reference to PlayerController
    }

    /// <summary>
    /// Updates the position of the cursor icon and the gun's rotation based on mouse movement.
    /// </summary>
    void Update()
    {
        UpdateCursorIconPosition(); // Update the position of the cursor icon on screen
        UpdateGunRotation(); // Update the gun's rotation each frame
    }

    /// <summary>
    /// Uses the new Input System to get the mouse's position on the screen and update the cursor icon's position in the world space.
    /// Ensures the cursor icon stays in front of all other UI elements.
    /// </summary>
    void UpdateCursorIconPosition()
    {
        // Use the new Input System to get mouse position
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
        worldPosition.z = 0f; // Ensure the cursor icon stays on the same plane
        cursorIconInstance.transform.position = worldPosition; // Update the cursor icon's position

        // Ensure the cursor icon is at the frontmost layer (UI layer)
        cursorIconInstance.GetComponent<SpriteRenderer>().sortingLayerName = "UI";
        cursorIconInstance.GetComponent<SpriteRenderer>().sortingOrder = 999;
    }

    /// <summary>
    /// Updates the rotation of the gun based on the mouse position in world space.
    /// If the player is facing left, the rotation is adjusted accordingly.
    /// </summary>
    void UpdateGunRotation()
    {
        // Get the mouse position in world space
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
        mouseWorldPosition.z = 0f; // Ensure we are on the same plane (no z-axis rotation)

        // Get the direction from the gun to the mouse
        Vector3 direction = mouseWorldPosition - gunTransform.position;

        // Calculate the target rotation angle based on the mouse position
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Adjust the rotation based on the player's facing direction
        if (!playerController.IsFacingRight())
        {
            targetAngle += 180f; // Flip the angle if the player is facing left
        }

        // Rotate the gun to face the target angle
        Quaternion rotation = Quaternion.Euler(0f, 0f, targetAngle);
        gunTransform.rotation = rotation; // Update the gun's rotation
    }

    /// <summary>
    /// Ensures the system cursor is visible when the GunController script is destroyed.
    /// </summary>
    void OnDestroy()
    {
        // Show the system cursor when the script is destroyed
        Cursor.visible = true;
    }
}
