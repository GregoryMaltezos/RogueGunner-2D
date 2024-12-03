using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoEnablePlayerAmmo : MonoBehaviour
{
    private GameObject gunUIManager; // Reference to the GunUIManager
    private GameObject playerAmmo;   // Reference to the PlayerAmmo canvas

    void Start()
    {
        // Find the GunUIManager in the scene
        gunUIManager = GameObject.FindObjectOfType<GunUIManager>()?.gameObject;

        if (gunUIManager != null)
        {
            // Try to find the PlayerAmmo object as a child of GunUIManager
            playerAmmo = gunUIManager.transform.Find("PlayerAmmo")?.gameObject;

            if (playerAmmo != null)
            {
                // Now check the current scene
                string currentScene = SceneManager.GetActiveScene().name;

                // If the scene is "RoomContent", enable PlayerAmmo; otherwise, disable it
                if (currentScene == "RoomContent")
                {
                    EnablePlayerAmmo();
                }
                else
                {
                    DisablePlayerAmmo();
                }
            }
            else
            {
                Debug.LogError("PlayerAmmo canvas not found as a child of GunUIManager.");
            }
        }
        else
        {
            Debug.LogError("GunUIManager not found in the scene.");
        }
    }

    // Enable PlayerAmmo
    private void EnablePlayerAmmo()
    {
        if (playerAmmo != null && !playerAmmo.activeSelf)
        {
            playerAmmo.SetActive(true);
            Debug.Log("PlayerAmmo enabled.");
        }
    }

    // Disable PlayerAmmo
    private void DisablePlayerAmmo()
    {
        if (playerAmmo != null && playerAmmo.activeSelf)
        {
            playerAmmo.SetActive(false);
            Debug.Log("PlayerAmmo disabled.");
        }
    }
}
