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

        inputActions = new NewControls();
        grenadeAction = inputActions.PlayerInput.Grenade;
    }

    void OnEnable()
    {
        grenadeAction.Enable();
        grenadeAction.performed += _ => ThrowGrenade();
    }

    void OnDisable()
    {
        grenadeAction.Disable();
        grenadeAction.performed -= _ => ThrowGrenade();
    }

    void Start()
    {
        currentGrenades = maxGrenades;
        playerController = PlayerController.instance;

        // Update UI initially
        GunUIManager.instance?.UpdateGrenadeUI(currentGrenades);
    }

    void ThrowGrenade()
    {
        if (currentGrenades > 0)
        {
            AudioManager.instance.PlayOneShot(grenadePin, this.transform.position);

            GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            mousePosition.z = Camera.main.WorldToScreenPoint(throwPoint.position).z;
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            Vector3 throwDirection = (targetPosition - throwPoint.position).normalized;

            if (!playerController.IsFacingRight())
            {
                throwDirection.x = -Mathf.Abs(throwDirection.x);
            }

            Rigidbody2D grenadeRb = grenade.GetComponent<Rigidbody2D>();
            if (grenadeRb != null)
            {
                grenadeRb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
            }

            currentGrenades--;

            // Update grenade count in UI
            GunUIManager.instance?.UpdateGrenadeUI(currentGrenades);
        }
    }

    public void AddGrenade()
    {
        if (currentGrenades < maxGrenades)
        {
            currentGrenades++;
            GunUIManager.instance?.UpdateGrenadeUI(currentGrenades);
        }
    }

    public int GetCurrentGrenades()
    {
        return currentGrenades;
    }
}
