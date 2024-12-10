using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ObjectInteraction : MonoBehaviour
{
    public string promptMessage = "Press 'E' to interact"; // Message to display.
    public float interactionDistance = 2.0f; // Distance within which the interaction can occur.
    public Font customFont; // Assign mini_pixel-7 font in the Inspector.
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Offset to place the text above the player.

    private GameObject interactionPrompt; // Runtime-created UI Text object.
    private Canvas canvas; // Reference to the Canvas in your scene.
    private InputAction interactAction; // InputAction for interacting.
    private bool isPlayerNearby = false;
    private bool hasInteracted = false; // Flag to check if interaction has occurred.

    void OnEnable()
    {
        // Initialize and enable the input action
        var playerInput = new NewControls(); // Replace with your Input System action map
        interactAction = playerInput.PlayerInput.Interact;
        interactAction.Enable();
    }

    void OnDisable()
    {
        // Disable the input action when the object is disabled
        interactAction.Disable();
        DestroyInteractionPrompt(); // Ensure prompt is destroyed when the object is disabled
    }

    void Start()
    {
        // Search for the canvas named "InteractCanvas" in the scene
        GameObject canvasObject = GameObject.Find("InteractCanvas");
        if (canvasObject != null)
        {
            canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("InteractCanvas found, but it does not have a Canvas component.");
            }
        }
        else
        {
            Debug.LogError("No Canvas named 'InteractCanvas' found in the scene. Please add one.");
        }
    }

    void Update()
    {
        if (hasInteracted) return; // Skip all logic if interaction has already occurred.

        // Check the distance between the player and the object
        float distanceToPlayer = Vector3.Distance(PlayerController.instance.transform.position, transform.position);

        // Determine if the player is close enough for interaction
        isPlayerNearby = distanceToPlayer <= interactionDistance;

        if (isPlayerNearby && interactionPrompt == null)
        {
            CreateInteractionPrompt();
        }
        else if (!isPlayerNearby && interactionPrompt != null)
        {
            DestroyInteractionPrompt();
        }

        // Handle interaction when the player is nearby and the interact key is pressed
        if (isPlayerNearby && interactAction.triggered)
        {
            Interact();
        }
    }

    private void CreateInteractionPrompt()
    {
        if (canvas == null) return;

        // Create a new UI Text object
        interactionPrompt = new GameObject("InteractionPrompt");
        interactionPrompt.transform.SetParent(canvas.transform);

        // Add and configure the Text component
        Text textComponent = interactionPrompt.AddComponent<Text>();
        textComponent.text = promptMessage;
        textComponent.font = customFont; // Use the custom font
        textComponent.fontSize = 40;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;

        // Set up RectTransform for positioning
        RectTransform rectTransform = interactionPrompt.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        rectTransform.sizeDelta = new Vector2(300, 50);

        // Convert world position to canvas position and offset the prompt above the player
        Vector3 worldPosition = PlayerController.instance.transform.position + offset;
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        rectTransform.position = screenPosition;
    }

    private void DestroyInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            Destroy(interactionPrompt);
        }
    }

    private void Interact()
    {
        Debug.Log("Player interacted with the object!");
        hasInteracted = true; // Set flag to prevent further interactions.

        // Remove the interaction prompt after interacting
        DestroyInteractionPrompt();

        // Open the chest and give loot (Add your chest opening and loot logic here)
        OpenChest();

        // Do not destroy the chest
        // Destroy(gameObject); // Removed this line, chest won't be destroyed.
    }

    private void OpenChest()
    {
        // Your chest opening logic goes here
        // Example: Play animation, give loot to player, etc.
        Debug.Log("Chest opened! Loot is given.");
    }

    private void OnDestroy()
    {
        // Ensure the prompt is destroyed if the object is destroyed
        DestroyInteractionPrompt();
    }
}
