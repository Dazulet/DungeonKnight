# Assignment 10 (RK2): submission checklist

Legend: [x] done and in the repository · [ ] you still need to do it

Deadline: Canvas closes when your practice lesson starts in week 11 (OR1-1 Wed 11 Nov 08:00 · OR1-2 Thu 12 Nov 11:00 · OR1-3 Thu 12 Nov 12:30).

## 1. What you build

- [x] Three enemy types with different behaviour: `PatrollerAI.cs`, `ShooterAI.cs`, `ChargerAI.cs` in `Assets/Scripts/Enemies/AI/`
- [x] A state machine for each, with 5 states and clear rules (see `Submission/RK2_Script_Map.md`)
- [x] Seeing the player: distance, angle and `Physics2D.Raycast`, and walls block it (`EnemyVision.cs`, used by all three)
- [x] Moving in 2D with waypoints (Patroller) and Rigidbody2D movement that steers around pillars
- [x] Fair attacks: a visible wind-up (Patroller 0.55 s, Shooter 0.7 s, Charger 0.65 s), a recovery and a cooldown. No attacks from off-screen or in the first 1.5 s.
- [x] All tuning numbers are `[SerializeField]` fields that you can change in the Inspector
- [x] One level with all three types together: `Assets/Scenes/Dungeon.unity` (2 Patrollers, 2 Shooters, 1 Charger), won by defeating all 5. A Victory screen shows the time and the try number.
- [x] Readable states: "!" and "?" icons, red body during wind-ups, blue stun, and state labels in the editor and development builds

## 2. Proof only you can make

- [x] **APK.** Release build after the balance change: `Builds/ReleaseAPK/DungeonKnight-RK2-release-balance1.apk` (package `com.daulet.dungeonknight`, not a development build, so the state labels are hidden). On this PC, Gradle needs the Java setting `-Djdk.net.unixdomain.tmpdir=C:/no_unix_socket_dir` to build.
- [x] **Test the APK on your own phone first.** The debug APK was tested on a real Android phone: the menu, movement, shooting, enemies, pause and Victory work.
- [x] **Playtest 1 (before the balance change):** two testers, my two younger brothers. They are two different people, and both played the game for the first time. Tester 1 won on the first try. Tester 2 won on the first try. Both said the game was too easy. Target: 2–4 tries.
- [ ] **Playtest 2 (after the balance change)** with new players who have never seen the game, on the phone:
  - Restart the app before each tester, so the counter starts at TRY 1.
  - Say only "play". Do not explain the enemies.
  - The HUD shows `TRY n`. The Victory and Game Over screens show the try number and the time.
  - Write down where each death happens and which enemy caused it.
  - Target: the level is beaten in 2–4 tries.
- [ ] **Playtest clip**, 1–2 minutes, showing both testers and their try counts
- [x] **Changed after playtest 1** (see `DECISIONS.md`): Patroller health 4 → 6, Charger health 5 → 7, Shooter cooldown 1.8 s → 1.4 s. Wind-ups, warnings and damage did not change.
- [ ] Record the tries in playtest 2, so you can show tries before → after.
- [ ] `Tools/generate_enemy_art.py` (the script that drew the new enemy sprites) needs `Tools/generate_art.py`. Both are still untracked in this repository. Commit them together if you want the art to be reproducible after cloning. The game itself does not need them, because the PNG files are committed.

## 3. AI and the decision log

- [x] `DECISIONS.md` in the repository root, with 5 short entries (3 about the enemies, 1 about moving the work into this repository, 1 about the playtest and the balance change)
- [ ] After playtest 2, add one short line with the new tries
- [x] This week's commits are in the repository: 6 feature commits from 8a601f6 to 546ba92 and the docs commit 053be6a, merged into `main` with pull request #1 (165c4ea)
- [x] Pushed to GitHub: `origin/main` is at 165c4ea
- [ ] Origin of the work: the same Assignment 10 code was first committed by mistake in another local project (github.com/Nurzhigit08/DungeonKnight). The README and `DECISIONS.md` say this. The commit messages themselves do not name the source. Agree with Nurzhigit what happens to the copy in his repository, and tell your instructor if they ask about it.
- [ ] Be ready to explain any script at the defense (weeks 14–15). Start with `EnemyBrain.cs`, `EnemyVision.cs` and one enemy script.

## 4. Video (required)

- [ ] 2–3 minutes, MP4, your own voice, in English. Use `Submission/RK2_Presentation_Script.md`.
- [ ] It shows the three enemies, their states, the script map (`Submission/RK2_Script_Map.png`) and what you changed after the playtest
- Gameplay screenshots for slides are in `Submission/Screenshots/RK2/`. Unity rendered them in Play mode during automated tests (camera only, no HUD). They are not human playtest footage.

## 5. Upload to Canvas

- [ ] Repository link in the submission comment
- [ ] `DungeonKnight.apk`
- [ ] Playtest clip (1–2 min)
- [ ] Video MP4 (2–3 min). Upload the file, not a link.

## 6. Not accepted (check you avoid these)

- [x] The APK starts on a phone (you tested the debug APK on a real Android phone; install the release APK once before you upload it)
- [x] Three enemies with different behaviour, not three skins
- [x] Commits from this week exist

## What was verified, and how

| Check | How | Result |
|---|---|---|
| C# compiles | Unity 6000.6.0f1 in batchmode | 0 errors |
| Each enemy's states, wind-ups, cooldowns, dodges, no damage after death | Automated Play Mode tests in batchmode | All passed |
| Walls block sight, melee and bolts | Automated Play Mode tests | All passed |
| No off-screen attacks, no attacks in the first 1.5 s | Automated Play Mode tests | All passed |
| Level, Victory, tries, Game Over, Endless mode | Automated Play Mode tests | All passed |
| HUD does not cover playable tiles on 16:9, 18:9, 20:9 phones | Calculation from the HUD and camera numbers | Clear (4:3 tablets still have some overlap) |
| Works on a physical phone | You tested the debug APK on a real Android phone | Menu, movement, shooting, enemies, pause and Victory work |
| After the balance change: enemy states, fairness, level | The same automated Play Mode tests, run again | All passed (138 checks, 0 failures) |
| Difficulty for new players (2–4 tries) | Playtest 1, before the balance change | Both testers won on the first try; playtest 2 is not done yet |
