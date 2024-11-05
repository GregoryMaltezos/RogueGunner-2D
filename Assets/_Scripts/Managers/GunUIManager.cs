using UnityEngine;
using TMPro;
using System.Collections; // Import the TextMesh Pro namespace

public class GunUIManager : MonoBehaviour
{
    // Reference to the ammo text UI element
    public TextMeshProUGUI ammoText; // This will be assigned automatically
    public static GunUIManager instance;

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
    }


    void Start()
    {
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
}
