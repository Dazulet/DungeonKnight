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

Assignment 13 - Juice Pass
The project now uses URP 17.6 with the 2D Renderer (Gamma colour space). A Global Volume adds soft Bloom, Color Adjustments and a weak Vignette.
Five Particle Systems play on enemy hit, enemy death, player damage, coin pickup and level victory. Enemies flash white with the SpriteHitFlash Shader Graph. Kills and player damage cause a 0.05 s hit stop and a small screen shake.
The pause menu has EFFECTS ON/OFF (turns off only the Assignment 13 effects, for before/after comparisons) and FPS ON/OFF (a small counter under the hearts). Both are saved in save.json.

Assignment 12 - Sound Pass
Audio goes through a Unity AudioMixer (Assets/Audio/GameMixer.mixer): Master > Music > Music Pause, and Master > SFX. A Compressor on Master keeps loud moments under control. One GameAudio object (created at start, kept between scenes) owns a fixed pool of 12 SFX AudioSources and 2 music AudioSources. Every source is routed to a mixer group; the camera keeps the only AudioListener.
Sounds: footsteps, player shot, enemy hit, enemy death, player hurt, coin pickup, Patroller wind-up + slam, Shooter aim + shot, Charger wind-up + dash, Slime/Bat bite, UI click, victory and defeat. Each enemy warning sound starts with its red telegraph, before the attack. Repeated sounds use pitch 0.92-1.08 and 2-3 alternative clips. Each sound has a voice limit and a minimum interval, so many hits at once do not stack forever.
Music: a calm menu track and a faster level track. A scene change crossfades them in 1.5 s. Restart and the next level use the same Dungeon scene, so the level track keeps playing and is not restarted.
SETTINGS has MUSIC and SFX sliders (0-100%). The value goes to the mixer as 20*log10(max(value, 0.0001)) dB and is saved in save.json (musicVolume, sfxVolume). Old save files without these fields get 80%.
Pause (and the victory / game over screens) switches to the Paused snapshot: music -10 dB and a 900 Hz low-pass. Resume goes back to the Normal snapshot. The mixer runs on unscaled time, so this works while timeScale is 0.

Audio files and licences
All audio is original. I made it with Claude Code by procedural synthesis in Python (numpy): sine, square, triangle and saw oscillators, filtered noise, envelopes and simple echo. No samples, recordings or music by other people were used. Script: Tools/generate_audio.py (run "python Tools/generate_audio.py" to make the same files again). Licence: made for this project, same terms as the project code.
Assets/Audio/Sfx (44.1 kHz, 16-bit mono WAV):
footstep_1, footstep_2, footstep_3 - player steps (low thump + filtered noise)
player_shoot_1, player_shoot_2 - magic bolt (falling square tone + hiss)
enemy_hit_1, enemy_hit_2 - hit on an enemy (noise crack + falling knock)
enemy_death - enemy death (falling square tone + crunch)
player_hurt - knight takes damage (falling square tone with tremolo)
coin_1, coin_2 - coin pickup (two-note chime)
patroller_windup - Patroller warning (rising growl, 0.50 s)
patroller_slam - Patroller hit (deep boom + debris)
shooter_aim - Shooter warning (rising whine with a lock tick, 0.66 s)
shooter_fire - Shooter bolt (zap)
charger_windup - Charger warning (two snorts + rising rumble, 0.62 s)
charger_dash - Charger dash (air whoosh + stomp)
chaser_bite - Slime/Bat contact bite
ui_click - button click
victory - short rising jingle
defeat - short falling jingle
Assets/Audio/Music (32 kHz, 16-bit mono WAV, seamless loops):
music_menu - menu theme, A minor, 92 BPM, 41.7 s loop (pad, arpeggio, bass, soft melody)
music_level - level theme, D minor, 128 BPM, 30.0 s loop (bass line, drums, arpeggio, lead)

Project: Unity 6000.6.0f1. To build the APK, install the Android Build Support module in Unity Hub, then use the menu Dungeon Knight > Build Android APK.

Documentation: Submission/RK2_Script_Map.md (scripts and states), Submission/RK2_Script_Map.png, Submission/RK2_Presentation_Script.md, Submission/RK2_Submission_Checklist.md and DECISIONS.md.
Assignment 11: Submission/A11_Script_Map.md, Submission/A11_Script_Map.png, Submission/A11_Presentation_Script.md, Submission/A11_Demo_Instructions.md and Submission/A11_Submission_Checklist.md.
Assignment 12: Submission/A12_Script_Map.png, Submission/A12_Test_Report.md and Submission/A12_Video_Script.md.
Assignment 13: Submission/A13_Script_Map.png, Submission/A13_Test_Report.md, Submission/A13_Android_Test_Checklist.md and Submission/A13_Video_Script.md.

Origin of this work: I built Assignment 10 with Claude Code. By mistake, it was first committed in another local project folder (github.com/Nurzhigit08/DungeonKnight). It was then moved into this repository stage by stage. See DECISIONS.md.
