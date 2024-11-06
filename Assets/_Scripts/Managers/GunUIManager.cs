using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GunUIManager : MonoBehaviour
{
    // Reference to the ammo text UI element
    public TextMeshProUGUI ammoText; // This will be assigned automatically
    public static GunUIManager instance;

    // Reference to PlayerAmmo UI element
    private GameObject playerAmmo;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Debug.LogWarning("Multiple instances of GunUIManager found. Destroying the new one.");
            Destroy(gameObject);
        }

        // Find PlayerAmmo in the GunUIManager
        playerAmmo = transform.Find("PlayerAmmo")?.gameObject;

        // Register to handle scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Adjust visibility when the game starts
        AdjustPlayerAmmoVisibility();

        // Automatically assign ammoText from PlayerHP > Panel > AmmoText
        GameObject playerHPObject = GameObject.Find("PlayerHP");
        if (playerHPObject != null)
        {
            Transform panelTransform = playerHPObject.transform.Find("Panel"); // Find the Panel
            if (panelTransform != null)
            {
                Transform ammoTextTransform = panelTransform.Find("AmmoText"); // Find the AmmoText
                if (ammoTextTransform != null)
                {
                    ammoText = ammoTextTransform.GetComponent<TextMeshProUGUI>(); // Assign the TextMeshProUGUI component
                    if (ammoText == null)
                    {
                        Debug.LogError("AmmoText does not have a TextMeshProUGUI component.");
                    }
                    else
                    {
                        Debug.Log("ammoText assigned successfully.");
                    }
                }
                else
                {
                    Debug.LogError("AmmoText grandchild not found under Panel.");
                }
            }
        }
        else
        {
            Debug.LogError("PlayerHP object not found in the hierarchy.");
        }

        // Initialize UI
        StartCoroutine(InitializeUI());
    }

    private IEnumerator InitializeUI()
    {
        while (WeaponManager.instance == null)
        {
            yield return null; // Wait until the next frame
        }
        UpdateUI(); // Now it's safe to call
    }

    public void UpdateUI()
    {
        // Check if WeaponManager.instance is available
        if (WeaponManager.instance == null)
        {
            Debug.LogWarning("WeaponManager instance is null. Cannot update UI.");
            return;
        }

        // Ensure that the current gun index is valid
        int currentGunIndex = WeaponManager.instance.GetCurrentGunIndex();
        Gun currentGun = WeaponManager.instance.GetGun(currentGunIndex);

        if (currentGun != null)
        {
            int clipAmmo = currentGun.currentClipAmmo;
            int bulletsRemaining = currentGun.bulletsRemaining;

            if (currentGun.infiniteAmmo)
            {
                ammoText.text = $"{clipAmmo}/∞";
            }
            else
            {
                ammoText.text = $"{clipAmmo}/{bulletsRemaining}";
            }
        }
        else
        {
            Debug.LogWarning("Current gun is null. Unable to update UI.");
        }
    }

    public void OnWeaponPickup(int gunIndex)
    {
        UpdateUI();
    }

    // Adjust PlayerAmmo visibility based on the scene
    private void AdjustPlayerAmmoVisibility()
    {
        // Check the current scene
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "MainMenu")
        {
            // Disable PlayerAmmo in the Main Menu
            DisablePlayerAmmo();
        }
        else
        {
            // Enable PlayerAmmo in any game scene
            EnablePlayerAmmo();
        }
    }

    // Enable PlayerAmmo when in game scenes
    private void EnablePlayerAmmo()
    {
        if (playerAmmo != null && !playerAmmo.activeSelf)
        {
            playerAmmo.SetActive(true);
            Debug.Log("PlayerAmmo enabled in the game scene.");
        }
        else
        {
            Debug.LogWarning("PlayerAmmo is null or already enabled.");
        }
    }

    // Disable PlayerAmmo when in the Main Menu
    private void DisablePlayerAmmo()
    {
        if (playerAmmo != null && playerAmmo.activeSelf)
        {
            playerAmmo.SetActive(false);
            Debug.Log("PlayerAmmo disabled in the Main Menu.");
        }
        else
        {
            Debug.LogWarning("PlayerAmmo is null or already disabled.");
        }
    }

    // Handle scene loaded events to adjust visibility based on the scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AdjustPlayerAmmoVisibility();
    }

    // Clean up the event subscription when this object is destroyed
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
