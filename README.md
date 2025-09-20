# Procedural Dungeon Shooter

A 3D procedural dungeon shooter game built with Unity and C#. Features randomly generated dungeons, FPS mechanics, enemy AI, and perma-death gameplay.

## Features

- **Procedural Generation**: Each run creates a unique 3D dungeon layout with random enemy placement and loot spawns
- **FPS Mechanics**: First-person movement, mouse look, and shooting controls
- **Enemy System**: AI enemies with randomized stats and behaviors
- **Loot System**: Collectible items with random stats and upgrades
- **Perma-death**: Complete restart with new dungeon generation upon player death
- **Modular Design**: Clean, organized code structure for easy modification and learning

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs          # Main game loop and state management
│   │   └── SceneController.cs      # Scene loading and transitions
│   ├── Player/
│   │   ├── PlayerController.cs     # FPS movement and controls
│   │   ├── PlayerShooting.cs       # Weapon handling and shooting
│   │   └── PlayerHealth.cs         # Player health and death
│   ├── Dungeon/
│   │   ├── DungeonGenerator.cs     # Procedural dungeon creation
│   │   ├── RoomData.cs            # Room structure definitions
│   │   └── MazeGenerator.cs       # Maze layout algorithms
│   ├── Enemies/
│   │   ├── EnemyController.cs      # Base enemy behavior
│   │   ├── EnemyAI.cs             # AI pathfinding and combat
│   │   ├── EnemySpawner.cs        # Enemy placement system
│   │   └── EnemyStats.cs          # Randomized enemy attributes
│   ├── Loot/
│   │   ├── LootManager.cs         # Loot spawning and management
│   │   ├── Item.cs                # Base item class
│   │   └── ItemStats.cs           # Random item generation
│   └── UI/
│       ├── UIManager.cs           # Main UI controller
│       ├── HealthUI.cs            # Health display
│       └── GameOverUI.cs          # Death and restart screen
├── Scenes/
│   ├── MainMenu.unity             # Main menu scene
│   └── GameScene.unity            # Main game scene
├── Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── Loot/
│   └── Environment/
└── Materials/
    ├── Player/
    ├── Enemies/
    └── Environment/
```

## Setup Instructions

1. **Unity Version**: Unity 2022.3 LTS or newer recommended
2. **Clone Repository**: `git clone https://github.com/dhkiller350/rythm.git`
3. **Open in Unity**: Add project to Unity Hub and open
4. **Scene Setup**: Open `Assets/Scenes/GameScene.unity`
5. **Play**: Press Play button to start the game

## Getting Started

### Basic Controls
- **WASD**: Movement
- **Mouse**: Look around
- **Left Click**: Shoot
- **R**: Restart (after death)

### Gameplay Loop
1. Player spawns in a procedurally generated dungeon
2. Navigate through rooms while fighting enemies
3. Collect loot to improve stats
4. Upon death, restart with a completely new dungeon layout

## Customization

The modular design allows easy customization:

- **Dungeon Size**: Modify `DungeonGenerator.cs` parameters
- **Enemy Types**: Add new enemy prefabs and behaviors
- **Loot Variety**: Extend `Item.cs` and `ItemStats.cs`
- **Player Abilities**: Enhance `PlayerController.cs` and `PlayerShooting.cs`

## Development Notes

- Code follows Unity best practices and C# conventions
- Prefab-based design for easy asset management
- Configurable parameters via Unity Inspector
- Commented code for learning and modification
- Performance-optimized for smooth gameplay

## License

Licensed under the Apache License, Version 2.0. See LICENSE file for details.