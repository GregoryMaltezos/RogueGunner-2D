using UnityEngine;

public class GunRotation : MonoBehaviour
{
    public float flipThreshold = 90f; // Angle threshold for flipping the character
    private bool facingRight = true; // Variable to track character's facing direction

    void Update()
    {
        // Get horizontal input for character movement
        float horizontalInput = Input.GetAxis("Horizontal");

        // Get the mouse position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Get the direction from the weapon to the mouse
        Vector3 direction = mousePos - transform.position;

        // Determine if the character should be facing right or left based on the mouse position
        if (direction.x > 0 && !facingRight)
        {
            FlipCharacter();
        }
        else if (direction.x < 0 && facingRight)
        {
            FlipCharacter();
        }

        // Calculate the target rotation angle
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Adjust the rotation based on the character's facing direction
        if (!facingRight)
        {
            targetAngle += 180f; // Add 180 degrees if facing left
        }

        // Rotate the gun to face the mouse direction
        Quaternion rotation = Quaternion.Euler(0f, 0f, targetAngle);
        foreach (Transform child in transform)
        {
            child.rotation = rotation;
        }

        // Flip the character's scale based on movement input
        if (horizontalInput != 0)
        {
            transform.parent.localScale = new Vector3(Mathf.Abs(transform.parent.localScale.x) * Mathf.Sign(horizontalInput), transform.parent.localScale.y, transform.parent.localScale.z);
        }
    }

    void FlipCharacter()
    {
        // Flip the character's facing direction
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1
        Vector3 theScale = transform.parent.localScale;
        theScale.x *= -1;
        transform.parent.localScale = theScale;
    }
}
