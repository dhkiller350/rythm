using UnityEngine;
using UnityEngine.UI;
using ProceduralDungeonShooter.Core;
using ProceduralDungeonShooter.Enemies;

namespace ProceduralDungeonShooter.UI
{
    /// <summary>
    /// Handles game over screen display and functionality
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("Game Over UI Elements")]
        public Text gameOverTitle;
        public Text survivalTimeText;
        public Text enemiesKilledText;
        public Text itemsCollectedText;
        public Text finalScoreText;
        
        [Header("Buttons")]
        public Button restartButton;
        public Button mainMenuButton;
        public Button quitButton;
        
        [Header("Statistics")]
        public GameObject statisticsPanel;
        public Text bestTimeText;
        public Text totalGamesText;
        
        // Private variables
        private GameStats currentGameStats;
        
        void Start()
        {
            // Set up button listeners
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }
        }
        
        public void UpdateGameOverScreen()
        {
            if (GameManager.Instance != null)
            {
                currentGameStats = GameManager.Instance.GetGameStats();
                DisplayGameStats();
            }
        }
        
        void DisplayGameStats()
        {
            // Update survival time
            if (survivalTimeText != null)
            {
                survivalTimeText.text = $"Survival Time: {currentGameStats.survivalTime:F1}s";
            }
            
            // Update enemies killed (simplified - would need proper tracking)
            if (enemiesKilledText != null)
            {
                EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
                int enemiesKilled = spawner != null ? spawner.GetTotalEnemiesSpawned() - spawner.GetAliveEnemyCount() : 0;
                enemiesKilledText.text = $"Enemies Defeated: {enemiesKilled}";
            }
            
            // Update items collected (simplified - would need proper tracking)
            if (itemsCollectedText != null)
            {
                // This would need proper item collection tracking
                itemsCollectedText.text = "Items Collected: --";
            }
            
            // Calculate and display final score
            if (finalScoreText != null)
            {
                int finalScore = CalculateFinalScore();
                finalScoreText.text = $"Final Score: {finalScore}";
            }
            
            // Update statistics
            UpdateStatistics();
        }
        
        int CalculateFinalScore()
        {
            // Simple scoring system
            int score = Mathf.RoundToInt(currentGameStats.survivalTime * 10);
            
            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null)
            {
                int enemiesKilled = spawner.GetTotalEnemiesSpawned() - spawner.GetAliveEnemyCount();
                score += enemiesKilled * 50;
            }
            
            // Bonus for higher difficulty levels
            score += (currentGameStats.currentLevel - 1) * 100;
            
            return score;
        }
        
        void UpdateStatistics()
        {
            // Update best time (using PlayerPrefs for persistence)
            float bestTime = PlayerPrefs.GetFloat("BestSurvivalTime", 0f);
            if (currentGameStats.survivalTime > bestTime)
            {
                bestTime = currentGameStats.survivalTime;
                PlayerPrefs.SetFloat("BestSurvivalTime", bestTime);
                PlayerPrefs.Save();
            }
            
            if (bestTimeText != null)
            {
                bestTimeText.text = $"Best Time: {bestTime:F1}s";
            }
            
            // Update total games played
            int totalGames = PlayerPrefs.GetInt("TotalGamesPlayed", 0) + 1;
            PlayerPrefs.SetInt("TotalGamesPlayed", totalGames);
            PlayerPrefs.Save();
            
            if (totalGamesText != null)
            {
                totalGamesText.text = $"Games Played: {totalGames}";
            }
        }
        
        void RestartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }
        
        void ReturnToMainMenu()
        {
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ReturnToMainMenu();
            }
        }
        
        void QuitGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }
        
        public void ShowStatistics()
        {
            if (statisticsPanel != null)
            {
                statisticsPanel.SetActive(!statisticsPanel.activeSelf);
            }
        }
        
        public void ResetStatistics()
        {
            PlayerPrefs.DeleteKey("BestSurvivalTime");
            PlayerPrefs.DeleteKey("TotalGamesPlayed");
            PlayerPrefs.Save();
            
            UpdateStatistics();
            
            Debug.Log("Statistics reset");
        }
    }
}