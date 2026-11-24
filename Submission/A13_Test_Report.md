# Assignment 13 - Test Report

Date: 2026-11-24. Branch: `feature/juice-pass` (from `feature/data-and-saves`).
Unity 6000.6.0f1, URP 17.6.0 with the 2D Renderer, Gamma colour space.

**Where the tests ran:** all tests below ran in the Unity Editor on my Windows PC, in batch mode.
Nothing here is a result from a real Android phone. The phone tests are in
`A13_Android_Test_Checklist.md` and are **not done yet**.

## 1. Automated tests (Editor, batch mode)

The tests are Editor "probe" scripts. They are kept outside Git. Each one opens a scene, enters Play mode,
drives the game and prints PASS/FAIL lines.

**Result:** every old test still passes after the migration: 156/156 Assignment 10 checks and 182/182 Assignment 11 checks (with UiShots), plus 87/87 new checks. 0 errors or exceptions in the Console during the runs. The final run was done after the last scene rebuild.

| Test | What it checks | Before URP (baseline) | After URP + juice |
|---|---|---|---|
| PatrollerProbe | Patroller states and attack | 22/22 | 22/22 |
| ShooterProbe | Shooter aim, bolts, walls | 29/29 | 29/29 |
| ChargerProbe | Charger wind-up, dash, stun | 24/24 | 24/24 |
| FairnessProbe | line of sight, on-screen attacks | 14/14 | 14/14 |
| ReactionProbe | enemies react to hits | 15/15 | 15/15 |
| LevelProbe | level 1 layout and spawns | 34/34 | 34/34 |
| TouchProbe | joystick, shoot button, pause button | 18/18 | 18/18 |
| GameplayProbe (clear / 40 s) | bot plays, no errors | Victory, 0 errors / 0 errors | Victory, 0 errors / 0 errors |
| StageSmoke (Menu / Dungeon / Endless) | scenes load and play, no errors | 0 / 0 / 0 errors | 0 / 0 / 0 errors |
| DataProbe | ScriptableObject data is used | 27/27 | 27/27 |
| LevelFlowProbe | level select, locks, unlocks, next level | 65/65 | 65/65 |
| LevelClearProbe 1 / 2 / 3 | bot clears each level | Victory ×3 | Victory ×3 (12.3 s, 33.0 s, 61.3 s) |
| SaveProbe (unit, corrupt file, session 1, session 2) | save.json, broken file, restart | 29 + 6 + 6 + 18 | 29 + 6 + 6 + 18 |
| UiShotsProbe | menus and HUD fit on 1440/1920/2340/2400 × 1080 | 25/25 | 25/25 |
| **JuiceProbe (new)** | Assignment 13 features, see below | - | **87/87** |

### JuiceProbe - what the 87 checks cover

- **URP:** URP is the active pipeline, the renderer is the 2D Renderer, colour space is still Gamma,
  the camera renders post-processing, the Global Volume is on, Bloom / Color Adjustments / Vignette are active,
  the Vignette is weak (≤ 0.35), every renderer has a working material, no missing scripts.
- **Particles:** each of the 5 events spawns its prefab and the prefab shows particles
  (max alive: hit 6, death 14, player damage 10, coin 8, victory 44). Each prefab is a real ParticleSystem,
  does not loop, has Stop Action = Destroy, has at most 60 particles and is drawn below projectiles.
  After the run, **0 particle objects are left** (no leaks).
- **Shader Graph:** Patroller, Shooter, Charger, Slime and Bat use `Dungeon Knight/SpriteHitFlash`,
  which comes from `Assets/Art/Shaders/SpriteHitFlash.shadergraph`. In Play mode a hit sets `_FlashAmount`
  to about 0.85 on Patroller, Shooter and Charger, and it goes back to 0. The sprite colour (AI tint) does not change while it flashes.
- **Hit stop:** a normal hit does not stop time. A kill and player damage set timeScale to 0, and it is 1 again after 0.12 s.
  Pause during a hit stop stays paused; resume gives time back; disabling HitStop gives time back;
  restart and scene reload during a hit stop start with timeScale 1; victory and game over still freeze time;
  restart after game over gives timeScale 1; the main menu has timeScale 1.
- **Screen shake:** a kill starts a small camera shake (only with effects ON).
- **Effects switch:** the pause menu button changes EFFECTS ON/OFF and saves it in save.json.
  OFF: no new particles, no Volume, the old sprite vignette is back, the old red flash is used, no hit stop,
  live Assignment 13 particles are removed. The old DeathPuff and CoinSparkle still play.
- **FPS counter:** the FPS button shows and hides it; it shows a value like `FPS 59 LOW 31 FX OFF` (Editor value, not phone).
- **Old saves:** a save.json from Assignment 11 (without the new fields) loads with effects ON and FPS hidden,
  and keeps the old settings.

## 2. Visual checks (Editor screenshots)

I compared Editor screenshots before and after the migration (same scene, same size).
The Built-in and URP pictures look the same; URP adds a little contrast, colour and soft bloom on torches.
Screenshots checked: HUD at 4 screen sizes, menus, victory, pause menu at 1440/1920/2400 × 1080,
the FPS counter, the white flash, the particles, and EFFECTS ON vs OFF on the same frozen frame.

**One problem seen once:** in the very first test run after the URP import, one Editor screenshot showed
some wall tiles with the wrong picture. It did not happen again in three more runs with screenshots.
I think it was a first-import problem in the Editor capture, but I am not sure, so it is on the phone checklist.

## 3. Android builds

**The APK is not built yet.** The agent tried an IL2CPP ARM64 release build in batch mode. Unity finished
the player build (scenes, URP shaders, IL2CPP), and the report shows that all Assignment 13 assets were packed
(5 Fx prefabs, SpriteHitFlash material and Shader Graph, PixelParticle material, JuiceVolumeProfile).
Then Gradle stopped with `java.io.IOException: Unable to establish loopback connection`.
A small Java test in the same terminal gave the same error, so the problem is the agent's sandboxed terminal,
not the project. I build the APK myself in Unity:

- Release: menu **Dungeon Knight > Build Android APK** → `Builds/DungeonKnight.apk`
- Development (for FPS and Profiler): menu **Dungeon Knight > Build Android APK (Development + Profiler)**
  → `Builds/DungeonKnight-development.apk`

Update: Active Input Handling was "Both", which Unity 6.6 does not allow for Android, so my first build in the
Editor was cancelled. It is now "Input System Package (New)", the system the game already used. After the change,
all tests pass again (156 + 182 + 87), and an Android project export with Development + Profiler options finished
with 0 errors (Input Handling check, IL2CPP, URP shaders). Only the Gradle step is left for my own Unity Editor.

Result of my build: ______ (write OK / error and the APK size).

## 4. Not tested yet (needs my phone)

- FPS on a real Android phone with all effects ON (target: 30 or more).
- Unity Profiler connected to the phone.
- Touch controls, readability of effects and the hit flash on a real screen.
- Before/after video on the phone.
- Restart and save tests on the phone (also still open from Assignment 11).

## 5. Known limits

- Hit stop pauses the level timer for 0.05 s on each kill or hit, so a level time can be about 0.5 s longer. The timer limit is 2-3 minutes, so this does not change the game.
- The game asks Android for 60 FPS (`Application.targetFrameRate = 60`). This can use more battery than the old default of 30 FPS.
- The victory confetti plays under the victory panel after 0.9 s, so it is mostly seen in the moment before the panel opens.
