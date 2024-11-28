using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using FMODUnity;
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



    private NewControls controls;
    private InputAction dpadLeftAction;
    private InputAction dpadRightAction;
    [SerializeField] private EventReference weaponSwitch;
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
        controls = new NewControls();
    }
    void OnEnable()
    {
        // Enable the input actions for DPad Left and DPad Right
        dpadLeftAction = controls.PlayerInput.DpadL;
        dpadRightAction = controls.PlayerInput.DpadR;

        dpadLeftAction.Enable();
        dpadRightAction.Enable();

        // Set up action listeners for DPad buttons
        dpadLeftAction.performed += ctx => SwitchWeaponLeft();
        dpadRightAction.performed += ctx => SwitchWeaponRight();
    }

    void OnDisable()
    {
        // Disable the actions when the script is disabled
        dpadLeftAction.Disable();
        dpadRightAction.Disable();
    }

    public void SwitchWeaponLeft()
    {
        int previousIndex = currentGunIndex - 1;
        if (previousIndex < 0)
        {
            previousIndex = equippedGunIndices.Count - 1; // Loop to the last weapon if we're at the first one
        }

        SwitchWeaponByIndex(previousIndex);
    }

    public void SwitchWeaponRight()
    {
        int nextIndex = currentGunIndex + 1;
        if (nextIndex >= equippedGunIndices.Count)
        {
            nextIndex = 0; // Loop to the first weapon if we're at the last one
        }

        SwitchWeaponByIndex(nextIndex);
    }

    private void SwitchWeaponByIndex(int index)
    {
        int weaponIndex = equippedGunIndices[index];
        AudioManager.instance.PlayOneShot(weaponSwitch, this.transform.position);
        // Check if the selected weapon index is valid and not locked
        if (weaponIndex >= 0 && weaponIndex < guns.Count && !guns[weaponIndex].locked)
        {
            // Stop reload sound of the current weapon
            if (currentGunIndex >= 0 && currentGunIndex < guns.Count)
            {
                Gun currentGun = guns[currentGunIndex].gunObject.GetComponent<Gun>();
                if (currentGun != null)
                {
                    currentGun.StopReloadSound();  // Stop the reload sound of the current weapon
                    currentGun.ResetReloadingState();  // Reset any reloading state
                }
            }

            // Update the current gun index to the new weapon
            currentGunIndex = weaponIndex;
            UpdateGunsVisibility();
            GunUIManager.instance.UpdateUI(); // Update UI when weapon is switched
        }
    }

    void Start()
    {
        // Ensure the first gun is always unlocked
        if (guns.Count > 0)
        {
            guns[0].locked = false; // Unlock the first gun
        }

        // DEBUG: Clear PlayerPrefs (use only temporarily)
        // PlayerPrefs.DeleteAll();

        LoadUnlockedGuns();
        LoadEquippedGunIndices();
        LoadPickedUpWeapons();
        LoadGunAmmoData();

        // Ensure pistol (first gun) is included by default
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
            if (Keyboard.current[(Key)(Key.Digit1 + i)].wasPressedThisFrame)
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
                AudioManager.instance.PlayOneShot(weaponSwitch, this.transform.position);
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
            if (gunIndex != -1 && gun.gunObject != null) // Ensure the gunObject is not null
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
    public void RestoreSomeGunAmmoData()
    {
        foreach (var gun in guns)
        {
            int gunIndex = GetGunIndex(gun.gunObject);
            if (gunIndex != -1)
            {
                Gun gunComponent = gun.gunObject.GetComponent<Gun>();
                if (gunComponent != null)
                {
                    // Calculate quarter of the maximum ammo to restore
                    int ammoToRestore = Mathf.CeilToInt(gunComponent.maxAmmo * 0.25f);  // 25% of the max ammo

                    gunComponent.bulletsRemaining = ammoToRestore;  // Restore the total bullets remaining
                    gunComponent.clipsRemaining = Mathf.FloorToInt(ammoToRestore / gunComponent.ammoPerClip);  // How many full clips
                    gunComponent.currentClipAmmo = Mathf.Min(gunComponent.ammoPerClip, ammoToRestore); // Restore ammo in the current clip

                    // Optional debug log to check the ammo restoration
                    Debug.Log($"Restored ammo for gun {gunIndex} - Bullets: {gunComponent.bulletsRemaining}, Clips: {gunComponent.clipsRemaining}, Current Clip Ammo: {gunComponent.currentClipAmmo}");
                }
            }
        }

        // Call this after updating ammo data to ensure UI updates and state is saved
        GunUIManager.instance.UpdateUI();
        Debug.Log("Dungeon regenerated. Ammo data restored (quarter of max ammo).");
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
        // First, lock all guns except the first one
        for (int i = 1; i < guns.Count; i++)
        {
            guns[i].locked = true;
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

        // Ensure the first gun is always unlocked
        if (guns.Count > 0)
        {
            guns[0].locked = false;
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