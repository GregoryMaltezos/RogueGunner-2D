using UnityEngine;
using TMPro; // Import the TextMesh Pro namespace

public class GunUIManager : MonoBehaviour
{
    public static GunUIManager instance; // Singleton instance

    // Reference to the ammo text UI element
    public TextMeshProUGUI ammoText; // Make sure to assign this in the Inspector

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

    // Method to update the UI with the currently equipped gun's ammo
    public void UpdateUI()
    {
        int currentGunIndex = WeaponManager.instance.GetCurrentGunIndex();
        Gun currentGun = WeaponManager.instance.GetGun(currentGunIndex);

        if (currentGun != null)
        {
            // Directly access the ammo properties from the current gun instance
            int clipAmmo = currentGun.currentClipAmmo;
            int bulletsRemaining = currentGun.bulletsRemaining;

            // Check if the gun has infinite ammo directly from the gun instance
            if (currentGun.infiniteAmmo)
            {
                ammoText.text = $"{clipAmmo}/∞"; // Display as currentClipAmmo/∞
            }
            else
            {
                ammoText.text = $"{clipAmmo}/{bulletsRemaining}"; // Normal display
            }

         //   Debug.Log($"Updating UI: Current Clip Ammo: {clipAmmo}, Total Bullets Remaining: {bulletsRemaining}");
        }
        else
        {
            Debug.LogWarning("Current gun is null. Unable to update UI.");
        }
    }

    // Optional: Method to explicitly handle weapon pickups and update UI
    public void OnWeaponPickup(int gunIndex)
    {
        // Call UpdateUI to ensure the correct ammo is displayed for the current weapon
        UpdateUI();
    }
}
