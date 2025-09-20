using UnityEngine;
using ProceduralDungeonShooter.Player;

namespace ProceduralDungeonShooter.Loot
{
    /// <summary>
    /// Base item class for all collectible items
    /// </summary>
    public class Item : MonoBehaviour
    {
        [Header("Item Settings")]
        public ItemType itemType;
        public string itemName = "Item";
        public string description = "A useful item";
        public float value = 1f;
        
        [Header("Visual Settings")]
        public float rotationSpeed = 90f;
        public float bobSpeed = 2f;
        public float bobHeight = 0.3f;
        public GameObject collectEffect;
        
        [Header("Audio")]
        public AudioClip collectSound;
        
        // Private variables
        private Vector3 startPosition;
        private bool isCollected = false;
        private ItemStats stats;
        
        // Events
        public System.Action<Item> OnItemCollected;
        
        public enum ItemType
        {
            HealthPickup,
            AmmoPickup,
            WeaponUpgrade,
            SpeedBoost,
            DamageBoost,
            ArmorPickup
        }
        
        void Awake()
        {
            stats = GetComponent<ItemStats>();
            startPosition = transform.position;
        }
        
        void Start()
        {
            // Generate random stats if ItemStats component is present
            if (stats != null)
            {
                stats.GenerateRandomStats();
                ApplyStatsToItem();
            }
        }
        
        void Update()
        {
            if (!isCollected)
            {
                AnimateItem();
            }
        }
        
        void AnimateItem()
        {
            // Rotate item
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            // Bob up and down
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        
        void ApplyStatsToItem()
        {
            if (stats != null)
            {
                value = stats.GetPrimaryValue();
                
                // Update visual appearance based on rarity
                UpdateVisualsByRarity(stats.rarity);
            }
        }
        
        void UpdateVisualsByRarity(ItemStats.ItemRarity rarity)
        {
            Renderer itemRenderer = GetComponentInChildren<Renderer>();
            if (itemRenderer != null)
            {
                switch (rarity)
                {
                    case ItemStats.ItemRarity.Common:
                        itemRenderer.material.color = Color.white;
                        break;
                    case ItemStats.ItemRarity.Uncommon:
                        itemRenderer.material.color = Color.green;
                        break;
                    case ItemStats.ItemRarity.Rare:
                        itemRenderer.material.color = Color.blue;
                        break;
                    case ItemStats.ItemRarity.Epic:
                        itemRenderer.material.color = Color.magenta;
                        break;
                    case ItemStats.ItemRarity.Legendary:
                        itemRenderer.material.color = Color.yellow;
                        rotationSpeed *= 1.5f; // Legendary items spin faster
                        break;
                }
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;
            
            if (other.CompareTag("Player"))
            {
                CollectItem(other.gameObject);
            }
        }
        
        void CollectItem(GameObject collector)
        {
            if (isCollected) return;
            
            isCollected = true;
            
            // Apply item effect
            ApplyItemEffect(collector);
            
            // Play collect sound
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
            
            // Spawn collect effect
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, transform.rotation);
            }
            
            // Notify events
            OnItemCollected?.Invoke(this);
            
            Debug.Log($"Player collected {itemName} (Value: {value})");
            
            // Destroy item
            Destroy(gameObject);
        }
        
        void ApplyItemEffect(GameObject player)
        {
            switch (itemType)
            {
                case ItemType.HealthPickup:
                    ApplyHealthPickup(player);
                    break;
                    
                case ItemType.AmmoPickup:
                    ApplyAmmoPickup(player);
                    break;
                    
                case ItemType.WeaponUpgrade:
                    ApplyWeaponUpgrade(player);
                    break;
                    
                case ItemType.SpeedBoost:
                    ApplySpeedBoost(player);
                    break;
                    
                case ItemType.DamageBoost:
                    ApplyDamageBoost(player);
                    break;
                    
                case ItemType.ArmorPickup:
                    ApplyArmorPickup(player);
                    break;
            }
        }
        
        void ApplyHealthPickup(GameObject player)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(value);
            }
        }
        
        void ApplyAmmoPickup(GameObject player)
        {
            PlayerShooting playerShooting = player.GetComponent<PlayerShooting>();
            if (playerShooting != null)
            {
                playerShooting.AddAmmo((int)value);
            }
        }
        
        void ApplyWeaponUpgrade(GameObject player)
        {
            PlayerShooting playerShooting = player.GetComponent<PlayerShooting>();
            if (playerShooting != null && stats != null)
            {
                float damageBonus = stats.GetDamageBonus();
                float fireRateBonus = stats.GetFireRateBonus();
                float accuracyBonus = stats.GetAccuracyBonus();
                
                playerShooting.UpgradeWeapon(
                    playerShooting.damage + damageBonus,
                    playerShooting.fireRate + fireRateBonus,
                    playerShooting.weaponAccuracy + accuracyBonus
                );
            }
        }
        
        void ApplySpeedBoost(GameObject player)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.walkSpeed += value;
                playerController.runSpeed += value * 1.5f;
            }
        }
        
        void ApplyDamageBoost(GameObject player)
        {
            PlayerShooting playerShooting = player.GetComponent<PlayerShooting>();
            if (playerShooting != null)
            {
                playerShooting.damage += value;
            }
        }
        
        void ApplyArmorPickup(GameObject player)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Increase max health as armor
                playerHealth.maxHealth += value;
                playerHealth.Heal(value); // Also heal for the armor amount
            }
        }
        
        public string GetDisplayName()
        {
            if (stats != null)
            {
                return $"{stats.rarity} {itemName}";
            }
            return itemName;
        }
        
        public string GetDescription()
        {
            string desc = description;
            
            if (stats != null)
            {
                desc += $"\\nRarity: {stats.rarity}";
                desc += $"\\nValue: {value:F1}";
                
                if (itemType == ItemType.WeaponUpgrade)
                {
                    desc += $"\\nDamage Bonus: +{stats.GetDamageBonus():F1}";
                    desc += $"\\nFire Rate Bonus: +{stats.GetFireRateBonus():F1}";
                    desc += $"\\nAccuracy Bonus: +{stats.GetAccuracyBonus():F2}";
                }
            }
            
            return desc;
        }
        
        public ItemStats GetStats()
        {
            return stats;
        }
    }
}