using UnityEngine;
using UnityEngine.UI;
using ProceduralDungeonShooter.Player;

namespace ProceduralDungeonShooter.UI
{
    /// <summary>
    /// Handles health display UI elements
    /// </summary>
    public class HealthUI : MonoBehaviour
    {
        [Header("Health UI Elements")]
        public Slider healthSlider;
        public Text healthText;
        public Image healthFill;
        
        [Header("Color Settings")]
        public Color fullHealthColor = Color.green;
        public Color midHealthColor = Color.yellow;
        public Color lowHealthColor = Color.red;
        
        [Header("Animation")]
        public bool animateHealthChanges = true;
        public float animationSpeed = 2f;
        
        // Private variables
        private float targetHealth = 1f;
        private float currentDisplayHealth = 1f;
        
        void Start()
        {
            UpdateHealth(1f);
        }
        
        void Update()
        {
            if (animateHealthChanges && Mathf.Abs(currentDisplayHealth - targetHealth) > 0.01f)
            {
                currentDisplayHealth = Mathf.MoveTowards(currentDisplayHealth, targetHealth, animationSpeed * Time.deltaTime);
                UpdateHealthDisplay();
            }
        }
        
        public void UpdateHealth(float healthPercentage)
        {
            targetHealth = Mathf.Clamp01(healthPercentage);
            
            if (!animateHealthChanges)
            {
                currentDisplayHealth = targetHealth;
                UpdateHealthDisplay();
            }
        }
        
        void UpdateHealthDisplay()
        {
            // Update slider
            if (healthSlider != null)
            {
                healthSlider.value = currentDisplayHealth;
            }
            
            // Update text
            if (healthText != null)
            {
                PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
                if (playerHealth != null)
                {
                    healthText.text = $"{playerHealth.GetCurrentHealth():F0}/{playerHealth.GetMaxHealth():F0}";
                }
                else
                {
                    healthText.text = $"{currentDisplayHealth * 100:F0}%";
                }
            }
            
            // Update color
            UpdateHealthColor();
        }
        
        void UpdateHealthColor()
        {
            if (healthFill == null) return;
            
            Color targetColor;
            
            if (currentDisplayHealth > 0.6f)
            {
                targetColor = Color.Lerp(midHealthColor, fullHealthColor, (currentDisplayHealth - 0.6f) / 0.4f);
            }
            else if (currentDisplayHealth > 0.3f)
            {
                targetColor = Color.Lerp(lowHealthColor, midHealthColor, (currentDisplayHealth - 0.3f) / 0.3f);
            }
            else
            {
                targetColor = lowHealthColor;
            }
            
            healthFill.color = targetColor;
        }
    }
}