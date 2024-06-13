using UnityEngine;

public class LootInteraction : MonoBehaviour
{
    public GameObject weaponToUnlock; // The weapon prefab to unlock
    private WeaponManager weaponManager; // Reference to the WeaponManager script

    private void Start()
    {
        weaponManager = FindObjectOfType<WeaponManager>(); // Find the WeaponManager in the scene
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UnlockWeapon();
            Destroy(gameObject); // Destroy the chest loot
        }
    }

    private void UnlockWeapon()
    {
        if (weaponManager != null)
        {
            //weaponManager.AddGun(weaponToUnlock);
        }
        else
        {
            Debug.LogWarning("WeaponManager not found!");
        }
    }
}
