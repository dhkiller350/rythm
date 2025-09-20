using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProceduralDungeonShooter.Core
{
    /// <summary>
    /// Main game manager that handles game state, level progression, and core game loop
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        public bool startGameOnAwake = true;
        public float restartDelay = 2f;
        
        [Header("Game State")]
        public bool gameStarted = false;
        public bool gameOver = false;
        public int currentLevel = 1;
        public float gameTime = 0f;
        
        // Singleton instance
        public static GameManager Instance { get; private set; }
        
        // Events
        public System.Action OnGameStart;
        public System.Action OnGameOver;
        public System.Action OnGameRestart;
        
        // Component references
        private DungeonGenerator dungeonGenerator;
        private PlayerController playerController;
        private UIManager uiManager;
        
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            if (startGameOnAwake)
            {
                StartGame();
            }
        }
        
        void Update()
        {
            if (gameStarted && !gameOver)
            {
                gameTime += Time.deltaTime;
                
                // Check for restart input after game over
                if (gameOver && Input.GetKeyDown(KeyCode.R))
                {
                    RestartGame();
                }
            }
        }
        
        void InitializeGame()
        {
            // Find and cache component references
            dungeonGenerator = FindObjectOfType<DungeonGenerator>();
            playerController = FindObjectOfType<PlayerController>();
            uiManager = FindObjectOfType<UIManager>();
            
            // Set initial game state
            gameStarted = false;
            gameOver = false;
            gameTime = 0f;
            currentLevel = 1;
            
            // Ensure cursor is locked for FPS controls
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void StartGame()
        {
            gameStarted = true;
            gameOver = false;
            gameTime = 0f;
            
            // Generate new dungeon
            if (dungeonGenerator != null)
            {
                dungeonGenerator.GenerateDungeon();
            }
            
            // Initialize player
            if (playerController != null)
            {
                playerController.InitializePlayer();
            }
            
            // Update UI
            if (uiManager != null)
            {
                uiManager.OnGameStart();
            }
            
            // Invoke game start event
            OnGameStart?.Invoke();
            
            Debug.Log("Game Started - New dungeon generated!");
        }
        
        public void GameOver()
        {
            if (gameOver) return; // Prevent multiple calls
            
            gameOver = true;
            gameStarted = false;
            
            // Show cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Update UI
            if (uiManager != null)
            {
                uiManager.OnGameOver();
            }
            
            // Invoke game over event
            OnGameOver?.Invoke();
            
            Debug.Log($"Game Over! Survived for {gameTime:F1} seconds");
            
            // Auto-restart after delay (optional)
            // Invoke(nameof(RestartGame), restartDelay);
        }
        
        public void RestartGame()
        {
            // Reset game state
            gameOver = false;
            gameStarted = false;
            gameTime = 0f;
            
            // Lock cursor for FPS controls
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Invoke restart event
            OnGameRestart?.Invoke();
            
            // Start new game
            StartGame();
            
            Debug.Log("Game Restarted - New dungeon generated!");
        }
        
        public void PauseGame()
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void ResumeGame()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        // Called when player dies
        public void OnPlayerDeath()
        {
            GameOver();
        }
        
        // Get current game statistics
        public GameStats GetGameStats()
        {
            return new GameStats
            {
                survivalTime = gameTime,
                currentLevel = currentLevel,
                isGameOver = gameOver
            };
        }
    }
    
    // Data structure for game statistics
    [System.Serializable]
    public struct GameStats
    {
        public float survivalTime;
        public int currentLevel;
        public bool isGameOver;
    }
}