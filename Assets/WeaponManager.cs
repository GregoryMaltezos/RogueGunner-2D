using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GunInfo
{
    public GameObject gunObject;
    public bool locked;
}

public class WeaponManager : MonoBehaviour
{
    public List<GunInfo> guns = new List<GunInfo>(); // List to hold all the guns (locked and unlocked)
    private int currentGunIndex = 0; // Default to the first gun

    void Start()
    {
        UpdateGunsVisibility();
    }

    void Update()
    {
        // Example: Switching weapons using keyboard inputs
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(0); // Switch to the first gun
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && guns.Count > 1)
        {
            SwitchWeapon(1); // Switch to the second gun if available
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && guns.Count > 2)
        {
            SwitchWeapon(2); // Switch to the third gun if available
        }
        // Add more cases for additional gun slots (e.g., Alpha4 for slot 4)
    }

    public void SwitchWeapon(int gunIndex)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count && guns[gunIndex].gunObject != null && !guns[gunIndex].locked)
        {
            currentGunIndex = gunIndex; // Update current gun index
            UpdateGunsVisibility();
        }
    }

    public void UnlockGun(int gunIndex)
    {
        if (gunIndex >= 0 && gunIndex < guns.Count && guns[gunIndex].locked)
        {
            guns[gunIndex].locked = false; // Unlock the gun
            UpdateGunsVisibility(); // Update visibility
        }
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