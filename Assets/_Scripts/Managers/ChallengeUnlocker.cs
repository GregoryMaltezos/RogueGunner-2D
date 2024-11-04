using UnityEngine;

public class ChallengeUnlocker : MonoBehaviour
{
    public int weaponIndexToUnlock; // Index of the weapon to unlock in the GameProgressManager

    // This method should be called when the challenge is completed
    public void CompleteChallenge()
    {
        GameProgressManager.instance.UnlockWeapon(weaponIndexToUnlock);
        Debug.Log("Challenge completed! Weapon unlocked: " + weaponIndexToUnlock);
    }
}
