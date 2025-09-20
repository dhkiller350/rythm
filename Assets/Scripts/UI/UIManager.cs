using UnityEngine;
using UnityEngine.UI;
using ProceduralDungeonShooter.Player;
using ProceduralDungeonShooter.Enemies;
using ProceduralDungeonShooter.Loot;
using ProceduralDungeonShooter.Core;

namespace ProceduralDungeonShooter.UI
{
    /// <summary>
    /// Main UI manager that coordinates all UI elements
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject mainMenuPanel;
        public GameObject gameplayPanel;
        public GameObject gameOverPanel;
        public GameObject pausePanel;
        
        [Header("UI Components")]
        public HealthUI healthUI;
        public AmmoUI ammoUI;
        public GameOverUI gameOverUI;
        public Text scoreText;
        public Text timeText;
        
        [Header("HUD Elements")]
        public GameObject crosshair;
        public Text enemyCountText;
        public Text lootCountText;
        public Text levelText;
        
        // Private variables
        private bool isGameplayUIActive = false;
        private bool isPaused = false;
        
        // Game references
        private PlayerHealth playerHealth;
        private PlayerShooting playerShooting;
        private EnemySpawner enemySpawner;
        private LootManager lootManager;
        
        void Awake()
        {
            // Find game components
            FindGameComponents();
            
            // Subscribe to component events
            SubscribeToEvents();
        }
        
        void Start()
        {
            // Initialize UI state
            ShowMainMenu();
        }
        
        void Update()
        {
            if (isGameplayUIActive)
            {
                UpdateGameplayUI();
                HandleUIInput();
            }
        }
        
        void FindGameComponents()
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            playerShooting = FindObjectOfType<PlayerShooting>();
            enemySpawner = FindObjectOfType<EnemySpawner>();
            lootManager = FindObjectOfType<LootManager>();
        }
        
        void SubscribeToEvents()
        {
            // Subscribe to player health changes
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += OnPlayerHealthChanged;
            }
            
            // Subscribe to ammo changes
            if (playerShooting != null)
            {
                playerShooting.OnAmmoChanged += OnAmmoChanged;
            }
            
            // Subscribe to game manager events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart += OnGameStart;
                GameManager.Instance.OnGameOver += OnGameOver;
            }
        }
        
        void HandleUIInput()
        {
            // Pause/unpause with Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
        
        void UpdateGameplayUI()
        {
            // Update time display
            if (timeText != null && GameManager.Instance != null)
            {
                timeText.text = $"Time: {GameManager.Instance.gameTime:F1}s";
            }
            
            // Update enemy count
            if (enemyCountText != null && enemySpawner != null)
            {
                enemyCountText.text = $"Enemies: {enemySpawner.GetAliveEnemyCount()}";
            }
            
            // Update loot count
            if (lootCountText != null && lootManager != null)
            {
                lootCountText.text = $"Loot: {lootManager.GetActiveLootCount()}";
            }
            
            // Update level
            if (levelText != null && enemySpawner != null)
            {
                levelText.text = $"Level: {enemySpawner.GetCurrentDifficultyLevel()}";
            }
        }
        
        public void ShowMainMenu()
        {
            SetPanelActive(mainMenuPanel, true);
            SetPanelActive(gameplayPanel, false);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(pausePanel, false);
            
            isGameplayUIActive = false;
            
            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void OnGameStart()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(gameplayPanel, true);
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(pausePanel, false);
            
            isGameplayUIActive = true;
            isPaused = false;
            
            // Hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Enable crosshair
            if (crosshair != null)
            {
                crosshair.SetActive(true);
            }
            
            // Initialize UI values
            InitializeGameplayUI();
        }
        
        public void OnGameOver()
        {
            SetPanelActive(gameOverPanel, true);
            SetPanelActive(gameplayPanel, false);
            
            isGameplayUIActive = false;
            
            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Hide crosshair
            if (crosshair != null)
            {
                crosshair.SetActive(false);
            }
            
            // Update game over UI
            if (gameOverUI != null)
            {
                gameOverUI.UpdateGameOverScreen();
            }
        }
        
        void InitializeGameplayUI()
        {
            // Initialize health UI
            if (healthUI != null && playerHealth != null)
            {
                healthUI.UpdateHealth(playerHealth.GetHealthPercentage());
            }
            
            // Initialize ammo UI
            if (ammoUI != null && playerShooting != null)
            {
                ammoUI.UpdateAmmo(playerShooting.currentAmmo, playerShooting.maxAmmo);
            }
        }
        
        void OnPlayerHealthChanged(float healthPercentage)
        {
            if (healthUI != null)
            {
                healthUI.UpdateHealth(healthPercentage);
            }
        }
        
        void OnAmmoChanged(int currentAmmo, int maxAmmo)
        {
            if (ammoUI != null)
            {
                ammoUI.UpdateAmmo(currentAmmo, maxAmmo);
            }
        }
        
        public void PauseGame()
        {
            isPaused = true;
            SetPanelActive(pausePanel, true);
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
        }
        
        public void ResumeGame()
        {
            isPaused = false;
            SetPanelActive(pausePanel, false);
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }
        
        void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
        
        // UI Button Methods
        public void StartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }
        
        public void RestartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }
        
        public void QuitGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }
        
        public void ReturnToMainMenu()
        {
            ShowMainMenu();
            
            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadMainMenu();
            }
        }
        
        public void ShowControls()
        {
            Debug.Log("Controls: WASD - Move, Mouse - Look, Left Click - Shoot, R - Restart (after death), ESC - Pause");
        }
        
        public bool IsGameplayUIActive()
        {
            return isGameplayUIActive;
        }
        
        public bool IsPaused()
        {
            return isPaused;
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= OnPlayerHealthChanged;
            }
            
            if (playerShooting != null)
            {
                playerShooting.OnAmmoChanged -= OnAmmoChanged;
            }
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= OnGameStart;
                GameManager.Instance.OnGameOver -= OnGameOver;
            }
        }
    }
}