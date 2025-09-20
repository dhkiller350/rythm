using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProceduralDungeonShooter.Core
{
    /// <summary>
    /// Handles scene loading and transitions
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        [Header("Scene Names")]
        public string mainMenuScene = "MainMenu";
        public string gameScene = "GameScene";
        
        // Singleton instance
        public static SceneController Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuScene);
        }
        
        public void LoadGameScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameScene);
        }
        
        public void ReloadCurrentScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void QuitApplication()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}