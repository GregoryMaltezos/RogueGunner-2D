using UnityEngine;
using UnityEngine.UI;

public class BossHp : MonoBehaviour
{
    [SerializeField] private Slider slider; // Drag the Slider from the UI Canvas here
    [SerializeField] private float maxHp = 100f; // Default maximum HP
    private float currentHp;
    private bool hasFlashed = false; // To ensure the flash only happens once

    private void Start()
    {
        currentHp = maxHp; // Initialize HP
        UpdateHealthBar(); // Set initial slider value
    }

    public void TakeDamage(int amount)
    {
        if (currentHp <= 0) return; // Prevent damage if already dead

        currentHp -= amount; // Reduce HP
        if (currentHp < 0) currentHp = 0; // Prevent negative HP
        UpdateHealthBar(); // Update health bar

        // Notify boss script of health change
        SendMessage("SetCurrentHealth", (int)currentHp, SendMessageOptions.DontRequireReceiver);

        // Trigger the flash when health reaches 50% for the first time
        if (!hasFlashed && currentHp <= maxHp / 2)
        {
            hasFlashed = true; // Set the flag to true so it doesn't flash again
            SendMessage("FlashRed", SendMessageOptions.DontRequireReceiver);
        }

        if (currentHp <= 0)
        {
            Die(); // Handle boss death
        }
    }

    public void UpdateHealthBar()
    {
        slider.value = currentHp / maxHp; // Update slider value
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("FrBullet")) // Make sure the tag is "FrBullet"
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                // Ensure damage is applied only once per bullet
                bullet.hasCollided = true; // Mark the bullet as collided
                TakeDamage(bullet.damage);
                Destroy(collision.gameObject); // Destroy bullet after damage
            }
        }
    }

    private void Die()
    {
        Debug.Log("Boss is dead!");
        Destroy(gameObject); // Destroy boss
    }
}
