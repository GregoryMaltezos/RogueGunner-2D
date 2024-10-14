using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private Image fadeImage; // Reference to the Image component for fading
    [SerializeField] private float fadeOutDuration = 2f; // Duration of the fade out
    [SerializeField] private float fadeInDuration = 2f;  // Duration of the fade in

    // Change this from private to public or protected internal
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

        // Ensure the transition canvas is active
        if (transitionCanvas != null)
        {
            transitionCanvas.gameObject.SetActive(true); // Activate the canvas
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
            transitionCanvas.sortingOrder = 3; // Change sorting order for fade out
        }

        float elapsedTime = 0f;
        Color tempColor = fadeImage.color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime
            tempColor.a = Mathf.Clamp01(elapsedTime / fadeOutDuration); // Fade the alpha from 0 to 1
            fadeImage.color = tempColor;
            yield return null; // Wait for the next frame
        }

        // Ensure it's fully black at the end
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
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime
            tempColor.a = Mathf.Clamp01(1 - (elapsedTime / fadeInDuration)); // Fade the alpha from 1 to 0
            fadeImage.color = tempColor;
            yield return null; // Wait for the next frame
        }

        // Ensure it's fully clear at the end
        tempColor.a = 0f;
        fadeImage.color = tempColor;

        if (transitionCanvas != null)
        {
            transitionCanvas.sortingOrder = 0; // Restore sorting order after fade in
        }
        Debug.Log("Fade to clear complete.");
    }
}
