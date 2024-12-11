using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

public class PlayerGrenade : MonoBehaviour
{
    public static PlayerGrenade instance; // Singleton for UI updates

    public GameObject grenadePrefab;
    public Transform throwPoint;
    public int maxGrenades = 3;
    public float throwForce = 10f;
    private int currentGrenades;

    private PlayerController playerController;
    private NewControls inputActions;
    private InputAction grenadeAction;

    [SerializeField] private EventReference grenadePin;


    /// <summary>
    /// Initializes the instance of the PlayerGrenade singleton and sets up input actions.
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple PlayerGrenade instances found. Destroying the new one.");
            Destroy(gameObject);
        }
        // Initialize input actions
        inputActions = new NewControls();
        grenadeAction = inputActions.PlayerInput.Grenade;
    }

    /// <summary>
    /// Enables the grenade action and binds it to the ThrowGrenade method.
    /// </summary>
    void OnEnable()
    {
        grenadeAction.Enable();
        grenadeAction.performed += _ => ThrowGrenade();
    }

    /// <summary>
    /// Disables the grenade action and unbinds it from the ThrowGrenade method.
    /// </summary>
    void OnDisable()
    {
        grenadeAction.Disable();
        grenadeAction.performed -= _ => ThrowGrenade();
    }

    /// <summary>
    /// Initializes grenade count and updates the UI when the game starts.
    /// </summary>
    void Start()
    {
        currentGrenades = maxGrenades; // Set the initial grenade count
        playerController = PlayerController.instance;

        // Update UI initially
        GunUIManager.instance?.UpdateGrenadeUI(currentGrenades); 
    }

    /// <summary>
    /// Throws a grenade if the player has grenades available. 
    /// It instantiates the grenade and applies a force to throw it towards the mouse position.
    /// </summary>
    void ThrowGrenade()
    {
        if (currentGrenades > 0) 
        {
            AudioManager.instance.PlayOneShot(grenadePin, this.transform.position); // Play grenade pin sound effect

            GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);  // Instantiate the grenade prefab at the throw point
            // Get the mouse position in world space
            Vector3 mousePosition = Mouse.current.position.ReadValue(); 
            mousePosition.z = Camera.main.WorldToScreenPoint(throwPoint.position).z;
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            // Calculate the direction to throw the grenade
            Vector3 throwDirection = (targetPosition - throwPoint.position).normalized;
            // If the player is facing left, adjust the throw direction
            if (!playerController.IsFacingRight())
            {
                throwDirection.x = -Mathf.Abs(throwDirection.x);
            }
            // Apply a force to the grenade's rigidbody to throw it
            Rigidbody2D grenadeRb = grenade.GetComponent<Rigidbody2D>();
            if (grenadeRb != null)
            {
                grenadeRb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
            }
            // Decrease the grenade count after throwing
            currentGrenades--;

            // Update grenade count in UI
            GunUIManager.instance?.UpdateGrenadeUI(currentGrenades);
        }
    }

    /// <summary>
    /// Adds a grenade to the player's inventory if the maximum grenades haven't been reached.
    /// </summary>
    public void AddGrenade()
    {
        // Increase grenade count if it is less than max grenades
        if (currentGrenades < maxGrenades) 
        {
            currentGrenades++;
            GunUIManager.instance?.UpdateGrenadeUI(currentGrenades);
        }
    }

    /// <summary>
    /// Returns the current number of grenades the player has.
    /// </summary>
    public int GetCurrentGrenades()
    {
        return currentGrenades;
    }
}
