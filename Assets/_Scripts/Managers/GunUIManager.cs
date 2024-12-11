using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GunUIManager : MonoBehaviour
{
    public TextMeshProUGUI ammoText; // UI element for ammo
    public TextMeshProUGUI grenadeText; // UI element for grenades
    public static GunUIManager instance; // Singleton instance to access GunUIManager across scenes

    private GameObject playerAmmo; // Reference to the player ammo UI object

    /// <summary>
    /// Initializes the GunUIManager instance and ensures it's unique across scenes.
    /// </summary>
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Multiple instances of GunUIManager found. Destroying the new one.");
            Destroy(gameObject);
        }

        playerAmmo = transform.Find("PlayerAmmo")?.gameObject; // Find the PlayerAmmo object in the scene

        SceneManager.sceneLoaded += OnSceneLoaded;  // Register to listen for scene load events
    }
    /// <summary>
    /// Starts the GunUIManager, adjusts ammo visibility, and initializes UI components.
    /// </summary>
    void Start()
    {
        AdjustPlayerAmmoVisibility();

        GameObject playerHPObject = GameObject.Find("PlayerHP"); // Find the PlayerHP object in the scene
        if (playerHPObject != null)
        {
            Transform panelTransform = playerHPObject.transform.Find("Panel"); // Find the Panel under PlayerHP
            if (panelTransform != null)
            {
                ammoText = panelTransform.Find("AmmoText")?.GetComponent<TextMeshProUGUI>(); // Find AmmoText
                grenadeText = panelTransform.Find("GrenadeText")?.GetComponent<TextMeshProUGUI>(); // Find GrenadeText
                 // Check if ammoText and grenadeText were found
                if (ammoText == null) Debug.LogError("AmmoText not found or missing TextMeshProUGUI component.");
                if (grenadeText == null) Debug.LogError("GrenadeText not found or missing TextMeshProUGUI component.");
            }
        }
        // Wait for WeaponManager to be ready before initializing UI
        StartCoroutine(InitializeUI());
    }

    /// <summary>
    /// Initializes the UI when WeaponManager is available, ensuring UI updates are done only when WeaponManager is ready.
    /// </summary>
    private IEnumerator InitializeUI()
    {
        while (WeaponManager.instance == null) // Wait until WeaponManager is initialized
        {
            yield return null;
        }
        UpdateUI(); // Update the UI after WeaponManager is ready
    }

    /// <summary>
    /// Updates the ammo and grenade UI based on the current state of the player’s weapons and grenades.
    /// </summary>
    public void UpdateUI()
    {
        if (WeaponManager.instance == null)
        {
            Debug.LogWarning("WeaponManager instance is null. Cannot update UI.");
            return;
        }

        int currentGunIndex = WeaponManager.instance.GetCurrentGunIndex(); // Get the current gun index
        Gun currentGun = WeaponManager.instance.GetGun(currentGunIndex); // Get the current gun

        if (currentGun != null)
        {
            int clipAmmo = currentGun.currentClipAmmo; // Ammo in the current clip
            int bulletsRemaining = currentGun.bulletsRemaining; // Remaining ammo

            ammoText.text = currentGun.infiniteAmmo ? $"{clipAmmo}/∞" : $"{clipAmmo}/{bulletsRemaining}"; // Update ammo text depending on whether the gun has infinite ammo
        }

  
        if (grenadeText != null && PlayerGrenade.instance != null) // Update grenade count if grenadeText and PlayerGrenade instance are valid
        {
            grenadeText.text = $"{PlayerGrenade.instance.GetCurrentGrenades()}"; // Display current grenades count
        }
    }

    /// <summary>
    /// Called when a weapon is picked up. Updates the UI to reflect the new weapon.
    /// </summary>
    /// <param name="gunIndex">The index of the newly picked-up weapon.</param>
    public void OnWeaponPickup(int gunIndex)
    {
        UpdateUI(); // Update the UI to reflect the newly picked-up weapon
    }

    /// <summary>
    /// Adjusts the visibility of the ammo UI based on the current scene.
    /// </summary>
    private void AdjustPlayerAmmoVisibility()
    {
        string currentSceneName = SceneManager.GetActiveScene().name; 

        if (currentSceneName == "MainMenu") // Hide ammo UI in the MainMenu scene
        {
            DisablePlayerAmmo();
        }
        else // Show ammo UI in all other scenes
        {
            EnablePlayerAmmo();
        }
    }
    /// <summary>
    /// Enables the player’s ammo UI if it’s not already enabled.
    /// </summary>
    private void EnablePlayerAmmo()
    {
        if (playerAmmo != null && !playerAmmo.activeSelf)
        {
            playerAmmo.SetActive(true);
        }
    }
    /// <summary>
    /// Disables the player’s ammo UI if it’s currently enabled.
    /// </summary>
    private void DisablePlayerAmmo()
    {
        if (playerAmmo != null && playerAmmo.activeSelf)
        {
            playerAmmo.SetActive(false);
        }
    }
    /// <summary>
    /// Called when a scene is loaded to adjust ammo visibility based on the scene.
    /// </summary>
    /// <param name="scene">The loaded scene.</param>
    /// <param name="mode">The scene load mode.</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AdjustPlayerAmmoVisibility();
    }
    /// <summary>
    /// Unsubscribes from the sceneLoaded event when the GunUIManager is destroyed.
    /// </summary>
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Updates the grenade UI with the current grenade count.
    /// </summary>
    /// <param name="currentGrenades">The current number of grenades the player has.</param>
    public void UpdateGrenadeUI(int currentGrenades)
    {
        if (grenadeText != null)
        {
            grenadeText.text = $"{currentGrenades}"; // Update the grenade text
        }
    }
}
