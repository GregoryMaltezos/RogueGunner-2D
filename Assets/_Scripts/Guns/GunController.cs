using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform gunTransform; // Reference to the gun's transform
    public GameObject cursorIconPrefab; // Reference to the cursor icon prefab
    private GameObject cursorIconInstance; // Instance of the cursor icon

    private PlayerController playerController; // Reference to the PlayerController

    void Start()
    {
        // Instantiate the cursor icon and hide the system cursor
        cursorIconInstance = Instantiate(cursorIconPrefab);
        cursorIconInstance.transform.localScale *= 2.5f; // Make it larger
        Cursor.visible = false;

        playerController = PlayerController.instance; // Get reference to PlayerController
    }

    void Update()
    {
        UpdateCursorIconPosition();
        UpdateGunRotation(); // Update the gun's rotation each frame
    }

    void UpdateCursorIconPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Ensure the cursor icon stays on the same plane
        cursorIconInstance.transform.position = mousePosition;

        // Ensure the cursor icon is at the frontmost layer
        cursorIconInstance.GetComponent<SpriteRenderer>().sortingLayerName = "UI";
        cursorIconInstance.GetComponent<SpriteRenderer>().sortingOrder = 999;
    }

    void UpdateGunRotation()
    {
        // Get the mouse position in world space
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f; // Ensure we are on the same plane

        // Get the direction from the gun to the mouse
        Vector3 direction = mousePos - gunTransform.position;

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

    void OnDestroy()
    {
        // Show the system cursor when the script is destroyed
        Cursor.visible = true;
    }
}