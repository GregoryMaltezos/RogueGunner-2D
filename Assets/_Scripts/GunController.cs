using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform gunTransform; // Reference to the gun's transform
    public GameObject cursorIconPrefab; // Reference to the cursor icon prefab
    private GameObject cursorIconInstance; // Instance of the cursor icon

    void Start()
    {
        // Instantiate the cursor icon and hide the system cursor
        cursorIconInstance = Instantiate(cursorIconPrefab);
        cursorIconInstance.transform.localScale *= 2.5f; // Make it larger
        Cursor.visible = false;
    }

    void Update()
    {
        UpdateCursorIconPosition();
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

    void OnDestroy()
    {
        // Show the system cursor when the script is destroyed
        Cursor.visible = true;
    }
}
