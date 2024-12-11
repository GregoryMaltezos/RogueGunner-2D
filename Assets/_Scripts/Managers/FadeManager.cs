using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private Image fadeImage; // Reference to the Image component for fading
    [SerializeField] private float fadeOutDuration = 2f; // Duration of the fade out
    [SerializeField] private float fadeInDuration = 2f;  // Duration of the fade in

    public Canvas transitionCanvas; // Reference to the Canvas to change its sorting order

    /// <summary>
    /// Initializes the FadeManager by ensuring that the fade image starts fully transparent,
    /// and the transition canvas is correctly set up.
    /// </summary>
    private void Start()
    {
        // Ensure the fadeImage is fully transparent at start
        if (fadeImage != null)
        {
            Color tempColor = fadeImage.color;
            tempColor.a = 0f; // Set to transparent
            fadeImage.color = tempColor;
        }
        else
        {
            Debug.LogError("Fade Image is not assigned in the Inspector.");
        }

        if (transitionCanvas != null)  // Ensure the transition canvas is active, but disable raycast blocking
        {
            transitionCanvas.gameObject.SetActive(true);
            transitionCanvas.sortingOrder = 0; // Set the initial sorting order to the back
            fadeImage.raycastTarget = false; // Prevent fade image from blocking clicks
        }
        else
        {
            Debug.LogError("Transition Canvas is not assigned in the Inspector.");
        }
    }
    /// <summary>
    /// Fades the screen to black over the specified duration.
    /// </summary>
    /// <returns>An IEnumerator for use in a coroutine.</returns>
    public IEnumerator FadeToBlack()
    {
        if (transitionCanvas != null) // Ensure the canvas is brought to the front for the fade-out
        {
            transitionCanvas.sortingOrder = 3; // Set a high sorting order to appear above other UI elements
            fadeImage.raycastTarget = true;    // Enable raycasting to block user interaction during the fade-out
        }

        float elapsedTime = 0f;  // Fade out the screen (make the fade image's alpha value increase)
        Color tempColor = fadeImage.color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime to ignore time scaling
            tempColor.a = Mathf.Clamp01(elapsedTime / fadeOutDuration); // Gradually increase alpha
            fadeImage.color = tempColor;
            yield return null;
        }

        tempColor.a = 1f; // Ensure the alpha reaches 1 (fully opaque)
        fadeImage.color = tempColor;
        Debug.Log("Fade to black complete.");
    }

    /// <summary>
    /// Fades the screen to clear (transparent) over the specified duration.
    /// </summary>
    /// <returns>An IEnumerator for use in a coroutine.</returns>
    public IEnumerator FadeToClear()
    {
        float elapsedTime = 0f; // Fade in the screen (make the fade image's alpha value decrease)
        Color tempColor = fadeImage.color;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime to ignore time scaling
            tempColor.a = Mathf.Clamp01(1 - (elapsedTime / fadeInDuration)); // Gradually decrease alpha
            fadeImage.color = tempColor;
            yield return null;
        }

        tempColor.a = 0f; // Ensure the alpha reaches 0 (fully transparent)
        fadeImage.color = tempColor;

        if (transitionCanvas != null) // Reset the canvas to the background and disable raycasting after fade-in
        {
            transitionCanvas.sortingOrder = 0; // Lower the sorting order after fade in
            fadeImage.raycastTarget = false;   // Disable blocking after fade in
        }

        Debug.Log("Fade to clear complete.");
    }
}
