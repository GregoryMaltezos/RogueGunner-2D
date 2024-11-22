using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
public class PlayerGrenade : MonoBehaviour
{
    public GameObject grenadePrefab;
    public Transform throwPoint;  // The point from which the grenade is thrown
    public int maxGrenades = 3;
    public float throwForce = 10f;  // The force applied to the grenade
    private int currentGrenades;

    private PlayerController playerController; // Reference to the PlayerController script

    private NewControls inputActions; // Reference to the NewControls input asset
    private InputAction grenadeAction;
    [SerializeField] private EventReference grenadePin;
    void Awake()
    {
        inputActions = new NewControls(); // Create an instance of the NewControls asset
        grenadeAction = inputActions.PlayerInput.Grenade;
    }
    void OnEnable()
    {
        // Enable the grenade action when the object is enabled
        grenadeAction.Enable();
        grenadeAction.performed += _ => ThrowGrenade(); // Subscribe to the performed event, triggering ThrowGrenade
    }

    void OnDisable()
    {
        // Disable the grenade action when the object is disabled to avoid memory leaks
        grenadeAction.Disable();
        grenadeAction.performed -= _ => ThrowGrenade(); // Unsubscribe from the performed event
    }
    void Start()
    {
        currentGrenades = maxGrenades;
        playerController = PlayerController.instance; // Get reference to PlayerController
    }

    void Update()
    {
        // No longer need to check for the key press here, it's handled by the Input Action
    }

    void ThrowGrenade()
    {
        if (currentGrenades > 0)
        {
            AudioManager.instance.PlayOneShot(grenadePin, this.transform.position);
            // Create the grenade at the throw point
            GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);

            // Get the position of the mouse in world space
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            mousePosition.z = Camera.main.WorldToScreenPoint(throwPoint.position).z;  // Maintain correct z distance
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            // Calculate the direction from the throw point to the mouse position
            Vector3 throwDirection = (targetPosition - throwPoint.position).normalized;

            // Check if the player is facing left and adjust the throw direction accordingly
            if (!playerController.IsFacingRight())
            {
                // If the player is facing left, reverse the throw direction's x component
                throwDirection.x = -Mathf.Abs(throwDirection.x); // Make sure x is negative
            }

            // Apply force to the grenade in the calculated throw direction
            Rigidbody2D grenadeRb = grenade.GetComponent<Rigidbody2D>();
            if (grenadeRb != null)
            {
                grenadeRb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse); // Use Impulse for immediate force
            }

            currentGrenades--;
        }
    }

    public void AddGrenade()
    {
        if (currentGrenades < maxGrenades)
        {
            currentGrenades++;
        }
    }

}
