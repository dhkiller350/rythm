using UnityEngine;
using System.Collections.Generic;
using ProceduralDungeonShooter.Core;

namespace ProceduralDungeonShooter.Enemies
{
    /// <summary>
    /// Spawns enemies throughout the dungeon at designated spawn points
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawning Settings")]
        public GameObject[] enemyPrefabs;
        public int minEnemiesPerRoom = 1;
        public int maxEnemiesPerRoom = 3;
        public float spawnDelay = 1f;
        
        [Header("Difficulty Scaling")]
        public bool scaleDifficulty = true;
        public float difficultyIncrease = 0.1f;
        public int maxEnemyCount = 20;
        
        [Header("Spawn Timing")]
        public bool spawnOnGameStart = true;
        public bool spawnContinuously = false;
        public float respawnDelay = 30f;
        
        // Private variables
        private List<Transform> spawnPoints = new List<Transform>();
        private List<GameObject> spawnedEnemies = new List<GameObject>();
        private DungeonGenerator dungeonGenerator;
        private int currentDifficultyLevel = 1;
        private float lastSpawnTime = 0f;
        
        // Events
        public System.Action<int> OnEnemiesSpawned;
        public System.Action<GameObject> OnEnemySpawned;
        
        void Awake()
        {
            dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        }
        
        void Start()
        {
            // Subscribe to game events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart += OnGameStart;
                GameManager.Instance.OnGameRestart += OnGameRestart;
            }
        }
        
        void Update()
        {
            if (spawnContinuously && Time.time >= lastSpawnTime + respawnDelay)
            {
                if (GetAliveEnemyCount() < maxEnemyCount / 2)
                {
                    SpawnEnemyWave();
                    lastSpawnTime = Time.time;
                }
            }
        }
        
        void OnGameStart()
        {
            if (spawnOnGameStart)
            {
                StartCoroutine(DelayedSpawn());
            }
        }
        
        void OnGameRestart()
        {
            ClearAllEnemies();
            currentDifficultyLevel = 1;
        }
        
        System.Collections.IEnumerator DelayedSpawn()
        {
            // Wait for dungeon generation to complete
            yield return new WaitForSeconds(0.5f);
            
            GetSpawnPoints();
            SpawnInitialEnemies();
        }
        
        void GetSpawnPoints()
        {
            spawnPoints.Clear();
            
            if (dungeonGenerator != null)
            {
                List<Transform> enemySpawns = dungeonGenerator.GetEnemySpawnPoints();
                spawnPoints.AddRange(enemySpawns);
            }
            
            Debug.Log($"Found {spawnPoints.Count} enemy spawn points");
        }
        
        void SpawnInitialEnemies()
        {
            if (spawnPoints.Count == 0 || enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("No spawn points or enemy prefabs available");
                return;
            }
            
            int totalEnemies = 0;
            
            foreach (Transform spawnPoint in spawnPoints)
            {
                int enemiesInThisRoom = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
                
                // Limit total enemy count
                if (totalEnemies + enemiesInThisRoom > maxEnemyCount)
                {
                    enemiesInThisRoom = maxEnemyCount - totalEnemies;
                }
                
                if (enemiesInThisRoom <= 0) break;
                
                StartCoroutine(SpawnEnemiesAtPoint(spawnPoint, enemiesInThisRoom));
                totalEnemies += enemiesInThisRoom;
            }
            
            OnEnemiesSpawned?.Invoke(totalEnemies);
            Debug.Log($"Spawning {totalEnemies} enemies across {spawnPoints.Count} rooms");
        }
        
        System.Collections.IEnumerator SpawnEnemiesAtPoint(Transform spawnPoint, int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnEnemyAtPosition(spawnPoint.position);
                yield return new WaitForSeconds(spawnDelay);
            }
        }
        
        GameObject SpawnEnemyAtPosition(Vector3 position)
        {
            if (enemyPrefabs.Length == 0) return null;
            
            // Choose random enemy prefab
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            
            // Add some random offset to spawn position
            Vector3 spawnPosition = position + new Vector3(
                Random.Range(-2f, 2f),
                0f,
                Random.Range(-2f, 2f)
            );
            
            // Spawn enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.transform.parent = transform; // Organize under spawner
            
            // Apply difficulty scaling
            if (scaleDifficulty)
            {
                ApplyDifficultyScaling(enemy);
            }
            
            // Add to spawned enemies list
            spawnedEnemies.Add(enemy);
            
            // Subscribe to enemy death
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.OnEnemyDeath += OnEnemyDied;
            }
            
            OnEnemySpawned?.Invoke(enemy);
            Debug.Log($"Spawned enemy at {spawnPosition}");
            
            return enemy;
        }
        
        void ApplyDifficultyScaling(GameObject enemy)
        {
            EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
            if (enemyStats != null)
            {
                // Increase stats based on difficulty level
                float difficultyMultiplier = 1f + (currentDifficultyLevel - 1) * difficultyIncrease;
                
                enemyStats.baseHealth *= difficultyMultiplier;
                enemyStats.baseDamage *= difficultyMultiplier;
                enemyStats.baseSpeed *= Mathf.Min(difficultyMultiplier, 1.5f); // Cap speed increase
                
                // Reapply random stats with new base values
                enemyStats.ApplyRandomStats();
            }
        }
        
        void OnEnemyDied(EnemyController enemy)
        {
            if (spawnedEnemies.Contains(enemy.gameObject))
            {
                spawnedEnemies.Remove(enemy.gameObject);
            }
            
            // Check if all enemies are dead
            if (GetAliveEnemyCount() <= 0)
            {
                OnAllEnemiesDefeated();
            }
        }
        
        void OnAllEnemiesDefeated()
        {
            currentDifficultyLevel++;
            Debug.Log($"All enemies defeated! Difficulty increased to level {currentDifficultyLevel}");
            
            // Spawn new wave after delay
            if (spawnContinuously)
            {
                Invoke(nameof(SpawnEnemyWave), respawnDelay);
            }
        }
        
        void SpawnEnemyWave()
        {
            if (spawnPoints.Count == 0) return;
            
            int waveSize = Random.Range(minEnemiesPerRoom * spawnPoints.Count / 2, 
                                       maxEnemiesPerRoom * spawnPoints.Count / 2);
            waveSize = Mathf.Min(waveSize, maxEnemyCount - GetAliveEnemyCount());
            
            if (waveSize <= 0) return;
            
            // Distribute enemies across random spawn points
            for (int i = 0; i < waveSize; i++)
            {
                Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Count)];
                SpawnEnemyAtPosition(randomSpawn.position);
            }
            
            Debug.Log($"Spawned enemy wave of {waveSize} enemies (Difficulty: {currentDifficultyLevel})");
        }
        
        public void SpawnEnemyAtRandomPoint()
        {
            if (spawnPoints.Count > 0)
            {
                Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Count)];
                SpawnEnemyAtPosition(randomSpawn.position);
            }
        }
        
        public void ClearAllEnemies()
        {
            foreach (GameObject enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            spawnedEnemies.Clear();
        }
        
        public int GetAliveEnemyCount()
        {
            // Remove null references
            spawnedEnemies.RemoveAll(enemy => enemy == null);
            return spawnedEnemies.Count;
        }
        
        public int GetTotalEnemiesSpawned()
        {
            return spawnedEnemies.Count;
        }
        
        public int GetCurrentDifficultyLevel()
        {
            return currentDifficultyLevel;
        }
        
        public List<GameObject> GetSpawnedEnemies()
        {
            return new List<GameObject>(spawnedEnemies);
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= OnGameStart;
                GameManager.Instance.OnGameRestart -= OnGameRestart;
            }
        }
    }
}