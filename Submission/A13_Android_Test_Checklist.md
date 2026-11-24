# Assignment 13 - Android Test Checklist

Use this list on a real Android phone. Write the real result next to each line.
Do not copy results from the Unity Editor: the Editor is not the phone.

Phone model: ______________________  Android version: ________  Date: __________

## 0. Install

1. Open the project in Unity 6000.6.0f1 (first open after URP takes a few minutes).
2. Build the **development** APK: menu **Dungeon Knight > Build Android APK (Development + Profiler)**
   → `Builds/DungeonKnight-development.apk`. Use it for the FPS and Profiler evidence.
3. Build the **release** APK: menu **Dungeon Knight > Build Android APK** → `Builds/DungeonKnight.apk`.
   Use it for Canvas. You can rename it to `DungeonKnight-A13-release.apk`.
4. Connect the phone with USB. Turn on Developer options and USB debugging.
5. Install with `adb install -r <file>.apk`, or copy the file to the phone and open it.

The two APKs have the same package name, so the second install replaces the first one.
Your save file stays.

## 1. Old features still work (Assignment 10 and 11)

- [ ] The game opens in landscape. The menu buttons react to touch.
- [ ] PLAY opens Level Select. Level 1 is open. Locked levels show a lock.
- [ ] The joystick moves the knight. The shoot button fires. Auto aim works.
- [ ] The pause button pauses. RESUME, RESTART and MAIN MENU work.
- [ ] Win Level 1. VICTORY shows. Level 2 is now unlocked.
- [ ] Close the app fully and open it again. Level 2 is still unlocked. Best score is still there.
- [ ] Settings: SCREEN SHAKE and AUTO AIM still switch ON/OFF and are remembered after a restart.

## 2. The five particle effects (EFFECTS: ON)

- [ ] Enemy hit: small white-yellow sparks on the enemy.
- [ ] Enemy death: a burst of pale purple pixels and the old grey puff.
- [ ] Player damage: red pixels around the knight.
- [ ] Coin pickup: small gold pixels go up from the coin.
- [ ] Level victory: coloured confetti from the knight, still moving on the VICTORY screen.
- [ ] No effect hides an enemy, an enemy bolt, the joystick, the shoot button or the HUD.

## 3. Shader Graph hit flash

- [ ] Hit a Patroller, a Shooter and a Charger (Level 2 has all three).
- [ ] Each enemy turns **white** for a very short time when it is hit.
- [ ] An enemy in its red "wind-up" state stays red-tinted and still flashes white.
- [ ] No pink/magenta sprites anywhere (pink means a broken shader).
- [ ] No wrong sprites: walls look like walls, pillars like pillars (once, in an Editor test
      screenshot right after the URP import, some tiles showed the wrong picture; it did not happen again).

## 4. Post-processing, screen shake, hit stop

- [ ] With EFFECTS: ON the image has soft bloom on bright things (coins, bolts, torches), a bit more colour, and a soft dark border.
- [ ] Nothing is too bright or blurred. The text is sharp.
- [ ] When an enemy dies or the knight is hit, the game stops for a very short moment (hit stop). It always continues.
- [ ] Pause right after a kill, then RESUME: the game continues normally.
- [ ] Kill the last enemy: VICTORY appears after about 1 second, like before.
- [ ] Lose all hearts: GAME OVER appears. TRY AGAIN starts the level at normal speed.

## 5. Effects ON/OFF switch (pause menu)

- [ ] Open pause. Press **EFFECTS: ON** - it changes to **EFFECTS: OFF**.
- [ ] With OFF: no new particles, no bloom/colour/vignette, old red hit flash, no hit stop.
- [ ] With OFF: the old effects still work (grey death puff, hit sparks flipbook, coin sparkle, screen shake on damage).
- [ ] Close the app and open it again: the EFFECTS setting is remembered.
- [ ] Switch back to **EFFECTS: ON** before the FPS test.

## 6. Evidence 1 - FPS screenshot (required)

1. Install the development APK. Open **Level 3** (the hardest level, most enemies).
2. Pause, press **FPS: OFF** so it shows **FPS: ON**, and make sure it says **EFFECTS: ON**. Press RESUME.
3. The counter is at the top left, under the hearts: `FPS 60  LOW 45  FX ON`.
   - `FPS` = average frames per second for the last half second.
   - `LOW` = the slowest single frame in that half second, as FPS.
4. Play for 30 seconds, shoot enemies and collect coins so particles are on screen.
5. Take a phone screenshot (usually Power + Volume Down) while particles are visible.
6. The screenshot must show `FX ON` and an FPS value of 30 or more.
7. Write the numbers here: average FPS ______  lowest LOW ______

The game asks for 60 FPS (`Application.targetFrameRate = 60`). If your phone shows less than 30,
write it down honestly and tell me. Do not edit the screenshot.

## 7. Evidence 2 - Unity Profiler connected to the phone (required)

1. Use the **development** APK (a release APK cannot connect to the Profiler).
2. Connect the phone by USB. In Unity: **Window > Analysis > Profiler**.
3. In the Profiler toolbar, open the target list ("Play Mode" / "Editor") and choose your phone
   (`AndroidPlayer(...)` or the USB device). If it is not listed, run once in a terminal:
   `adb forward tcp:34999 localabstract:Unity-com.daulet.dungeonknight` and choose `<Enter IP>` = `127.0.0.1:34999`.
4. Make sure the record button is red. Play Level 3 with EFFECTS: ON for 20-30 seconds.
5. Click a frame during a fight. The CPU graph and the frame time must be visible,
   and the device name must be visible in the target list.
6. Take a **PC screenshot** of the whole Unity window (Win + Shift + S).
7. Optional: do the same with EFFECTS: OFF for a comparison.

## 8. Before/After video (same situation)

1. Start Level 1 with EFFECTS: OFF. Play 20 seconds: kill 2 enemies, take 1 hit, pick up 1 coin.
2. Press RESTART. Pause, switch EFFECTS: ON, resume, and do the same moves.
3. Or: pause at an interesting moment and switch EFFECTS ON/OFF while paused - the frozen
   picture changes (particles, white flash, bloom and vignette appear or disappear).
4. Use the phone screen recorder. Keep the clips for the video.
