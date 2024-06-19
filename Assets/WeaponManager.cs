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
    private int currentGunIndex = 0; // Default to the first gun

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize with the first gun if it exists
        if (guns.Count > 0)
        {
            equippedGunIndices.Add(0);
        }
        UpdateGunsVisibility();
    }

    void Update()
    {
        // Example: Switching weapons using keyboard inputs
        if (Input.GetKeyDown(KeyCode.Alpha1) && equippedGunIndices.Count > 0)
        {
            SwitchWeapon(0); // Switch to the first equipped gun
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && equippedGunIndices.Count > 1)
        {
            SwitchWeapon(1); // Switch to the second equipped gun if available
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && equippedGunIndices.Count > 2)
        {
            SwitchWeapon(2); // Switch to the third equipped gun if available
        }
        // Add more cases for additional gun slots (e.g., Alpha4 for slot 4)
    }

    public void SwitchWeapon(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedGunIndices.Count)
        {
            currentGunIndex = equippedGunIndices[slotIndex]; // Update current gun index
            UpdateGunsVisibility();
        }
    }

    public void UnlockGun(int gunIndex)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count && guns[gunIndex].locked)
        {
            guns[gunIndex].locked = false; // Unlock the gun
            equippedGunIndices.Add(gunIndex); // Add the unlocked gun to equipped guns list
            UpdateGunsVisibility(); // Update visibility
        }
    }

    public void UnlockGun(GameObject gunPrefab)
    {
        for (int i = 0; i < guns.Count; i++)
        {
            if (guns[i].gunObject == gunPrefab && guns[i].locked)
            {
                UnlockGun(i);
                return;
            }
        }
        Debug.LogWarning("The gun prefab is not found in the list or already unlocked.");
    }

    void UpdateGunsVisibility()
    {
        for (int i = 0; i < guns.Count; i++)
        {
            if (guns[i].gunObject != null)
            {
                guns[i].gunObject.SetActive(i == currentGunIndex && !guns[i].locked); // Activate the selected unlocked gun and deactivate others
            }
        }
    }
}
