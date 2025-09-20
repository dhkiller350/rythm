using UnityEngine;
using UnityEngine.UI;
using ProceduralDungeonShooter.Player;

namespace ProceduralDungeonShooter.UI
{
    /// <summary>
    /// Handles ammo and weapon UI display
    /// </summary>
    public class AmmoUI : MonoBehaviour
    {
        [Header("Ammo UI Elements")]
        public Text ammoText;
        public Slider ammoSlider;
        public Image ammoFill;
        
        [Header("Reload UI")]
        public GameObject reloadIndicator;
        public Slider reloadProgress;
        public Text reloadText;
        
        [Header("Colors")]
        public Color fullAmmoColor = Color.white;
        public Color lowAmmoColor = Color.red;
        public Color reloadColor = Color.yellow;
        
        // Private variables
        private PlayerShooting playerShooting;
        private bool isReloading = false;
        
        void Start()
        {
            playerShooting = FindObjectOfType<PlayerShooting>();
            
            if (playerShooting != null)
            {
                playerShooting.OnReloadStart += OnReloadStart;
                playerShooting.OnReloadComplete += OnReloadComplete;
            }
            
            // Hide reload indicator initially
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(false);
            }
        }
        
        void Update()
        {
            if (isReloading && playerShooting != null)
            {
                UpdateReloadProgress();
            }
        }
        
        public void UpdateAmmo(int currentAmmo, int maxAmmo)
        {
            // Update ammo text
            if (ammoText != null)
            {
                ammoText.text = $"{currentAmmo} / {maxAmmo}";
            }
            
            // Update ammo slider
            if (ammoSlider != null)
            {
                ammoSlider.value = maxAmmo > 0 ? (float)currentAmmo / maxAmmo : 0f;
            }
            
            // Update ammo color
            UpdateAmmoColor(currentAmmo, maxAmmo);
        }
        
        void UpdateAmmoColor(int currentAmmo, int maxAmmo)
        {
            if (ammoFill == null) return;
            
            float ammoPercentage = maxAmmo > 0 ? (float)currentAmmo / maxAmmo : 0f;
            
            if (ammoPercentage <= 0.2f) // Low ammo
            {
                ammoFill.color = lowAmmoColor;
            }
            else
            {
                ammoFill.color = Color.Lerp(lowAmmoColor, fullAmmoColor, ammoPercentage);
            }
        }
        
        void OnReloadStart()
        {
            isReloading = true;
            
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(true);
            }
            
            if (reloadText != null)
            {
                reloadText.text = "Reloading...";
            }
            
            if (ammoFill != null)
            {
                ammoFill.color = reloadColor;
            }
        }
        
        void OnReloadComplete()
        {
            isReloading = false;
            
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(false);
            }
            
            // Reset ammo color
            if (playerShooting != null)
            {
                UpdateAmmoColor(playerShooting.currentAmmo, playerShooting.maxAmmo);
            }
        }
        
        void UpdateReloadProgress()
        {
            if (reloadProgress == null || playerShooting == null) return;
            
            // Calculate reload progress (this is a simplified version)
            // In a real implementation, you'd need to track the actual reload timer
            float progress = Mathf.PingPong(Time.time * 2f, 1f);
            reloadProgress.value = progress;
        }
        
        void OnDestroy()
        {
            if (playerShooting != null)
            {
                playerShooting.OnReloadStart -= OnReloadStart;
                playerShooting.OnReloadComplete -= OnReloadComplete;
            }
        }
    }
}