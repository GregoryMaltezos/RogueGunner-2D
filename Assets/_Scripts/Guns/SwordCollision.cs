using UnityEngine;

public class SwordCollision : MonoBehaviour
{
    public bool isSwordActive; // Indicates whether the sword is being used

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider that entered the trigger is an arrow and if the sword is active
        if (other.CompareTag("Arrow") && isSwordActive)
        {
            Debug.Log("Arrow hit the sword! Destroying arrow.");
            Destroy(other.gameObject); // Destroy the arrow upon hit
        }
    }

    // Call this method to activate or deactivate the sword
    public void SetSwordActive(bool active)
    {
        isSwordActive = active;
    }
}
