using UnityEngine;
using System.Collections.Generic;
using ProceduralDungeonShooter.Dungeon;
using ProceduralDungeonShooter.Core;

namespace ProceduralDungeonShooter.Loot
{
    /// <summary>
    /// Manages loot spawning and distribution throughout the dungeon
    /// </summary>
    public class LootManager : MonoBehaviour
    {
        [Header("Loot Settings")]
        public GameObject[] lootPrefabs;
        public float lootDropChance = 0.7f;
        public int maxLootItems = 15;
        
        [Header("Loot Types")]
        public GameObject healthPickupPrefab;
        public GameObject ammoPickupPrefab;
        public GameObject weaponUpgradePrefab;
        
        [Header("Drop Rates")]
        [Range(0f, 1f)]
        public float healthDropRate = 0.4f;
        [Range(0f, 1f)]
        public float ammoDropRate = 0.3f;
        [Range(0f, 1f)]
        public float weaponUpgradeDropRate = 0.3f;
        
        // Private variables
        private List<GameObject> spawnedLoot = new List<GameObject>();
        private List<Transform> lootSpawnPoints = new List<Transform>();
        private DungeonGenerator dungeonGenerator;
        
        // Events
        public System.Action<GameObject> OnLootSpawned;
        public System.Action<Item> OnLootCollected;
        
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
        
        void OnGameStart()
        {
            StartCoroutine(DelayedLootSpawn());
        }
        
        void OnGameRestart()
        {
            ClearAllLoot();
        }
        
        System.Collections.IEnumerator DelayedLootSpawn()
        {
            // Wait for dungeon generation
            yield return new WaitForSeconds(0.5f);
            
            GetLootSpawnPoints();
            SpawnInitialLoot();
        }
        
        void GetLootSpawnPoints()
        {
            lootSpawnPoints.Clear();
            
            if (dungeonGenerator != null)
            {
                List<Transform> lootSpawns = dungeonGenerator.GetLootSpawnPoints();
                lootSpawnPoints.AddRange(lootSpawns);
            }
            
            Debug.Log($"Found {lootSpawnPoints.Count} loot spawn points");
        }
        
        void SpawnInitialLoot()
        {
            if (lootSpawnPoints.Count == 0)
            {
                Debug.LogWarning("No loot spawn points available");
                return;
            }
            
            int lootCount = Mathf.Min(maxLootItems, lootSpawnPoints.Count);
            
            for (int i = 0; i < lootCount; i++)
            {
                Transform spawnPoint = lootSpawnPoints[i];
                SpawnRandomLoot(spawnPoint.position);
            }
            
            Debug.Log($"Spawned {lootCount} loot items");
        }
        
        public void SpawnLoot(Vector3 position)
        {
            if (Random.value <= lootDropChance && spawnedLoot.Count < maxLootItems)
            {
                SpawnRandomLoot(position);
            }
        }
        
        void SpawnRandomLoot(Vector3 position)
        {
            GameObject lootPrefab = ChooseLootPrefab();
            
            if (lootPrefab != null)
            {
                Vector3 spawnPosition = position + Vector3.up * 0.5f;
                GameObject lootItem = Instantiate(lootPrefab, spawnPosition, Quaternion.identity);
                lootItem.transform.parent = transform;
                
                spawnedLoot.Add(lootItem);
                OnLootSpawned?.Invoke(lootItem);
                
                // Set up item component
                Item item = lootItem.GetComponent<Item>();
                if (item != null)
                {
                    item.OnItemCollected += OnItemCollected;
                }
                
                Debug.Log($"Spawned loot: {lootItem.name} at {spawnPosition}");
            }
        }
        
        GameObject ChooseLootPrefab()
        {
            float randomValue = Random.value;
            
            if (randomValue < healthDropRate && healthPickupPrefab != null)
            {
                return healthPickupPrefab;
            }
            else if (randomValue < healthDropRate + ammoDropRate && ammoPickupPrefab != null)
            {
                return ammoPickupPrefab;
            }
            else if (weaponUpgradePrefab != null)
            {
                return weaponUpgradePrefab;
            }
            
            // Fallback to random prefab from array
            if (lootPrefabs.Length > 0)
            {
                return lootPrefabs[Random.Range(0, lootPrefabs.Length)];
            }
            
            return null;
        }
        
        void OnItemCollected(Item item)
        {
            if (spawnedLoot.Contains(item.gameObject))
            {
                spawnedLoot.Remove(item.gameObject);
            }
            
            OnLootCollected?.Invoke(item);
        }
        
        public void ClearAllLoot()
        {
            foreach (GameObject loot in spawnedLoot)
            {
                if (loot != null)
                {
                    Destroy(loot);
                }
            }
            spawnedLoot.Clear();
        }
        
        public int GetActiveLootCount()
        {
            spawnedLoot.RemoveAll(loot => loot == null);
            return spawnedLoot.Count;
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