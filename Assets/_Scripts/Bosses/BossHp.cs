using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class BossHp : MonoBehaviour
{
    [SerializeField] private Slider slider; 
    [SerializeField] private float maxHp = 100f; // Default maximum HP
    private float currentHp;
    private bool hasFlashed = false; // To ensure the flash only happens once
    private bool canTakeDamage = true; //  track if the boss can take damage

    public string bossId; // field for the boss ID

    private CorridorFirstDungeonGenerator dungeonGenerator; // Reference to the dungeon generator

    [SerializeField] private GameObject portalPrefab; 
    [SerializeField] private float deathAnimationDuration = 1f; // Duration of the death animation
                                                              
    [Header("Hit Sound Settings")]
    [SerializeField]
    private float hitSoundCooldown = 0.5f; // Minimum time between hit sounds
    private float lastHitSoundTime = -Mathf.Infinity; // Time the last hit sound was played
    [SerializeField] private EventReference death;
    [SerializeField] private EventReference hit;
    public float CurrentHp => currentHp; // Expose current health
    private BossHp bossHp;

    /// <summary>
    /// Initializes the boss's health and sets up references.
    /// </summary>
    private void Start()
    {
        currentHp = maxHp; // Initialize HP
        UpdateHealthBar(); // Set initial slider value
        bossHp = GetComponent<BossHp>();
        // Find the dungeon generator in the scene (if it exists)
        dungeonGenerator = FindObjectOfType<CorridorFirstDungeonGenerator>();
        if (dungeonGenerator == null)
        {
            Debug.LogError("Dungeon Generator not found in the scene.");
        }
    }

    /// <summary>
    /// Reduces the boss's health by a specified amount and checks for death or events.
    /// </summary>
    /// <param name="amount">The amount of damage to apply.</param>
    public void TakeDamage(int amount) 
    {
        if (!canTakeDamage || currentHp <= 0) return; // Prevent damage if the boss cannot take damage or is already dead

        currentHp -= amount; // Reduce HP
        PlayHitSound();
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
            StartCoroutine(Die()); // Handle boss death with animation
        }
    }

    /// <summary>
    /// Updates the health bar slider to reflect the current health.
    /// </summary>
    public void UpdateHealthBar()
    {
        slider.value = currentHp / maxHp; // Update slider value
    }


    /// <summary>
    /// Handles collision with bullets to apply damage.
    /// </summary>
    /// <param name="collision">Collision information.</param>
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

    /// <summary>
    /// Handles the boss's death, including animations, portal spawning, and cleanup.
    /// </summary>
    public IEnumerator Die()
    {
        Debug.Log("Boss is dead!");
        AudioManager.instance.PlayOneShot(death, this.transform.position);  // Play death sound
        GetComponent<Animator>().SetTrigger("Die");
        yield return new WaitForSeconds(deathAnimationDuration); // Wait for animation to complete
        SpawnPortal(); // Spawn a portal after death
        AudioManager.instance.SetMusicArea(MusicType.Peacefull); // Switch to peaceful music
        if (bossId == "1") // Complete challenge if the boss ID matches a specific value
        {
            ChallengeManager.instance.CompleteChallenge("DefeatGolem");
        }

        Destroy(gameObject); // Remove the boss object
    }

    public float MaxHp => maxHp; // Public property to expose maxHp

    /// <summary>
    /// Spawns a portal at a fixed position after the boss's death.
    /// </summary>
    private void SpawnPortal()
    {
        if (portalPrefab != null)
        {
            // Spawn the portal at the fixed position (0, 0)
            Instantiate(portalPrefab, new Vector3(0, 0, 0), Quaternion.identity); //Spawn Portal
        }
        else
        {
            Debug.LogError("Portal prefab not assigned in the Inspector.");
        }
    }

    /// <summary>
    /// Enables or disables the boss's ability to take damage.
    /// </summary>
    /// <param name="value">True to enable damage, false to disable it.</param>
    public void SetCanTakeDamage(bool value)
    {
        canTakeDamage = value;
    }

    /// <summary>
    /// Plays the hit sound effect, ensuring a cooldown between sounds.
    /// </summary>
    private void PlayHitSound()
    {
        // Only play the hit sound if enough time has passed since the last one
        if (Time.time >= lastHitSoundTime + hitSoundCooldown)
        {
            AudioManager.instance.PlayOneShot(hit, this.transform.position);
            lastHitSoundTime = Time.time; // Update the time of the last hit sound
        }
    }
}
