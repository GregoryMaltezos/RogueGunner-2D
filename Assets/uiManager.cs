using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject deathMenuPanel; // Reference to your Death Menu Panel

    private static UIManager instance;

    private void Awake()
    {
        // Singleton pattern: Ensure only one instance of UIManager exists
        if (instance != null)
        {
            Destroy(gameObject); // Destroy duplicate if another instance exists
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
    }

    // Show the Death Menu
    public void ShowDeathMenu()
    {
        if (deathMenuPanel != null)
        {
            deathMenuPanel.SetActive(true); // Show the death menu
        }
        else
        {
            Debug.LogError("Death Menu Panel not assigned.");
        }
    }

    // Hide the Death Menu
    public void HideDeathMenu()
    {
        if (deathMenuPanel != null)
        {
            deathMenuPanel.SetActive(false); // Hide the death menu
        }
    }
}
