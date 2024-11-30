using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GunUIManager : MonoBehaviour
{
    public TextMeshProUGUI ammoText; // UI element for ammo
    public TextMeshProUGUI grenadeText; // UI element for grenades
    public static GunUIManager instance;

    private GameObject playerAmmo;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Multiple instances of GunUIManager found. Destroying the new one.");
            Destroy(gameObject);
        }

        playerAmmo = transform.Find("PlayerAmmo")?.gameObject;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        AdjustPlayerAmmoVisibility();

        GameObject playerHPObject = GameObject.Find("PlayerHP");
        if (playerHPObject != null)
        {
            Transform panelTransform = playerHPObject.transform.Find("Panel");
            if (panelTransform != null)
            {
                ammoText = panelTransform.Find("AmmoText")?.GetComponent<TextMeshProUGUI>();
                grenadeText = panelTransform.Find("GrenadeText")?.GetComponent<TextMeshProUGUI>();

                if (ammoText == null) Debug.LogError("AmmoText not found or missing TextMeshProUGUI component.");
                if (grenadeText == null) Debug.LogError("GrenadeText not found or missing TextMeshProUGUI component.");
            }
        }

        StartCoroutine(InitializeUI());
    }

    private IEnumerator InitializeUI()
    {
        while (WeaponManager.instance == null)
        {
            yield return null;
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (WeaponManager.instance == null)
        {
            Debug.LogWarning("WeaponManager instance is null. Cannot update UI.");
            return;
        }

        int currentGunIndex = WeaponManager.instance.GetCurrentGunIndex();
        Gun currentGun = WeaponManager.instance.GetGun(currentGunIndex);

        if (currentGun != null)
        {
            int clipAmmo = currentGun.currentClipAmmo;
            int bulletsRemaining = currentGun.bulletsRemaining;

            ammoText.text = currentGun.infiniteAmmo ? $"{clipAmmo}/∞" : $"{clipAmmo}/{bulletsRemaining}";
        }

        // Update grenade count
        if (grenadeText != null && PlayerGrenade.instance != null)
        {
            grenadeText.text = $"{PlayerGrenade.instance.GetCurrentGrenades()}";
        }
    }

    public void OnWeaponPickup(int gunIndex)
    {
        UpdateUI();
    }

    private void AdjustPlayerAmmoVisibility()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "MainMenu")
        {
            DisablePlayerAmmo();
        }
        else
        {
            EnablePlayerAmmo();
        }
    }

    private void EnablePlayerAmmo()
    {
        if (playerAmmo != null && !playerAmmo.activeSelf)
        {
            playerAmmo.SetActive(true);
        }
    }

    private void DisablePlayerAmmo()
    {
        if (playerAmmo != null && playerAmmo.activeSelf)
        {
            playerAmmo.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AdjustPlayerAmmoVisibility();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Update grenades specifically
    public void UpdateGrenadeUI(int currentGrenades)
    {
        if (grenadeText != null)
        {
            grenadeText.text = $"{currentGrenades}";
        }
    }
}
