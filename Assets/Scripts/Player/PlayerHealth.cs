using UnityEngine;
using ProceduralDungeonShooter.Core;

namespace ProceduralDungeonShooter.Player
{
    /// <summary>
    /// Handles player health, damage, and death
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public float maxHealth = 100f;
        public float currentHealth;
        
        [Header("Damage Settings")]
        public float invulnerabilityTime = 1f;
        public bool isInvulnerable = false;
        
        [Header("Visual Feedback")]
        public float damageFlashDuration = 0.1f;
        public Color damageColor = Color.red;
        
        // Events
        public System.Action<float> OnHealthChanged;
        public System.Action OnPlayerDeath;
        
        // Private variables
        private float invulnerabilityTimer = 0f;
        private Camera playerCamera;
        private Color originalCameraColor;
        private bool isDead = false;
        
        void Awake()
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                originalCameraColor = playerCamera.backgroundColor;
            }
        }
        
        void Start()
        {
            ResetHealth();
        }
        
        void Update()
        {
            // Handle invulnerability timer
            if (isInvulnerable)
            {
                invulnerabilityTimer -= Time.deltaTime;
                if (invulnerabilityTimer <= 0f)
                {
                    isInvulnerable = false;
                }
            }
        }
        
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isDead = false;
            isInvulnerable = false;
            invulnerabilityTimer = 0f;
            
            // Notify UI of health change
            OnHealthChanged?.Invoke(GetHealthPercentage());
            
            Debug.Log($"Player health reset to {maxHealth}");
        }
        
        public void TakeDamage(float damage)
        {
            // Don't take damage if dead or invulnerable
            if (isDead || isInvulnerable)
                return;
            
            // Apply damage
            currentHealth -= damage;
            currentHealth = Mathf.Max(0f, currentHealth);
            
            // Set invulnerability
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityTime;
            
            // Visual feedback
            StartCoroutine(DamageFlash());
            
            // Notify UI of health change
            OnHealthChanged?.Invoke(GetHealthPercentage());
            
            Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");
            
            // Check for death
            if (currentHealth <= 0f)
            {
                Die();
            }
        }
        
        public void Heal(float healAmount)
        {
            if (isDead)
                return;
            
            currentHealth += healAmount;
            currentHealth = Mathf.Min(maxHealth, currentHealth);
            
            // Notify UI of health change
            OnHealthChanged?.Invoke(GetHealthPercentage());
            
            Debug.Log($"Player healed for {healAmount}. Health: {currentHealth}/{maxHealth}");
        }
        
        void Die()
        {
            if (isDead)
                return;
            
            isDead = true;
            
            Debug.Log("Player died!");
            
            // Disable player movement
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            // Disable player shooting
            PlayerShooting playerShooting = GetComponent<PlayerShooting>();
            if (playerShooting != null)
            {
                playerShooting.SetCanShoot(false);
            }
            
            // Notify events
            OnPlayerDeath?.Invoke();
            
            // Notify game manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerDeath();
            }
        }
        
        private System.Collections.IEnumerator DamageFlash()
        {
            if (playerCamera != null)
            {
                // Flash red
                playerCamera.backgroundColor = damageColor;
                yield return new WaitForSeconds(damageFlashDuration);
                
                // Return to original color
                playerCamera.backgroundColor = originalCameraColor;
            }
        }
        
        public float GetHealthPercentage()
        {
            return maxHealth > 0 ? currentHealth / maxHealth : 0f;
        }
        
        public bool IsDead()
        {
            return isDead;
        }
        
        public bool IsInvulnerable()
        {
            return isInvulnerable;
        }
        
        public float GetCurrentHealth()
        {
            return currentHealth;
        }
        
        public float GetMaxHealth()
        {
            return maxHealth;
        }
        
        // Called by external damage sources
        void OnTriggerEnter(Collider other)
        {
            // Handle collision damage from enemies or environmental hazards
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    TakeDamage(enemy.GetDamage());
                }
            }
            else if (other.CompareTag("Hazard"))
            {
                // Environmental damage
                TakeDamage(10f);
            }
        }
    }
}