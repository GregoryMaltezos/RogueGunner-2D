using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Reference to the Death Menu Panel UI element
    public GameObject deathMenuPanel; 

    private static UIManager instance;

    /// <summary>
    /// Ensures that only one instance of UIManager exists using the Singleton pattern.
    /// It also ensures the instance persists across scene loads.
    /// </summary>
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

    /// <summary>
    /// Displays the Death Menu by enabling the death menu panel.
    /// </summary>
    public void ShowDeathMenu()
    {
        // Check if the deathMenuPanel reference is assigned
        if (deathMenuPanel != null)
        {
            deathMenuPanel.SetActive(true); // Show the death menu by enabling the panel
        }
        else
        {
            Debug.LogError("Death Menu Panel not assigned.");
        } 
    }

    /// <summary>
    /// Hides the Death Menu by disabling the death menu panel.
    /// </summary>
    public void HideDeathMenu()
    {
        // Check if the deathMenuPanel reference is assigned
        if (deathMenuPanel != null)
        {
            deathMenuPanel.SetActive(false); // Hide the death menu by disabling the panel
        }
    }
}
