using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject settingsPanel;
    public GameObject challengesPanel; // The panel where challenges will be listed
    public GameObject otherCanvas;
    public GameObject challengePrefab; // Prefab for displaying each challenge
    private bool isPaused = false;
    private GunController gunController;
    private PlayerController playerController;

    private GameObject previousPanel; // To track the previous panel for back navigation

    public bool IsPaused
    {
        get { return isPaused; }
    }

    void Start()
    {
        gunController = FindObjectOfType<GunController>();
        playerController = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(false);
        challengesPanel.SetActive(false); // Hide Challenges menu
        Time.timeScale = 1f;
        isPaused = false;
        SetCursorState(false);
        EnableGunController(true);
        EnablePlayerController(true);
        if (otherCanvas != null) otherCanvas.SetActive(true);
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        settingsPanel.SetActive(false);
        challengesPanel.SetActive(false); // Ensure challenges panel is hidden
        Time.timeScale = 0f;
        isPaused = true;
        SetCursorState(true);
        EnableGunController(false);
        EnablePlayerController(false);
        if (otherCanvas != null) otherCanvas.SetActive(false);
    }

    public void OpenSettings()
    {
        previousPanel = pauseMenuUI.activeSelf ? pauseMenuUI : null;
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        settingsPanel.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    private void SetCursorState(bool isVisible)
    {
        Cursor.visible = isVisible;
    }

    private void EnableGunController(bool enable)
    {
        if (gunController != null)
        {
            gunController.enabled = enable;
        }
    }

    private void EnablePlayerController(bool enable)
    {
        if (playerController != null)
        {
            playerController.enabled = enable;
        }
    }

    public void OpenChallenges()
    {
        // Save the current panel to return to later
        previousPanel = pauseMenuUI.activeSelf ? pauseMenuUI : settingsPanel;

        pauseMenuUI.SetActive(false); // Hide Pause Menu
        settingsPanel.SetActive(false); // Hide Settings Panel
        challengesPanel.SetActive(true); // Show Challenges Menu

        DisplayChallenges(); // Populate challenges UI with current data
    }

    public void BackToPreviousMenu()
    {
        // Hide the challenges panel
        challengesPanel.SetActive(false);

        // Reactivate the previous panel
        if (previousPanel != null)
        {
            previousPanel.SetActive(true);
        }
    }

    private void DisplayChallenges()
    {
        List<ChallengeManager.Challenge> challenges = ChallengeManager.instance.challenges;

        // Ensure the Back button remains visible (if it's part of the challengesPanel)
        Transform backButtonTransform = challengesPanel.transform.Find("BackButton");
        if (backButtonTransform != null)
        {
            backButtonTransform.gameObject.SetActive(true);
        }

        // Clear all previous challenge entries to ensure a fresh display every time
        foreach (Transform child in challengesPanel.transform)
        {
            // Skip the Back button to prevent it from being destroyed
            if (child.name != "BackButton") // Replace with your actual button's name
            {
                Destroy(child.gameObject);
            }
        }

        // Instantiate new entries for each challenge
        for (int i = 0; i < challenges.Count; i++)
        {
            GameObject challengeTextObject = Instantiate(challengePrefab, challengesPanel.transform);

            TMP_Text challengeText = challengeTextObject.GetComponent<TMP_Text>();
            if (challengeText != null)
            {
                // Set the challenge status text with colored tags
                string status = challenges[i].completed ? "<color=green>Completed</color>" : "<color=red>Not Completed</color>";
                challengeText.text = $"{challenges[i].challengeId}: {status}\nDescription: {challenges[i].description}";
            }

            Debug.Log($"Challenge {i} displayed with ID: {challenges[i].challengeId} - {challenges[i].description}");
        }

        // Force layout update to refresh panel display
        LayoutRebuilder.ForceRebuildLayoutImmediate(challengesPanel.GetComponent<RectTransform>());
    }



}
