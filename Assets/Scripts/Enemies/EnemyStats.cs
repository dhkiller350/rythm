using UnityEngine;

namespace ProceduralDungeonShooter.Enemies
{
    /// <summary>
    /// Randomized enemy stats for procedural variety
    /// </summary>
    public class EnemyStats : MonoBehaviour
    {
        [Header("Base Stats")]
        public float baseHealth = 50f;
        public float baseDamage = 10f;
        public float baseSpeed = 3.5f;
        public float baseDetectionRange = 10f;
        
        [Header("Randomization Range")]
        [Range(0f, 1f)]
        public float healthVariance = 0.3f;
        [Range(0f, 1f)]
        public float damageVariance = 0.3f;
        [Range(0f, 1f)]
        public float speedVariance = 0.2f;
        [Range(0f, 1f)]
        public float detectionVariance = 0.2f;
        
        [Header("Elite Enemy Chance")]
        [Range(0f, 1f)]
        public float eliteChance = 0.1f;
        public float eliteMultiplier = 2f;
        public Color eliteColor = Color.red;
        public GameObject eliteEffect;
        
        // Current stats
        [HideInInspector]
        public float health;
        [HideInInspector]
        public float damage;
        [HideInInspector]
        public float speed;
        [HideInInspector]
        public float detectionRange;
        [HideInInspector]
        public bool isElite = false;
        
        void Awake()
        {
            // Apply randomization on awake
            ApplyRandomStats();
        }
        
        public void ApplyRandomStats()
        {
            // Determine if this is an elite enemy
            isElite = Random.value < eliteChance;
            
            // Calculate random multipliers
            float healthMult = Random.Range(1f - healthVariance, 1f + healthVariance);
            float damageMult = Random.Range(1f - damageVariance, 1f + damageVariance);
            float speedMult = Random.Range(1f - speedVariance, 1f + speedVariance);
            float detectionMult = Random.Range(1f - detectionVariance, 1f + detectionVariance);
            
            // Apply base stats with randomization
            health = baseHealth * healthMult;
            damage = baseDamage * damageMult;
            speed = baseSpeed * speedMult;
            detectionRange = baseDetectionRange * detectionMult;
            
            // Apply elite bonuses
            if (isElite)
            {
                health *= eliteMultiplier;
                damage *= eliteMultiplier;
                speed *= 1.2f; // Slight speed boost for elites
                detectionRange *= 1.3f; // Better detection for elites
                
                ApplyEliteVisuals();
            }
            
            Debug.Log($"Enemy stats generated - Health: {health:F1}, Damage: {damage:F1}, Speed: {speed:F1}, Elite: {isElite}");
        }
        
        void ApplyEliteVisuals()
        {
            // Change enemy color to indicate elite status
            Renderer enemyRenderer = GetComponentInChildren<Renderer>();
            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = eliteColor;
            }
            
            // Spawn elite effect
            if (eliteEffect != null)
            {
                GameObject effect = Instantiate(eliteEffect, transform);
                effect.transform.localPosition = Vector3.zero;
            }
            
            // Increase scale slightly
            transform.localScale *= 1.1f;
        }
        
        public EnemyStatData GetStatData()
        {
            return new EnemyStatData
            {
                health = health,
                damage = damage,
                speed = speed,
                detectionRange = detectionRange,
                isElite = isElite
            };
        }
        
        public float GetDifficultyRating()
        {
            // Calculate difficulty based on stats
            float rating = (health / baseHealth) * 0.4f +
                          (damage / baseDamage) * 0.4f +
                          (speed / baseSpeed) * 0.2f;
            
            if (isElite)
            {
                rating *= 1.5f;
            }
            
            return rating;
        }
    }
    
    // Data structure for enemy stats
    [System.Serializable]
    public struct EnemyStatData
    {
        public float health;
        public float damage;
        public float speed;
        public float detectionRange;
        public bool isElite;
    }
}