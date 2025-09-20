# Unity Setup Instructions

## Setting up the Procedural Dungeon Shooter in Unity

### Requirements
- Unity 2022.3 LTS or newer
- Git for version control

### Step-by-Step Setup

#### 1. Create New Unity Project
1. Open Unity Hub
2. Click "New Project"
3. Select "3D Core" template
4. Set project name to "ProceduralDungeonShooter"
5. Set location to your desired folder
6. Click "Create Project"

#### 2. Import the Scripts
1. Copy all files from the repository's `Assets` folder into your Unity project's `Assets` folder
2. Unity will automatically import and compile the scripts

#### 3. Basic Scene Setup

##### Create the Game Scene:
1. Create new scene: `File > New Scene > Basic (Built-in Renderer)`
2. Save as `Assets/Scenes/GameScene.unity`

##### Set up the Game Manager:
1. Create empty GameObject: `GameObject > Create Empty`
2. Rename to "GameManager"
3. Add `GameManager` script component
4. Add `SceneController` script component

##### Set up the Player:
1. Create empty GameObject: `GameObject > Create Empty`
2. Rename to "Player"
3. Add the following components:
   - `Character Controller`
   - `PlayerController` script
   - `PlayerHealth` script
   - `PlayerShooting` script
4. Create child Camera:
   - `GameObject > Camera`
   - Position at (0, 1.8, 0)
   - Assign to PlayerController's "Player Camera" field

##### Set up the Dungeon Generator:
1. Create empty GameObject: `GameObject > Create Empty`
2. Rename to "DungeonGenerator"
3. Add `DungeonGenerator` script component

##### Set up Enemy Spawner:
1. Create empty GameObject: `GameObject > Create Empty`
2. Rename to "EnemySpawner"
3. Add `EnemySpawner` script component

##### Set up Loot Manager:
1. Create empty GameObject: `GameObject > Create Empty`
2. Rename to "LootManager"
3. Add `LootManager` script component

##### Set up UI:
1. Create UI Canvas: `GameObject > UI > Canvas`
2. Add `UIManager` script to Canvas
3. Create basic UI elements as children of Canvas:
   - Health Bar (UI > Slider)
   - Ammo Counter (UI > Text)
   - Crosshair (UI > Image)
   - Game Over Panel (UI > Panel)

#### 4. Create Prefabs

##### Floor Prefab:
1. Create a Cube: `GameObject > 3D Object > Cube`
2. Scale to (10, 0.1, 10)
3. Add material (Create > Material, set color to gray)
4. Drag to Assets/Prefabs/Environment folder to create prefab
5. Delete from scene

##### Wall Prefab:
1. Create a Cube: `GameObject > 3D Object > Cube`
2. Scale to (10, 3, 1)
3. Add material (Create > Material, set color to brown)
4. Drag to Assets/Prefabs/Environment folder to create prefab
5. Delete from scene

##### Basic Enemy Prefab:
1. Create a Capsule: `GameObject > 3D Object > Capsule`
2. Add the following components:
   - `Nav Mesh Agent`
   - `EnemyController` script
   - `EnemyAI` script
   - `EnemyStats` script
3. Set Capsule Collider to trigger
4. Add "Enemy" tag
5. Drag to Assets/Prefabs/Enemies folder to create prefab
6. Delete from scene

##### Basic Loot Prefab:
1. Create a Sphere: `GameObject > 3D Object > Sphere`
2. Scale to (0.5, 0.5, 0.5)
3. Add the following components:
   - `Sphere Collider` (set as trigger)
   - `Item` script
   - `ItemStats` script
4. Add "Loot" tag
5. Drag to Assets/Prefabs/Loot folder to create prefab
6. Delete from scene

#### 5. Configure Components

##### DungeonGenerator Configuration:
- Assign Floor Prefab to "Floor Prefab" field
- Assign Wall Prefab to "Wall Prefab" field
- Set dungeon size (recommend 20x20 for testing)

##### EnemySpawner Configuration:
- Assign Enemy Prefab to "Enemy Prefabs" array
- Set min/max enemies per room

##### LootManager Configuration:
- Assign Loot Prefab to "Loot Prefabs" array

##### PlayerShooting Configuration:
- Create a Line Renderer component for laser line
- Configure weapon settings (damage, fire rate, etc.)

#### 6. Build Settings and NavMesh

##### Build Settings:
1. Go to `File > Build Settings`
2. Add current scene to "Scenes in Build"
3. Set target platform

##### NavMesh Baking:
1. Select all wall prefabs
2. Mark as "Navigation Static" in Inspector
3. Go to `Window > AI > Navigation`
4. Click "Bake" to generate NavMesh

#### 7. Testing the Game

1. Press Play in Unity Editor
2. Use WASD to move, mouse to look around
3. Left click to shoot
4. Watch for procedurally generated dungeon
5. Test enemy spawning and AI
6. Test loot collection
7. Test death and restart functionality

### Troubleshooting

**Common Issues:**

1. **Scripts not compiling:**
   - Check for missing using statements
   - Ensure all scripts are in correct folders
   - Check Unity Console for errors

2. **Player not moving:**
   - Ensure Character Controller is properly configured
   - Check player input settings

3. **Enemies not spawning:**
   - Ensure NavMesh is baked
   - Check enemy prefab configuration
   - Verify spawn point generation

4. **Dungeon not generating:**
   - Check DungeonGenerator prefab assignments
   - Ensure floor/wall prefabs are assigned

5. **UI not working:**
   - Check Canvas settings
   - Ensure UI elements are properly assigned in UIManager

### Performance Tips

1. Use object pooling for enemies and projectiles
2. Limit dungeon size for better performance
3. Use LOD (Level of Detail) for distant objects
4. Optimize materials and textures

### Next Steps for Enhancement

1. Add sound effects and background music
2. Create more diverse enemy types
3. Add particle effects for shooting and explosions
4. Implement more weapon types
5. Add power-ups and special abilities
6. Create multiple dungeon themes
7. Add multiplayer support