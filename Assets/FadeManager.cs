using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    [SerializeField] private Image fadeImage; // Reference to the Image component for fading
    [SerializeField] private float fadeOutDuration = 2f; // Duration of the fade out
    [SerializeField] private float fadeInDuration = 2f;  // Duration of the fade in
    [SerializeField] private Canvas transitionCanvas; // Reference to the Canvas to change its sorting order

    // Sorting orders for fade-out and fade-in
    [SerializeField] private int fadeOutSortingOrder = 3; // Sorting order during fade out
    [SerializeField] private int fadeInSortingOrder = 0;  // Sorting order during fade in

    private int originalSortingOrder; // Store the original sorting order

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

        // Store the original sorting order of the transition canvas
        if (transitionCanvas != null)
        {
            originalSortingOrder = transitionCanvas.sortingOrder; // Get the initial sorting order
        }
        else
        {
            Debug.LogError("Transition Canvas is not assigned in the Inspector.");
        }
    }

    public IEnumerator FadeToBlack()
    {
        // Set the transition canvas sorting order for fade out
        if (transitionCanvas != null)
        {
            transitionCanvas.sortingOrder = fadeOutSortingOrder; // Change sorting order for fade out
        }

        float elapsedTime = 0f;
        Color tempColor = fadeImage.color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            tempColor.a = Mathf.Clamp01(elapsedTime / fadeOutDuration); // Fade the alpha from 0 to 1
            fadeImage.color = tempColor;
            yield return null; // Wait for the next frame
        }
    }

    public IEnumerator FadeToClear()
    {
        float elapsedTime = 0f;
        Color tempColor = fadeImage.color;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            tempColor.a = Mathf.Clamp01(1 - (elapsedTime / fadeInDuration)); // Fade the alpha from 1 to 0
            fadeImage.color = tempColor;
            yield return null; // Wait for the next frame
        }

        // Set the transition canvas sorting order for fade in
        if (transitionCanvas != null)
        {
            transitionCanvas.sortingOrder = fadeInSortingOrder; // Restore sorting order after fade in
        }
    }
}
