using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using FMODUnity;
[System.Serializable]
public class GunInfo
{
    public GameObject gunObject; // The gun's GameObject.
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

    /// <summary>
    /// Called when the script is first initialized. Sets up the singleton instance and ensures it persists across scenes.
    /// </summary>
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

    /// <summary>
    /// Enables the input actions for DPad buttons when the script is enabled. Sets up listeners for DPad button presses.
    /// </summary>
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
    /// <summary>
    /// Disables the input actions for DPad buttons when the script is disabled.
    /// </summary>
    void OnDisable()
    {
       
        dpadLeftAction.Disable();
        dpadRightAction.Disable();
    }

    /// <summary>
    /// Switch to the previous weapon (left). Loops back to the last weapon if we're at the first one.
    /// </summary>
    public void SwitchWeaponLeft()
    {
        int previousIndex = currentGunIndex - 1;
        if (previousIndex < 0)
        {
            previousIndex = equippedGunIndices.Count - 1; // Loop to the last weapon if we're at the first one
        }

        SwitchWeaponByIndex(previousIndex);
    }
    /// <summary>
    /// Switch to the next weapon (right). Loops back to the first weapon if we're at the last one.
    /// </summary>
    public void SwitchWeaponRight()
    {
        int nextIndex = currentGunIndex + 1;
        if (nextIndex >= equippedGunIndices.Count)
        {
            nextIndex = 0; // Loop to the first weapon if we're at the last one
        }

        SwitchWeaponByIndex(nextIndex);
    }

    /// <summary>
    /// Switches the weapon by the given index and updates visibility and UI. Plays weapon switch sound.
    /// </summary>
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

    /// <summary>
    /// Called at the start of the game. Initializes and loads the necessary data for guns, ammo, and equipment.
    /// Ensures the first gun is always unlocked and equipped by default.
    /// </summary>
    void Start()
    {
        // Ensure the first gun is always unlocked
        if (guns.Count > 0)
        {
            guns[0].locked = false; // Unlock the first gun
        }

        // DEBUG: Clear PlayerPrefs 
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


    /// <summary>
    /// check for weapon switch inputs (1 to 8 keys).
    /// If a weapon switch key is pressed, switches the weapon accordingly.
    /// </summary>
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

    /// <summary>
    /// Switches the weapon to the one in the specified slot.
    /// If the weapon is unlocked, it becomes the new current weapon, and UI is updated.
    /// </summary>
    /// <param name="slotIndex">The index of the slot to switch to (0 to 7).</param>
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

    /// <summary>
    /// Unlocks the specified gun and adds it to the equipped guns if not already equipped.
    /// Saves the updated state to PlayerPrefs.
    /// </summary>
    /// <param name="gunIndex">The index of the gun to unlock.</param>
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

    /// <summary>
    /// Gets the list of picked-up weapons.
    /// </summary>
    /// <returns>A list of indices of weapons that have been picked up.</returns>
    public List<int> GetPickedUpWeapons() => pickedUpWeapons;

    /// <summary>
    /// Adds a weapon to the list of picked-up weapons if it is not already in the list.
    /// </summary>
    /// <param name="gunIndex">The index of the weapon to add.</param>
    public void AddPickedUpWeapon(int gunIndex)
    {
        if (!pickedUpWeapons.Contains(gunIndex))
        {
            pickedUpWeapons.Add(gunIndex);
            SavePickedUpWeapons();
        }
    }
    /// <summary>
    /// Clears the list of picked-up weapons and saves the empty list to PlayerPrefs.
    /// </summary>
    public void ResetPickedUpWeapons()
    {
        pickedUpWeapons.Clear();
        pickedUpWeapons.Clear();
        SavePickedUpWeapons();
    }
    /// <summary>
    /// Saves the list of picked-up weapons to PlayerPrefs as a comma-separated string.
    /// </summary>
    void SavePickedUpWeapons()
    {
        PlayerPrefs.SetString(PickedUpWeaponsKey, string.Join(",", pickedUpWeapons));
        PlayerPrefs.Save();
    }
    /// <summary>
    /// Loads the list of picked-up weapons from PlayerPrefs and updates the list accordingly.
    /// </summary>
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


    /// <summary>
    /// Handles the process when a weapon is picked up. If the weapon is locked, it gets unlocked.
    /// If it's unlocked, ammo is restored. The weapon is then switched to and UI is updated.
    /// </summary>
    /// <param name="gunIndex">The index of the weapon being picked up.</param>
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

            // add the gun to the picked-up weapons list
            AddPickedUpWeapon(gunIndex);
            SwitchWeapon(equippedGunIndices.IndexOf(gunIndex));
            GunUIManager.instance.OnWeaponPickup(gunIndex);
        }
    }



    /// <summary>
    /// Sets the number of remaining bullets for a specific gun.
    /// </summary>
    /// <param name="gunIndex">The index of the gun to set bullets remaining for.</param>
    /// <param name="bullets">The number of bullets to set.</param>

    public void SetGunBulletsRemaining(int gunIndex, int bullets)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count)
        {
            gunBulletsRemaining[gunIndex] = bullets;
        }
    }

    /// <summary>
    /// Gets the number of remaining bullets for a specific gun.
    /// If no specific data is available, it calculates the default ammo amount.
    /// </summary>
    /// <param name="gunIndex">The index of the gun to get bullets remaining for.</param>
    /// <returns>The number of remaining bullets.</returns>
    public int GetGunBulletsRemaining(int gunIndex)
    {
        return gunBulletsRemaining.ContainsKey(gunIndex) ? gunBulletsRemaining[gunIndex] : guns[gunIndex].gunObject.GetComponent<Gun>().maxAmmo - guns[gunIndex].gunObject.GetComponent<Gun>().ammoPerClip;
    }
    /// <summary>
    /// Sets the number of remaining clips for a specific gun.
    /// </summary>
    /// <param name="gunIndex">The index of the gun to set clips remaining for.</param>
    /// <param name="clips">The number of clips to set.</param>
    public void SetGunClipsRemaining(int gunIndex, int clips)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count)
        {
            gunClipsRemaining[gunIndex] = clips;
        }
    }
    /// <summary>
    /// Gets the number of remaining clips for a specific gun.
    /// If no specific data is available, it calculates the default clip count.
    /// </summary>
    /// <param name="gunIndex">The index of the gun to get clips remaining for.</param>
    /// <returns>The number of remaining clips.</returns>
    public int GetGunClipsRemaining(int gunIndex)
    {
        return gunClipsRemaining.ContainsKey(gunIndex) ? gunClipsRemaining[gunIndex] : (guns[gunIndex].gunObject.GetComponent<Gun>().maxAmmo - guns[gunIndex].gunObject.GetComponent<Gun>().ammoPerClip) / guns[gunIndex].gunObject.GetComponent<Gun>().ammoPerClip;
    }
    /// <summary>
    /// Sets the current clip ammo for a specific gun.
    /// </summary>
    /// <param name="gunIndex">The index of the gun to set current clip ammo for.</param>
    /// <param name="clipAmmo">The amount of ammo in the current clip.</param>
    public void SetGunClipAmmo(int gunIndex, int clipAmmo)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count)
        {
            gunClipAmmo[gunIndex] = clipAmmo;
        }
    }
    /// <summary>
    /// Gets the current clip ammo for a specific gun.
    /// If no specific data is available, it returns the default clip ammo value.
    /// </summary>
    /// <param name="gunIndex">The index of the gun to get current clip ammo for.</param>
    /// <returns>The current ammo in the clip.</returns>
    public int GetGunClipAmmo(int gunIndex)
    {
        return gunClipAmmo.ContainsKey(gunIndex) ? gunClipAmmo[gunIndex] : guns[gunIndex].gunObject.GetComponent<Gun>().ammoPerClip;
    }


    /// <summary>
    /// Saves the ammo data for all guns before the dungeon regeneration.
    /// This method ensures that all ammo states are saved for later restoration.
    /// </summary>
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


    /// <summary>
    /// Restores the ammo data for all guns after the dungeon regeneration.
    /// This ensures that the guns' ammo states are correctly restored based on the saved data.
    /// </summary>
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

    /// <summary>
    /// Restores a portion of the ammo (25% of max ammo) for all guns.
    /// This helps in resetting the ammo state for gameplay balance.
    /// </summary>
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

                    
                    Debug.Log($"Restored ammo for gun {gunIndex} - Bullets: {gunComponent.bulletsRemaining}, Clips: {gunComponent.clipsRemaining}, Current Clip Ammo: {gunComponent.currentClipAmmo}");
                }
            }
        }

        //called after updating ammo data to ensure UI updates and state is saved
        GunUIManager.instance.UpdateUI();
        Debug.Log("Dungeon regenerated. Ammo data restored (quarter of max ammo).");
    }

    /// <summary>
    /// Saves the current ammo data for all guns to PlayerPrefs, ensuring the data persists between game sessions.
    /// </summary>
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

    /// <summary>
    /// Loads the ammo data for all guns from PlayerPrefs, ensuring the state is restored for the current session.
    /// </summary>
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

    /// <summary>
    /// Serializes a dictionary of gun ammo data to a string for storage.
    /// </summary>
    /// <param name="dict">The dictionary to serialize.</param>
    /// <returns>A serialized string representation of the dictionary.</returns>
    string SerializeDictionary(Dictionary<int, int> dict)
    {
        List<string> entries = new List<string>();
        foreach (var pair in dict)
        {
            entries.Add($"{pair.Key}:{pair.Value}");
        }
        return string.Join(",", entries);
    }
    /// <summary>
    /// Deserializes a string into a dictionary of gun ammo data.
    /// </summary>
    /// <param name="data">The serialized string representation of the dictionary.</param>
    /// <returns>The deserialized dictionary.</returns>
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
    /// <summary>
    /// Gets the index of a gun in the guns list.
    /// </summary>
    /// <param name="gunObject">The gun object to search for.</param>
    /// <returns>The index of the gun in the guns list, or -1 if not found.</returns>
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

    /// <summary>
    /// Retrieves the current gun index.
    /// </summary>
    /// <returns>The index of the current gun.</returns>
    public int GetCurrentGunIndex()
    {
        return currentGunIndex;
    }

    /// <summary>
    /// Updates the visibility of all guns based on the current equipped gun index.
    /// </summary>
    void UpdateGunsVisibility()
    {
        for (int i = 0; i < guns.Count; i++)
        {
            guns[i].gunObject.SetActive(i == currentGunIndex);
        }
    }
    /// <summary>
    /// Saves the indices of unlocked guns to PlayerPrefs for persistence across game sessions.
    /// </summary>
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
    /// <summary>
    /// Loads the indices of unlocked guns from PlayerPrefs and updates the gun states accordingly.
    /// </summary>
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

    /// <summary>
    /// Saves the indices of equipped guns to PlayerPrefs for persistence across game sessions.
    /// </summary>
    void SaveEquippedGunIndices()
    {
        PlayerPrefs.SetString(EquippedGunIndicesKey, string.Join(",", equippedGunIndices));
        PlayerPrefs.Save();
    }
    /// <summary>
    /// Loads the indices of equipped guns from PlayerPrefs.
    /// </summary>
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

    /// <summary>
    /// Retrieves the <see cref="Gun"/> component of a specific gun by its index.
    /// </summary>
    /// <param name="gunIndex">The index of the gun to retrieve.</param>
    /// <returns>The <see cref="Gun"/> component of the specified gun, or null if not found.</returns>
    public Gun GetGun(int gunIndex)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count)
        {
            return guns[gunIndex].gunObject.GetComponent<Gun>();
        }
        return null;
    }



    /// <summary>
    /// Called when the dungeon is regenerated to restore the ammo data for all guns.
    /// </summary>
    public void OnDungeonGenerated()
    {
        RestoreAllGunAmmoData();
        Debug.Log("Dungeon regenerated. Ammo data restored.");
    }
}