using UnityEngine;

public class ResetButtonHandler : MonoBehaviour
{
    // This method will be called when the button is clicked
    public void DiePlayer()
    {
        // You can put any player death logic here, such as disabling player controls, playing death animation, etc.
        PlayerController.instance.Die(); // Replace with your actual player death method or logic

        // Complete the "DieOnce" challenge
        ChallengeManager.instance.CompleteChallenge("DieOnce");

        Debug.Log("Player died. DieOnce challenge completed.");
    }
}
