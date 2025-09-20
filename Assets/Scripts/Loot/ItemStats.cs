using UnityEngine;

namespace ProceduralDungeonShooter.Loot
{
    /// <summary>
    /// Generates random stats for items to create variety in loot
    /// </summary>
    public class ItemStats : MonoBehaviour
    {
        [Header("Base Stats")]
        public float baseValue = 10f;
        public float baseDamageBonus = 5f;
        public float baseFireRateBonus = 1f;
        public float baseAccuracyBonus = 0.1f;
        
        [Header("Randomization")]
        [Range(0f, 1f)]
        public float valueVariance = 0.3f;
        [Range(0f, 1f)]
        public float bonusVariance = 0.4f;
        
        [Header("Rarity Settings")]
        [Range(0f, 1f)]
        public float uncommonChance = 0.3f;
        [Range(0f, 1f)]
        public float rareChance = 0.15f;
        [Range(0f, 1f)]
        public float epicChance = 0.04f;
        [Range(0f, 1f)]
        public float legendaryChance = 0.01f;
        
        // Current stats
        [HideInInspector]
        public ItemRarity rarity = ItemRarity.Common;
        [HideInInspector]
        public float currentValue;
        [HideInInspector]
        public float damageBonus;
        [HideInInspector]
        public float fireRateBonus;
        [HideInInspector]
        public float accuracyBonus;
        
        public enum ItemRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }
        
        void Awake()
        {
            GenerateRandomStats();
        }
        
        public void GenerateRandomStats()
        {
            // Determine rarity
            rarity = DetermineRarity();
            
            // Get rarity multiplier
            float rarityMultiplier = GetRarityMultiplier(rarity);
            
            // Generate random multipliers
            float valueMult = Random.Range(1f - valueVariance, 1f + valueVariance);
            float damageMult = Random.Range(1f - bonusVariance, 1f + bonusVariance);
            float fireRateMult = Random.Range(1f - bonusVariance, 1f + bonusVariance);
            float accuracyMult = Random.Range(1f - bonusVariance, 1f + bonusVariance);
            
            // Apply base stats with randomization and rarity multiplier
            currentValue = baseValue * valueMult * rarityMultiplier;
            damageBonus = baseDamageBonus * damageMult * rarityMultiplier;
            fireRateBonus = baseFireRateBonus * fireRateMult * rarityMultiplier;
            accuracyBonus = baseAccuracyBonus * accuracyMult * rarityMultiplier;
            
            // Ensure values are positive
            currentValue = Mathf.Max(1f, currentValue);
            damageBonus = Mathf.Max(0f, damageBonus);
            fireRateBonus = Mathf.Max(0f, fireRateBonus);
            accuracyBonus = Mathf.Max(0f, accuracyBonus);
            accuracyBonus = Mathf.Min(0.5f, accuracyBonus); // Cap accuracy bonus
            
            Debug.Log($"Generated {rarity} item stats - Value: {currentValue:F1}, Damage: +{damageBonus:F1}");
        }
        
        ItemRarity DetermineRarity()
        {
            float roll = Random.value;
            
            if (roll < legendaryChance)
            {
                return ItemRarity.Legendary;
            }
            else if (roll < legendaryChance + epicChance)
            {
                return ItemRarity.Epic;
            }
            else if (roll < legendaryChance + epicChance + rareChance)
            {
                return ItemRarity.Rare;
            }
            else if (roll < legendaryChance + epicChance + rareChance + uncommonChance)
            {
                return ItemRarity.Uncommon;
            }
            else
            {
                return ItemRarity.Common;
            }
        }
        
        float GetRarityMultiplier(ItemRarity itemRarity)
        {
            switch (itemRarity)
            {
                case ItemRarity.Common:
                    return 1f;
                case ItemRarity.Uncommon:
                    return 1.3f;
                case ItemRarity.Rare:
                    return 1.7f;
                case ItemRarity.Epic:
                    return 2.2f;
                case ItemRarity.Legendary:
                    return 3f;
                default:
                    return 1f;
            }
        }
        
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return Color.white;
                case ItemRarity.Uncommon:
                    return Color.green;
                case ItemRarity.Rare:
                    return Color.blue;
                case ItemRarity.Epic:
                    return Color.magenta;
                case ItemRarity.Legendary:
                    return Color.yellow;
                default:
                    return Color.white;
            }
        }
        
        public float GetPrimaryValue()
        {
            return currentValue;
        }
        
        public float GetDamageBonus()
        {
            return damageBonus;
        }
        
        public float GetFireRateBonus()
        {
            return fireRateBonus;
        }
        
        public float GetAccuracyBonus()
        {
            return accuracyBonus;
        }
        
        public string GetRarityString()
        {
            return rarity.ToString();
        }
        
        public int GetRarityValue()
        {
            return (int)rarity;
        }
        
        public float GetStatSum()
        {
            return currentValue + damageBonus + fireRateBonus + (accuracyBonus * 10f);
        }
        
        public ItemStatData GetStatData()
        {
            return new ItemStatData
            {
                rarity = rarity,
                value = currentValue,
                damageBonus = damageBonus,
                fireRateBonus = fireRateBonus,
                accuracyBonus = accuracyBonus
            };
        }
        
        public void ApplyStatModifiers(float valueModifier, float bonusModifier)
        {
            currentValue *= valueModifier;
            damageBonus *= bonusModifier;
            fireRateBonus *= bonusModifier;
            accuracyBonus *= bonusModifier;
            
            // Ensure bounds
            currentValue = Mathf.Max(1f, currentValue);
            damageBonus = Mathf.Max(0f, damageBonus);
            fireRateBonus = Mathf.Max(0f, fireRateBonus);
            accuracyBonus = Mathf.Clamp(accuracyBonus, 0f, 0.5f);
        }
        
        public bool IsHigherRarity(ItemRarity otherRarity)
        {
            return (int)rarity > (int)otherRarity;
        }
        
        public float GetRarityChance(ItemRarity targetRarity)
        {
            switch (targetRarity)
            {
                case ItemRarity.Uncommon:
                    return uncommonChance;
                case ItemRarity.Rare:
                    return rareChance;
                case ItemRarity.Epic:
                    return epicChance;
                case ItemRarity.Legendary:
                    return legendaryChance;
                default:
                    return 1f - (uncommonChance + rareChance + epicChance + legendaryChance);
            }
        }
    }
    
    // Data structure for item stats
    [System.Serializable]
    public struct ItemStatData
    {
        public ItemStats.ItemRarity rarity;
        public float value;
        public float damageBonus;
        public float fireRateBonus;
        public float accuracyBonus;
    }
}