using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class BossHp : MonoBehaviour
{
    [SerializeField] private Slider slider; // Drag the Slider from the UI Canvas here
    [SerializeField] private float maxHp = 100f; // Default maximum HP
    private float currentHp;
    private bool hasFlashed = false; // To ensure the flash only happens once
    private bool canTakeDamage = true; // New variable to track if the boss can take damage

    public string bossId; // Add this field for the boss ID

    private CorridorFirstDungeonGenerator dungeonGenerator; // Reference to the dungeon generator

    [SerializeField] private GameObject portalPrefab; // Drag the Portal prefab here in the Inspector
    [SerializeField] private float deathAnimationDuration = 1f; // Duration of the death animation
                                                                // -------------------- Hit Sound Cooldown --------------------
    [Header("Hit Sound Settings")]
    [SerializeField]
    private float hitSoundCooldown = 0.5f; // Minimum time between hit sounds
    private float lastHitSoundTime = -Mathf.Infinity; // Time the last hit sound was played
    [SerializeField] private EventReference death;
    [SerializeField] private EventReference hit;
    public float CurrentHp => currentHp; // Expose current health
    private BossHp bossHp;
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

    public void TakeDamage(int amount)
    {
        if (!canTakeDamage || currentHp <= 0) return; // Prevent damage if flying or already dead

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

    public IEnumerator Die()
    {
        Debug.Log("Boss is dead!");
        AudioManager.instance.PlayOneShot(death, this.transform.position);
        GetComponent<Animator>().SetTrigger("Die");
        yield return new WaitForSeconds(deathAnimationDuration);
        SpawnPortal();
        AudioManager.instance.SetMusicArea(MusicType.Peacefull);
        if (bossId == "1")
        {
            ChallengeManager.instance.CompleteChallenge("DefeatBoss1");
        }

        Destroy(gameObject);
    }

    public float MaxHp => maxHp; // Public property to expose maxHp

    private void SpawnPortal()
    {
        if (portalPrefab != null)
        {
            // Spawn the portal at the fixed position (0, 0)
            Instantiate(portalPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }
        else
        {
            Debug.LogError("Portal prefab not assigned in the Inspector.");
        }
    }

    // Public method to enable or disable damage
    public void SetCanTakeDamage(bool value)
    {
        canTakeDamage = value;
    }
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
