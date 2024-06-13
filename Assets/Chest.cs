using UnityEngine;

public class Chest : MonoBehaviour
{
    public Animator chestAnimator; // Reference to the Animator component
    public GameObject[] weaponPrefabs; // Array of weapon prefabs
    public Transform spawnPoint; // Point where the weapon will be spawned
    public float interactionDistance = 2.0f; // Distance within which the player can interact

    private bool isOpen = false; // Track whether the chest is open

    void Update()
    {
        // Check if the player is near and presses 'E' to open the chest
        if (Vector3.Distance(PlayerController.instance.transform.position, transform.position) < interactionDistance && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpen = true;
        chestAnimator.SetTrigger("Open"); // Play the opening animation
        SpawnRandomWeapon(); // Spawn a random weapon
    }

    void SpawnRandomWeapon()
    {
        if (weaponPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, weaponPrefabs.Length);
            GameObject spawnedWeaponPrefab = weaponPrefabs[randomIndex];

            // Spawn the weapon at the specified spawn point
            GameObject spawnedWeapon = Instantiate(spawnedWeaponPrefab, spawnPoint.position, spawnPoint.rotation);

            // Ensure the weapon is initially active when spawned
            spawnedWeapon.SetActive(true);

            // Notify the WeaponManager to unlock this weapon
            //WeaponManager.instance.UnlockWeapon(spawnedWeapon);
        }
        else
        {
            Debug.LogWarning("No weapon prefabs assigned to the chest.");
        }
    }

    // Method to handle when a weapon is picked up from the chest
    public void NotifyWeaponPickedUp(GameObject weapon)
    {
        // Implement any specific logic here if needed
        Debug.Log("Weapon picked up from chest: " + weapon.name);
    }
}
