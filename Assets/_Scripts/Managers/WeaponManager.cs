using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GunInfo
{
    public GameObject gunObject;
    public bool locked = true; // All guns start as locked by default
}

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager instance; // Singleton instance
    public List<GunInfo> guns = new List<GunInfo>(); // List to hold all the guns (locked and unlocked)
    private List<int> equippedGunIndices = new List<int>(); // List to hold indices of currently equipped guns
    private List<int> pickedUpWeapons = new List<int>(); // List to track picked-up weapons
    private int currentGunIndex = 0; // Default to the first gun

    // PlayerPrefs keys
    public const string UnlockedGunsKey = "UnlockedGuns";
    public const string EquippedGunIndicesKey = "EquippedGunIndices";
    public const string PickedUpWeaponsKey = "PickedUpWeapons";
    public const string GunAmmoKey = "GunAmmo";
    public const string GunClipsKey = "GunClips";
    public const string GunClipAmmoKey = "GunClipAmmo";

    // Dictionaries to store ammo data
    private Dictionary<int, int> gunBulletsRemaining = new Dictionary<int, int>(); // Bullets remaining for each gun
    private Dictionary<int, int> gunClipsRemaining = new Dictionary<int, int>(); // Clips remaining for each gun
    private Dictionary<int, int> gunClipAmmo = new Dictionary<int, int>(); // Current clip ammo for each gun





    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Ensure the instance persists across scenes
        }
        else
        {
            Debug.LogWarning("Multiple instances of WeaponManager found. Destroying the new one.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // DEBUG: Clear PlayerPrefs (use only temporarily)
        // PlayerPrefs.DeleteAll();

        LoadUnlockedGuns();
        LoadEquippedGunIndices();
        LoadPickedUpWeapons();
        LoadGunAmmoData();

        // Ensure pistol is included by default
        if (guns.Count > 0 && !equippedGunIndices.Contains(0))
        {
            equippedGunIndices.Insert(0, 0);
        }

        // Set default gun index if no guns are equipped
        if (equippedGunIndices.Count > 0)
        {
            currentGunIndex = equippedGunIndices[0];
        }

        UpdateGunsVisibility();
        GunUIManager.instance.UpdateUI(); // Update UI at the start of the game
    }


    void Update()
    {
        for (int i = 0; i < 8; i++) // Check for weapon switch inputs from 1 to 8
        {
            if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" + (i + 1))))
            {
                SwitchWeapon(i);
                break;
            }
        }
    }

    public void SwitchWeapon(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedGunIndices.Count)
        {
            int weaponIndex = equippedGunIndices[slotIndex];

            // Check if the selected weapon index is valid and not locked
            if (weaponIndex >= 0 && weaponIndex < guns.Count && !guns[weaponIndex].locked)
            {
                // Reset the reloading state of the currently equipped gun
                if (currentGunIndex >= 0 && currentGunIndex < guns.Count)
                {
                    // Access the Gun component from the gunObject in GunInfo
                    Gun currentGun = guns[currentGunIndex].gunObject.GetComponent<Gun>();
                    if (currentGun != null)
                    {
                        currentGun.ResetReloadingState();
                    }
                }

                // Update the current gun index to the new weapon
                currentGunIndex = weaponIndex;
                UpdateGunsVisibility();
                GunUIManager.instance.UpdateUI(); // Update UI when weapon is switched
            }
        }
    }


    public void UnlockGun(int gunIndex)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count && guns[gunIndex].locked)
        {
            guns[gunIndex].locked = false;

            // Add the unlocked gun to equipped list if not already present
            if (!equippedGunIndices.Contains(gunIndex))
            {
                int insertIndex = equippedGunIndices.FindIndex(index => index == -1);
                if (insertIndex == -1) insertIndex = equippedGunIndices.Count;
                equippedGunIndices.Insert(insertIndex, gunIndex);
            }

            SaveUnlockedGuns();
            SaveEquippedGunIndices();
            SaveGunAmmoData();
            UpdateGunsVisibility();
        }
    }

    public List<int> GetPickedUpWeapons() => pickedUpWeapons;

    public void AddPickedUpWeapon(int gunIndex)
    {
        if (!pickedUpWeapons.Contains(gunIndex))
        {
            pickedUpWeapons.Add(gunIndex);
            SavePickedUpWeapons();
        }
    }

    public void ResetPickedUpWeapons()
    {
        pickedUpWeapons.Clear();
        SavePickedUpWeapons();
    }

    void SavePickedUpWeapons()
    {
        PlayerPrefs.SetString(PickedUpWeaponsKey, string.Join(",", pickedUpWeapons));
        PlayerPrefs.Save();
    }

    void LoadPickedUpWeapons()
    {
        if (PlayerPrefs.HasKey(PickedUpWeaponsKey))
        {
            string[] pickedUpWeaponsStr = PlayerPrefs.GetString(PickedUpWeaponsKey).Split(',');
            pickedUpWeapons.Clear();
            foreach (string weaponStr in pickedUpWeaponsStr)
            {
                if (int.TryParse(weaponStr, out int weaponIndex))
                {
                    pickedUpWeapons.Add(weaponIndex);
                }
            }
        }
    }



    public void PickupWeapon(int gunIndex)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count)
        {
            if (guns[gunIndex].locked)
            {
                // Unlock the weapon if it's locked
                UnlockGun(gunIndex);
            }
            else
            {
                // If already unlocked, restore ammo
                Gun gunComponent = guns[gunIndex].gunObject.GetComponent<Gun>();
                if (gunComponent != null)
                {
                    gunComponent.RestoreAmmo();
                }
            }

            // Optionally add the gun to the picked-up weapons list
            AddPickedUpWeapon(gunIndex);
            SwitchWeapon(equippedGunIndices.IndexOf(gunIndex));
            GunUIManager.instance.OnWeaponPickup(gunIndex);
        }
    }



    // Ammo Management Methods

    public void SetGunBulletsRemaining(int gunIndex, int bullets)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count)
        {
            gunBulletsRemaining[gunIndex] = bullets;
        }
    }

    public int GetGunBulletsRemaining(int gunIndex)
    {
        return gunBulletsRemaining.ContainsKey(gunIndex) ? gunBulletsRemaining[gunIndex] : guns[gunIndex].gunObject.GetComponent<Gun>().maxAmmo - guns[gunIndex].gunObject.GetComponent<Gun>().ammoPerClip;
    }

    public void SetGunClipsRemaining(int gunIndex, int clips)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count)
        {
            gunClipsRemaining[gunIndex] = clips;
        }
    }

    public int GetGunClipsRemaining(int gunIndex)
    {
        return gunClipsRemaining.ContainsKey(gunIndex) ? gunClipsRemaining[gunIndex] : (guns[gunIndex].gunObject.GetComponent<Gun>().maxAmmo - guns[gunIndex].gunObject.GetComponent<Gun>().ammoPerClip) / guns[gunIndex].gunObject.GetComponent<Gun>().ammoPerClip;
    }

    public void SetGunClipAmmo(int gunIndex, int clipAmmo)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count)
        {
            gunClipAmmo[gunIndex] = clipAmmo;
        }
    }

    public int GetGunClipAmmo(int gunIndex)
    {
        return gunClipAmmo.ContainsKey(gunIndex) ? gunClipAmmo[gunIndex] : guns[gunIndex].gunObject.GetComponent<Gun>().ammoPerClip;
    }

    // Save all guns' ammo before dungeon regeneration
    public void SaveAllGunAmmoData()
    {
        foreach (var gun in guns)
        {
            int gunIndex = GetGunIndex(gun.gunObject);
            if (gunIndex != -1)
            {
                Gun gunComponent = gun.gunObject.GetComponent<Gun>();
                if (gunComponent != null)
                {
                    SetGunBulletsRemaining(gunIndex, gunComponent.bulletsRemaining);
                    SetGunClipsRemaining(gunIndex, gunComponent.clipsRemaining);
                    SetGunClipAmmo(gunIndex, gunComponent.currentClipAmmo);
                    Debug.Log($"Saved Ammo Data - GunIndex: {gunIndex}, BulletsRemaining: {gunComponent.bulletsRemaining}, ClipsRemaining: {gunComponent.clipsRemaining}, CurrentClipAmmo: {gunComponent.currentClipAmmo}");
                }
            }
        }

        SaveGunAmmoData();
    }

    // Restore all guns' ammo after dungeon regeneration
    public void RestoreAllGunAmmoData()
    {
        foreach (var gun in guns)
        {
            int gunIndex = GetGunIndex(gun.gunObject);
            if (gunIndex != -1)
            {
                Gun gunComponent = gun.gunObject.GetComponent<Gun>();
                if (gunComponent != null)
                {
                    gunComponent.bulletsRemaining = GetGunBulletsRemaining(gunIndex);
                    gunComponent.clipsRemaining = GetGunClipsRemaining(gunIndex);
                    gunComponent.currentClipAmmo = GetGunClipAmmo(gunIndex);
                    // Debug.Log($"Restored Ammo Data - GunIndex: {gunIndex}, BulletsRemaining: {gunComponent.bulletsRemaining}, ClipsRemaining: {gunComponent.clipsRemaining}, CurrentClipAmmo: {gunComponent.currentClipAmmo}");
                }
            }
        }
    }

    // Save gun ammo data to PlayerPrefs
    void SaveGunAmmoData()
    {
        // Serialize dictionaries to strings
        string bulletsStr = SerializeDictionary(gunBulletsRemaining);
        string clipsStr = SerializeDictionary(gunClipsRemaining);
        string clipAmmoStr = SerializeDictionary(gunClipAmmo);

        PlayerPrefs.SetString(GunAmmoKey, bulletsStr);
        PlayerPrefs.SetString(GunClipsKey, clipsStr);
        PlayerPrefs.SetString(GunClipAmmoKey, clipAmmoStr);
        PlayerPrefs.Save();

        Debug.Log("Gun ammo data saved.");
    }

    // Load gun ammo data from PlayerPrefs
    void LoadGunAmmoData()
    {
        if (PlayerPrefs.HasKey(GunAmmoKey))
        {
            gunBulletsRemaining = DeserializeDictionary(PlayerPrefs.GetString(GunAmmoKey));
            gunClipsRemaining = DeserializeDictionary(PlayerPrefs.GetString(GunClipsKey));
            gunClipAmmo = DeserializeDictionary(PlayerPrefs.GetString(GunClipAmmoKey));

            Debug.Log("Gun ammo data loaded.");
        }
        else
        {
            // Initialize default ammo if no data is saved
            foreach (var gun in guns)
            {
                int gunIndex = GetGunIndex(gun.gunObject);
                if (gunIndex != -1)
                {
                    gunBulletsRemaining[gunIndex] = gun.gunObject.GetComponent<Gun>().maxAmmo - gun.gunObject.GetComponent<Gun>().ammoPerClip;
                    gunClipsRemaining[gunIndex] = (gun.gunObject.GetComponent<Gun>().maxAmmo - gun.gunObject.GetComponent<Gun>().ammoPerClip) / gun.gunObject.GetComponent<Gun>().ammoPerClip;
                    gunClipAmmo[gunIndex] = gun.gunObject.GetComponent<Gun>().ammoPerClip;
                }
            }

            SaveGunAmmoData();
            Debug.Log("Initialized default gun ammo data.");
        }
        GunUIManager.instance.UpdateUI();
    }

    // Helper methods to serialize and deserialize dictionaries
    string SerializeDictionary(Dictionary<int, int> dict)
    {
        List<string> entries = new List<string>();
        foreach (var pair in dict)
        {
            entries.Add($"{pair.Key}:{pair.Value}");
        }
        return string.Join(",", entries);
    }

    Dictionary<int, int> DeserializeDictionary(string data)
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();
        string[] entries = data.Split(',');
        foreach (string entry in entries)
        {
            string[] keyValue = entry.Split(':');
            if (keyValue.Length == 2 && int.TryParse(keyValue[0], out int key) && int.TryParse(keyValue[1], out int value))
            {
                dict[key] = value;
            }
        }
        return dict;
    }

    public int GetGunIndex(GameObject gunObject)
    {
        for (int i = 0; i < guns.Count; i++)
        {
            if (guns[i].gunObject == gunObject)
            {
                return i;
            }
        }
        return -1; // Return -1 if gun is not found
    }


    public int GetCurrentGunIndex()
    {
        return currentGunIndex;
    }


    void UpdateGunsVisibility()
    {
        for (int i = 0; i < guns.Count; i++)
        {
            guns[i].gunObject.SetActive(i == currentGunIndex);
        }
    }

    void SaveUnlockedGuns()
    {
        List<int> unlockedGuns = new List<int>();
        for (int i = 0; i < guns.Count; i++)
        {
            if (!guns[i].locked)
            {
                unlockedGuns.Add(i);
            }
        }
        PlayerPrefs.SetString(UnlockedGunsKey, string.Join(",", unlockedGuns));
        PlayerPrefs.Save();
    }

    void LoadUnlockedGuns()
    {
        // First, lock all guns
        foreach (var gun in guns)
        {
            gun.locked = true;
        }
        // Then, unlock the guns from the saved data
        if (PlayerPrefs.HasKey(UnlockedGunsKey))
        {
            string[] unlockedGunsStr = PlayerPrefs.GetString(UnlockedGunsKey).Split(',');
            foreach (string gunStr in unlockedGunsStr)
            {
                if (int.TryParse(gunStr, out int gunIndex) && gunIndex >= 0 && gunIndex < guns.Count)
                {
                    guns[gunIndex].locked = false;
                }
            }
        }
    }


    void SaveEquippedGunIndices()
    {
        PlayerPrefs.SetString(EquippedGunIndicesKey, string.Join(",", equippedGunIndices));
        PlayerPrefs.Save();
    }

    void LoadEquippedGunIndices()
    {
        if (PlayerPrefs.HasKey(EquippedGunIndicesKey))
        {
            string[] equippedGunsStr = PlayerPrefs.GetString(EquippedGunIndicesKey).Split(',');
            equippedGunIndices.Clear();
            foreach (string gunStr in equippedGunsStr)
            {
                if (int.TryParse(gunStr, out int gunIndex))
                {
                    equippedGunIndices.Add(gunIndex);
                }
            }
        }
    }

    public Gun GetGun(int gunIndex)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count)
        {
            return guns[gunIndex].gunObject.GetComponent<Gun>();
        }
        return null;
    }



    // Call this when the dungeon is regenerated to restore ammo data
    public void OnDungeonGenerated()
    {
        RestoreAllGunAmmoData();
        Debug.Log("Dungeon regenerated. Ammo data restored.");
    }
}