using UnityEngine;
using System.Collections.Generic;

namespace ProceduralDungeonShooter.Dungeon
{
    /// <summary>
    /// Additional maze generation algorithms for dungeon creation
    /// </summary>
    public static class MazeGenerator
    {
        public enum MazeType
        {
            RecursiveBacktracking,
            RandomizedPrim,
            CellularAutomata
        }
        
        /// <summary>
        /// Generate a maze using the specified algorithm
        /// </summary>
        public static int[,] GenerateMaze(int width, int height, MazeType type, int seed = 0)
        {
            if (seed != 0)
            {
                Random.InitState(seed);
            }
            
            switch (type)
            {
                case MazeType.RecursiveBacktracking:
                    return GenerateRecursiveBacktracking(width, height);
                case MazeType.RandomizedPrim:
                    return GenerateRandomizedPrim(width, height);
                case MazeType.CellularAutomata:
                    return GenerateCellularAutomata(width, height);
                default:
                    return GenerateRecursiveBacktracking(width, height);
            }
        }
        
        static int[,] GenerateRecursiveBacktracking(int width, int height)
        {
            int[,] maze = new int[width, height];
            bool[,] visited = new bool[width, height];
            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            
            // Initialize maze with walls
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    maze[x, y] = 0; // Wall
                }
            }
            
            // Start from random position
            Vector2Int start = new Vector2Int(
                Random.Range(1, width - 1) | 1, // Ensure odd coordinates
                Random.Range(1, height - 1) | 1
            );
            
            stack.Push(start);
            visited[start.x, start.y] = true;
            maze[start.x, start.y] = 1; // Floor
            
            Vector2Int[] directions = { Vector2Int.up * 2, Vector2Int.right * 2, Vector2Int.down * 2, Vector2Int.left * 2 };
            
            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();
                List<Vector2Int> neighbors = new List<Vector2Int>();
                
                // Find unvisited neighbors
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int neighbor = current + dir;
                    
                    if (IsValidMazePosition(neighbor, width, height) && !visited[neighbor.x, neighbor.y])
                    {
                        neighbors.Add(neighbor);
                    }
                }
                
                if (neighbors.Count > 0)
                {
                    Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
                    Vector2Int wall = current + (chosen - current) / 2;
                    
                    // Carve path
                    maze[wall.x, wall.y] = 1;
                    maze[chosen.x, chosen.y] = 1;
                    visited[chosen.x, chosen.y] = true;
                    
                    stack.Push(chosen);
                }
                else
                {
                    stack.Pop();
                }
            }
            
            return maze;
        }
        
        static int[,] GenerateRandomizedPrim(int width, int height)
        {
            int[,] maze = new int[width, height];
            List<Vector2Int> walls = new List<Vector2Int>();
            
            // Initialize maze with walls
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    maze[x, y] = 0;
                }
            }
            
            // Start with random cell
            Vector2Int start = new Vector2Int(Random.Range(1, width - 1), Random.Range(1, height - 1));
            maze[start.x, start.y] = 1;
            
            // Add walls to list
            AddWallsToList(start, walls, maze, width, height);
            
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            
            while (walls.Count > 0)
            {
                int randomIndex = Random.Range(0, walls.Count);
                Vector2Int wall = walls[randomIndex];
                walls.RemoveAt(randomIndex);
                
                // Check if wall divides a floor and wall
                List<Vector2Int> floorNeighbors = new List<Vector2Int>();
                
                foreach (Vector2Int dir in directions)
                {
                    Vector2Int neighbor = wall + dir;
                    
                    if (IsValidMazePosition(neighbor, width, height) && maze[neighbor.x, neighbor.y] == 1)
                    {
                        floorNeighbors.Add(neighbor);
                    }
                }
                
                if (floorNeighbors.Count == 1)
                {
                    maze[wall.x, wall.y] = 1;
                    AddWallsToList(wall, walls, maze, width, height);
                }
            }
            
            return maze;
        }
        
        static void AddWallsToList(Vector2Int cell, List<Vector2Int> walls, int[,] maze, int width, int height)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int wall = cell + dir;
                
                if (IsValidMazePosition(wall, width, height) && maze[wall.x, wall.y] == 0 && !walls.Contains(wall))
                {
                    walls.Add(wall);
                }
            }
        }
        
        static int[,] GenerateCellularAutomata(int width, int height, int iterations = 5)
        {
            int[,] maze = new int[width, height];
            
            // Initialize with random walls and floors
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        maze[x, y] = 0; // Border walls
                    }
                    else
                    {
                        maze[x, y] = Random.value < 0.45f ? 1 : 0; // 45% chance of floor
                    }
                }
            }
            
            // Apply cellular automata rules
            for (int i = 0; i < iterations; i++)
            {
                maze = ApplyCellularAutomataRules(maze, width, height);
            }
            
            return maze;
        }
        
        static int[,] ApplyCellularAutomataRules(int[,] maze, int width, int height)
        {
            int[,] newMaze = new int[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int wallCount = CountWallNeighbors(maze, x, y, width, height);
                    
                    if (wallCount > 4)
                    {
                        newMaze[x, y] = 0; // Wall
                    }
                    else if (wallCount < 4)
                    {
                        newMaze[x, y] = 1; // Floor
                    }
                    else
                    {
                        newMaze[x, y] = maze[x, y]; // Keep current state
                    }
                }
            }
            
            return newMaze;
        }
        
        static int CountWallNeighbors(int[,] maze, int x, int y, int width, int height)
        {
            int count = 0;
            
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    
                    int nx = x + dx;
                    int ny = y + dy;
                    
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    {
                        count++; // Count out-of-bounds as walls
                    }
                    else if (maze[nx, ny] == 0)
                    {
                        count++;
                    }
                }
            }
            
            return count;
        }
        
        static bool IsValidMazePosition(Vector2Int pos, int width, int height)
        {
            return pos.x > 0 && pos.x < width - 1 && pos.y > 0 && pos.y < height - 1;
        }
        
        /// <summary>
        /// Ensure the maze is connected by finding the largest connected area
        /// </summary>
        public static int[,] EnsureConnectivity(int[,] maze, int width, int height)
        {
            bool[,] visited = new bool[width, height];
            List<List<Vector2Int>> connectedAreas = new List<List<Vector2Int>>();
            
            // Find all connected areas
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (maze[x, y] == 1 && !visited[x, y])
                    {
                        List<Vector2Int> area = new List<Vector2Int>();
                        FloodFill(maze, visited, x, y, width, height, area);
                        
                        if (area.Count > 0)
                        {
                            connectedAreas.Add(area);
                        }
                    }
                }
            }
            
            // Find largest connected area
            if (connectedAreas.Count > 1)
            {
                List<Vector2Int> largestArea = connectedAreas[0];
                foreach (var area in connectedAreas)
                {
                    if (area.Count > largestArea.Count)
                    {
                        largestArea = area;
                    }
                }
                
                // Clear maze and keep only largest area
                int[,] newMaze = new int[width, height];
                foreach (var pos in largestArea)
                {
                    newMaze[pos.x, pos.y] = 1;
                }
                
                return newMaze;
            }
            
            return maze;
        }
        
        static void FloodFill(int[,] maze, bool[,] visited, int x, int y, int width, int height, List<Vector2Int> area)
        {
            if (x < 0 || x >= width || y < 0 || y >= height || visited[x, y] || maze[x, y] == 0)
                return;
            
            visited[x, y] = true;
            area.Add(new Vector2Int(x, y));
            
            // Recursively fill neighbors
            FloodFill(maze, visited, x + 1, y, width, height, area);
            FloodFill(maze, visited, x - 1, y, width, height, area);
            FloodFill(maze, visited, x, y + 1, width, height, area);
            FloodFill(maze, visited, x, y - 1, width, height, area);
        }
    }
}