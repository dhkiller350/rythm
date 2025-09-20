using UnityEngine;
using System.Collections.Generic;
using ProceduralDungeonShooter.Player;

namespace ProceduralDungeonShooter.Dungeon
{
    /// <summary>
    /// Procedural dungeon generator that creates random 3D dungeon layouts
    /// </summary>
    public class DungeonGenerator : MonoBehaviour
    {
        [Header("Dungeon Settings")]
        public int dungeonWidth = 20;
        public int dungeonHeight = 20;
        public float roomSize = 10f;
        public float wallHeight = 3f;
        
        [Header("Generation Parameters")]
        [Range(0.3f, 0.7f)]
        public float wallDensity = 0.45f;
        public int minRoomSize = 3;
        public int maxRooms = 8;
        public int generationSeed = 0;
        
        [Header("Prefabs")]
        public GameObject floorPrefab;
        public GameObject wallPrefab;
        public GameObject ceilingPrefab;
        public GameObject doorPrefab;
        
        [Header("Spawn Points")]
        public Transform playerSpawnPoint;
        public List<Transform> enemySpawnPoints = new List<Transform>();
        public List<Transform> lootSpawnPoints = new List<Transform>();
        
        // Private variables
        private int[,] dungeonGrid;
        private List<RoomData> rooms = new List<RoomData>();
        private GameObject dungeonParent;
        
        // Grid cell types
        private const int WALL = 0;
        private const int FLOOR = 1;
        private const int DOOR = 2;
        
        void Awake()
        {
            // Create parent object for organization
            dungeonParent = new GameObject("Generated Dungeon");
            dungeonParent.transform.parent = transform;
        }
        
        public void GenerateDungeon()
        {
            // Clear existing dungeon
            ClearDungeon();
            
            // Set random seed if specified
            if (generationSeed != 0)
            {
                Random.InitState(generationSeed);
            }
            else
            {
                Random.InitState(System.DateTime.Now.Millisecond);
            }
            
            // Initialize grid
            InitializeGrid();
            
            // Generate maze structure
            GenerateMaze();
            
            // Create rooms
            GenerateRooms();
            
            // Connect rooms with corridors
            ConnectRooms();
            
            // Clean up maze
            CleanupMaze();
            
            // Build 3D geometry
            BuildDungeon();
            
            // Place spawn points
            PlaceSpawnPoints();
            
            Debug.Log($"Dungeon generated with {rooms.Count} rooms");
        }
        
        void ClearDungeon()
        {
            // Destroy existing dungeon
            if (dungeonParent != null)
            {
                foreach (Transform child in dungeonParent.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            
            // Clear spawn points
            enemySpawnPoints.Clear();
            lootSpawnPoints.Clear();
            rooms.Clear();
        }
        
        void InitializeGrid()
        {
            dungeonGrid = new int[dungeonWidth, dungeonHeight];
            
            // Fill with walls
            for (int x = 0; x < dungeonWidth; x++)
            {
                for (int y = 0; y < dungeonHeight; y++)
                {
                    dungeonGrid[x, y] = WALL;
                }
            }
        }
        
        void GenerateMaze()
        {
            // Simple maze generation using recursive backtracking
            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            bool[,] visited = new bool[dungeonWidth, dungeonHeight];
            
            // Start from center
            Vector2Int start = new Vector2Int(dungeonWidth / 2, dungeonHeight / 2);
            stack.Push(start);
            visited[start.x, start.y] = true;
            dungeonGrid[start.x, start.y] = FLOOR;
            
            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();
                List<Vector2Int> neighbors = GetUnvisitedNeighbors(current, visited);
                
                if (neighbors.Count > 0)
                {
                    Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
                    
                    // Remove wall between current and chosen
                    Vector2Int wall = current + (chosen - current) / 2;
                    dungeonGrid[wall.x, wall.y] = FLOOR;
                    dungeonGrid[chosen.x, chosen.y] = FLOOR;
                    
                    visited[chosen.x, chosen.y] = true;
                    stack.Push(chosen);
                }
                else
                {
                    stack.Pop();
                }
            }
        }
        
        List<Vector2Int> GetUnvisitedNeighbors(Vector2Int pos, bool[,] visited)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();
            Vector2Int[] directions = { Vector2Int.up * 2, Vector2Int.down * 2, Vector2Int.left * 2, Vector2Int.right * 2 };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = pos + dir;
                
                if (IsValidPosition(neighbor) && !visited[neighbor.x, neighbor.y])
                {
                    neighbors.Add(neighbor);
                }
            }
            
            return neighbors;
        }
        
        void GenerateRooms()
        {
            int roomCount = Random.Range(3, maxRooms + 1);
            
            for (int i = 0; i < roomCount; i++)
            {
                int attempts = 0;
                while (attempts < 20) // Max attempts to place room
                {
                    int roomWidth = Random.Range(minRoomSize, minRoomSize + 3);
                    int roomHeight = Random.Range(minRoomSize, minRoomSize + 3);
                    
                    int x = Random.Range(2, dungeonWidth - roomWidth - 2);
                    int y = Random.Range(2, dungeonHeight - roomHeight - 2);
                    
                    RoomData newRoom = new RoomData(x, y, roomWidth, roomHeight);
                    
                    if (!RoomOverlaps(newRoom))
                    {
                        CreateRoom(newRoom);
                        rooms.Add(newRoom);
                        break;
                    }
                    
                    attempts++;
                }
            }
        }
        
        bool RoomOverlaps(RoomData newRoom)
        {
            foreach (RoomData existingRoom in rooms)
            {
                if (newRoom.Overlaps(existingRoom, 2)) // 2 cell buffer
                {
                    return true;
                }
            }
            return false;
        }
        
        void CreateRoom(RoomData room)
        {
            for (int x = room.x; x < room.x + room.width; x++)
            {
                for (int y = room.y; y < room.y + room.height; y++)
                {
                    if (IsValidPosition(new Vector2Int(x, y)))
                    {
                        dungeonGrid[x, y] = FLOOR;
                    }
                }
            }
        }
        
        void ConnectRooms()
        {
            // Connect each room to at least one other room
            for (int i = 0; i < rooms.Count; i++)
            {
                RoomData roomA = rooms[i];
                
                // Find closest room
                float closestDistance = float.MaxValue;
                RoomData closestRoom = null;
                
                for (int j = 0; j < rooms.Count; j++)
                {
                    if (i == j) continue;
                    
                    RoomData roomB = rooms[j];
                    float distance = Vector2.Distance(roomA.Center, roomB.Center);
                    
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestRoom = roomB;
                    }
                }
                
                if (closestRoom != null)
                {
                    CreateCorridor(roomA.Center, closestRoom.Center);
                }
            }
        }
        
        void CreateCorridor(Vector2Int start, Vector2Int end)
        {
            Vector2Int current = start;
            
            // Create L-shaped corridor
            while (current.x != end.x)
            {
                dungeonGrid[current.x, current.y] = FLOOR;
                current.x += current.x < end.x ? 1 : -1;
            }
            
            while (current.y != end.y)
            {
                dungeonGrid[current.x, current.y] = FLOOR;
                current.y += current.y < end.y ? 1 : -1;
            }
            
            dungeonGrid[end.x, end.y] = FLOOR;
        }
        
        void CleanupMaze()
        {
            // Remove isolated walls and clean up structure
            for (int x = 1; x < dungeonWidth - 1; x++)
            {
                for (int y = 1; y < dungeonHeight - 1; y++)
                {
                    if (dungeonGrid[x, y] == WALL)
                    {
                        int floorNeighbors = CountFloorNeighbors(x, y);
                        if (floorNeighbors >= 6) // Remove walls surrounded by too many floors
                        {
                            dungeonGrid[x, y] = FLOOR;
                        }
                    }
                }
            }
        }
        
        int CountFloorNeighbors(int x, int y)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    
                    int nx = x + dx;
                    int ny = y + dy;
                    
                    if (IsValidPosition(new Vector2Int(nx, ny)) && dungeonGrid[nx, ny] == FLOOR)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        
        void BuildDungeon()
        {
            for (int x = 0; x < dungeonWidth; x++)
            {
                for (int y = 0; y < dungeonHeight; y++)
                {
                    Vector3 position = new Vector3(x * roomSize, 0, y * roomSize);
                    
                    if (dungeonGrid[x, y] == FLOOR)
                    {
                        // Create floor
                        if (floorPrefab != null)
                        {
                            GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity, dungeonParent.transform);
                            floor.name = $"Floor_{x}_{y}";
                        }
                        
                        // Create ceiling
                        if (ceilingPrefab != null)
                        {
                            Vector3 ceilingPos = position + Vector3.up * wallHeight;
                            GameObject ceiling = Instantiate(ceilingPrefab, ceilingPos, Quaternion.identity, dungeonParent.transform);
                            ceiling.name = $"Ceiling_{x}_{y}";
                        }
                    }
                    else if (dungeonGrid[x, y] == WALL)
                    {
                        // Create wall
                        if (wallPrefab != null)
                        {
                            Vector3 wallPos = position + Vector3.up * (wallHeight / 2);
                            GameObject wall = Instantiate(wallPrefab, wallPos, Quaternion.identity, dungeonParent.transform);
                            wall.name = $"Wall_{x}_{y}";
                        }
                    }
                }
            }
        }
        
        void PlaceSpawnPoints()
        {
            // Place player spawn in first room
            if (rooms.Count > 0)
            {
                Vector2Int playerPos = rooms[0].Center;
                Vector3 worldPos = new Vector3(playerPos.x * roomSize, 1f, playerPos.y * roomSize);
                
                if (playerSpawnPoint != null)
                {
                    playerSpawnPoint.position = worldPos;
                }
                
                // Teleport player to spawn point
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    player.TeleportTo(worldPos);
                }
            }
            
            // Place enemy spawn points in other rooms
            for (int i = 1; i < rooms.Count; i++)
            {
                Vector2Int enemyPos = rooms[i].Center;
                Vector3 worldPos = new Vector3(enemyPos.x * roomSize, 0.5f, enemyPos.y * roomSize);
                
                GameObject spawnPoint = new GameObject($"EnemySpawn_{i}");
                spawnPoint.transform.position = worldPos;
                spawnPoint.transform.parent = dungeonParent.transform;
                enemySpawnPoints.Add(spawnPoint.transform);
            }
            
            // Place loot spawn points randomly in floor areas
            int lootCount = Random.Range(3, 8);
            for (int i = 0; i < lootCount; i++)
            {
                Vector2Int lootPos = GetRandomFloorPosition();
                if (lootPos != Vector2Int.zero)
                {
                    Vector3 worldPos = new Vector3(lootPos.x * roomSize, 0.5f, lootPos.y * roomSize);
                    
                    GameObject spawnPoint = new GameObject($"LootSpawn_{i}");
                    spawnPoint.transform.position = worldPos;
                    spawnPoint.transform.parent = dungeonParent.transform;
                    lootSpawnPoints.Add(spawnPoint.transform);
                }
            }
        }
        
        Vector2Int GetRandomFloorPosition()
        {
            List<Vector2Int> floorPositions = new List<Vector2Int>();
            
            for (int x = 0; x < dungeonWidth; x++)
            {
                for (int y = 0; y < dungeonHeight; y++)
                {
                    if (dungeonGrid[x, y] == FLOOR)
                    {
                        floorPositions.Add(new Vector2Int(x, y));
                    }
                }
            }
            
            if (floorPositions.Count > 0)
            {
                return floorPositions[Random.Range(0, floorPositions.Count)];
            }
            
            return Vector2Int.zero;
        }
        
        bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < dungeonWidth && pos.y >= 0 && pos.y < dungeonHeight;
        }
        
        public List<Transform> GetEnemySpawnPoints()
        {
            return enemySpawnPoints;
        }
        
        public List<Transform> GetLootSpawnPoints()
        {
            return lootSpawnPoints;
        }
        
        public Transform GetPlayerSpawnPoint()
        {
            return playerSpawnPoint;
        }
    }
}