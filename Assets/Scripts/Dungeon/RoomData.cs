using UnityEngine;

namespace ProceduralDungeonShooter.Dungeon
{
    /// <summary>
    /// Data structure representing a room in the dungeon
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        public int x;
        public int y;
        public int width;
        public int height;
        
        public RoomData(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        
        public Vector2Int Center
        {
            get { return new Vector2Int(x + width / 2, y + height / 2); }
        }
        
        public Vector2Int TopLeft
        {
            get { return new Vector2Int(x, y); }
        }
        
        public Vector2Int TopRight
        {
            get { return new Vector2Int(x + width - 1, y); }
        }
        
        public Vector2Int BottomLeft
        {
            get { return new Vector2Int(x, y + height - 1); }
        }
        
        public Vector2Int BottomRight
        {
            get { return new Vector2Int(x + width - 1, y + height - 1); }
        }
        
        public bool ContainsPoint(Vector2Int point)
        {
            return point.x >= x && point.x < x + width && 
                   point.y >= y && point.y < y + height;
        }
        
        public bool Overlaps(RoomData other, int buffer = 0)
        {
            return !(x + width + buffer <= other.x || 
                     other.x + other.width + buffer <= x ||
                     y + height + buffer <= other.y || 
                     other.y + other.height + buffer <= y);
        }
        
        public float DistanceTo(RoomData other)
        {
            return Vector2.Distance(Center, other.Center);
        }
        
        public Vector2Int GetRandomPoint()
        {
            return new Vector2Int(
                Random.Range(x, x + width),
                Random.Range(y, y + height)
            );
        }
        
        public int Area
        {
            get { return width * height; }
        }
        
        public override string ToString()
        {
            return $"Room({x}, {y}, {width}x{height})";
        }
    }
}