Dungeon Knight
Dungeon Knight is a 2D pixel-art survival game made in Unity.

Gameplay
The player controls a knight, fights enemies, collects coins, and tries to survive as long as possible.

Controls
WASD - Move
Space - Shoot
ESC - Pause

Mobile - Joystick and attack button
Features
Main Menu
Health and Score System
Enemy Spawning
Pause and Restart
Mobile UI

Assignment 10 - Three Enemy Types
The game now has two modes in the main menu:
PLAY - clear a dungeon level guarded by three enemy types. Defeat all 5 enemies to see the Victory screen.
ENDLESS - the original survival mode with slimes and bats.

The three enemies:
Patroller - walks between waypoints, chases the player and hits with a short melee attack after a red circle warning (0.55 s).
Shooter - keeps its distance, shows a red aim line that turns yellow and locks, then fires one bolt (0.7 s warning). Walls stop the bolts.
Charger - shows a wide red lane, then dashes in a straight line and is stunned after the dash (0.65 s warning).

Each enemy has its own state machine with 5 states. To see the player, every enemy checks the distance, the angle and a Physics2D.Raycast, so walls block its sight. Every attack has a visible wind-up, a recovery and a cooldown. Enemies show "!" when they see the player and "?" when they search.

The HUD shows TRY n and ENEMIES n in the level. The try counter helps to count tries in a playtest.

Assignment 11 - Data and Saves
PLAY now opens a level select with three levels. Level 1 is open; winning a level unlocks the next one. Locked levels cannot be started.
All balance numbers live in ScriptableObject assets in Assets/Data (PatrollerStats, ShooterStats, ChargerStats, SlimeStats, BatStats, KnightStats, CoinData, EndlessData). Change a value in the Inspector and the game changes without code changes.
Each level is a LevelData asset in Assets/Data/Levels (enemies, positions, patrol routes, spawn delays, time limit). Assets/Resources/LevelCatalog.asset lists them in order. The same Dungeon scene loads any level.
Progress, best scores and settings (screen shake, auto aim) are saved with JsonUtility to save.json in Application.persistentDataPath: after a win, on every settings change and when the app is paused. A missing or broken file never crashes the game: it is kept as save.broken.json and the game starts with defaults. SETTINGS > RESET PROGRESS asks for confirmation first.

Project: Unity 6000.6.0f1. To build the APK, install the Android Build Support module in Unity Hub, then use the menu Dungeon Knight > Build Android APK.

Documentation: Submission/RK2_Script_Map.md (scripts and states), Submission/RK2_Script_Map.png, Submission/RK2_Presentation_Script.md, Submission/RK2_Submission_Checklist.md and DECISIONS.md.
Assignment 11: Submission/A11_Script_Map.md, Submission/A11_Script_Map.png, Submission/A11_Presentation_Script.md, Submission/A11_Demo_Instructions.md and Submission/A11_Submission_Checklist.md.

Origin of this work: I built Assignment 10 with Claude Code. By mistake, it was first committed in another local project folder (github.com/Nurzhigit08/DungeonKnight). It was then moved into this repository stage by stage. See DECISIONS.md.
