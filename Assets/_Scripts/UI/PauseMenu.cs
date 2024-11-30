using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject settingsPanel;
    public GameObject challengesPanel;
    public GameObject controlsPanel; // Add reference to the Controls Panel
    public GameObject otherCanvas;
    public GameObject challengePrefab;
    private bool isPaused = false;
    private GunController gunController;
    private PlayerController playerController;

    private GameObject previousPanel;
    public InputActionReference pauseAction;
    public InputActionReference navigateAction;
    public InputActionReference submitAction;
    public InputActionReference cancelAction;

    public bool IsPaused
    {
        get { return isPaused; }
    }

    void Start()
    {
        gunController = FindObjectOfType<GunController>();
        playerController = FindObjectOfType<PlayerController>();
        pauseAction.action.Enable();
        navigateAction.action.Enable();
        submitAction.action.Enable();
        cancelAction.action.Enable();
    }

    void OnDisable()
    {
        pauseAction.action.Disable();
        navigateAction.action.Disable();
        submitAction.action.Disable();
        cancelAction.action.Disable();
    }

    void Update()
    {
        if (pauseAction.action.triggered)
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

        HandleNavigation();

        if (submitAction.action.triggered)
        {
            HandleSubmit();
        }

        if (cancelAction.action.triggered)
        {
            HandleCancel();
        }
    }

    private void HandleNavigation()
    {
        Vector2 navigationInput = navigateAction.action.ReadValue<Vector2>();

        if (navigationInput.y > 0) // Up
        {
            // Navigate up
        }
        else if (navigationInput.y < 0) // Down
        {
            // Navigate down
        }

        if (navigationInput.x != 0) // Left/Right
        {
            // Handle horizontal navigation
        }
    }

    private void HandleSubmit()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
                Debug.Log("Button clicked: " + button.name);
                return;
            }
        }

        Debug.Log("No button under mouse cursor.");
    }

    private void HandleCancel()
    {
        if (previousPanel != null)
        {
            if (settingsPanel.activeSelf) settingsPanel.SetActive(false);
            if (challengesPanel.activeSelf) challengesPanel.SetActive(false);
            if (controlsPanel.activeSelf) controlsPanel.SetActive(false);

            previousPanel.SetActive(true);
            previousPanel = null;
        }
        else
        {
            Resume();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(false);
        challengesPanel.SetActive(false);
        controlsPanel.SetActive(false); // Ensure controls panel is hidden
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
        challengesPanel.SetActive(false);
        controlsPanel.SetActive(false); // Ensure controls panel is hidden
        Time.timeScale = 0f;
        isPaused = true;
        SetCursorState(true);
        EnableGunController(false);
        EnablePlayerController(false);
        if (otherCanvas != null) otherCanvas.SetActive(false);

        EventSystem.current.SetSelectedGameObject(pauseMenuUI.transform.GetChild(0).gameObject);
    }

    public void OpenSettings()
    {
        previousPanel = pauseMenuUI.activeSelf ? pauseMenuUI : null;
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(settingsPanel.transform.GetChild(0).gameObject);
    }

    public void OpenControls()
    {
        previousPanel = pauseMenuUI.activeSelf ? pauseMenuUI : null;
        pauseMenuUI.SetActive(false);
        controlsPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(controlsPanel.transform.GetChild(0).gameObject);
    }

    public void BackToPauseMenu()
    {
        settingsPanel.SetActive(false);
        controlsPanel.SetActive(false);
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
        previousPanel = pauseMenuUI.activeSelf ? pauseMenuUI : settingsPanel;
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(false);
        challengesPanel.SetActive(true);

        DisplayChallenges();

        EventSystem.current.SetSelectedGameObject(challengesPanel.transform.GetChild(0).gameObject);
    }

    public void BackToPreviousMenu()
    {
        challengesPanel.SetActive(false);

        if (previousPanel != null)
        {
            previousPanel.SetActive(true);
        }
    }

    private void DisplayChallenges()
    {
        List<ChallengeManager.Challenge> challenges = ChallengeManager.instance.challenges;

        Transform backButtonTransform = challengesPanel.transform.Find("BackButton");
        if (backButtonTransform != null)
        {
            backButtonTransform.gameObject.SetActive(true);
        }

        foreach (Transform child in challengesPanel.transform)
        {
            if (child.name != "BackButton")
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < challenges.Count; i++)
        {
            GameObject challengeTextObject = Instantiate(challengePrefab, challengesPanel.transform);

            TMP_Text challengeText = challengeTextObject.GetComponent<TMP_Text>();
            if (challengeText != null)
            {
                string status = challenges[i].completed ? "<color=green>Completed</color>" : "<color=red>Not Completed</color>";
                challengeText.text = $"{challenges[i].challengeId}: {status}\nDescription: {challenges[i].description}";
            }

            Debug.Log($"Challenge {i} displayed with ID: {challenges[i].challengeId} - {challenges[i].description}");
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(challengesPanel.GetComponent<RectTransform>());
    }
}
