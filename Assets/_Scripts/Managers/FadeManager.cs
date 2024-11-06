using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private Image fadeImage; // Reference to the Image component for fading
    [SerializeField] private float fadeOutDuration = 2f; // Duration of the fade out
    [SerializeField] private float fadeInDuration = 2f;  // Duration of the fade in

    public Canvas transitionCanvas; // Reference to the Canvas to change its sorting order

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

        // Ensure the transition canvas is active, but disable raycast blocking
        if (transitionCanvas != null)
        {
            transitionCanvas.gameObject.SetActive(true);
            transitionCanvas.sortingOrder = 0; // Start with low sorting order
            fadeImage.raycastTarget = false; // Prevent fade image from blocking clicks
        }
        else
        {
            Debug.LogError("Transition Canvas is not assigned in the Inspector.");
        }
    }

    public IEnumerator FadeToBlack()
    {
        if (transitionCanvas != null)
        {
            transitionCanvas.sortingOrder = 3; // Bring canvas to the front for fade out
            fadeImage.raycastTarget = true;    // Enable blocking during fade out
        }

        float elapsedTime = 0f;
        Color tempColor = fadeImage.color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            tempColor.a = Mathf.Clamp01(elapsedTime / fadeOutDuration);
            fadeImage.color = tempColor;
            yield return null;
        }

        tempColor.a = 1f;
        fadeImage.color = tempColor;
        Debug.Log("Fade to black complete.");
    }

    public IEnumerator FadeToClear()
    {
        float elapsedTime = 0f;
        Color tempColor = fadeImage.color;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            tempColor.a = Mathf.Clamp01(1 - (elapsedTime / fadeInDuration));
            fadeImage.color = tempColor;
            yield return null;
        }

        tempColor.a = 0f;
        fadeImage.color = tempColor;

        if (transitionCanvas != null)
        {
            transitionCanvas.sortingOrder = 0; // Lower the sorting order after fade in
            fadeImage.raycastTarget = false;   // Disable blocking after fade in
        }

        Debug.Log("Fade to clear complete.");
    }
}
