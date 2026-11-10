# Assignment 10: presentation script (about 2:40)

**Status: script only. The video is not recorded yet, and the playtest has not happened yet.** Fill in the parts in [square brackets] with your real playtest results before you record.

## Before you record

- Open the project in Unity 6000.6.0f1 and open `Assets/Scenes/MainMenu.unity`.
- Play in the editor, so the state labels (PATROL, AIM, WINDUP…) are visible under each enemy. They are hidden in the release APK.
- Have these ready in tabs: `Submission/RK2_Script_Map.png`, `DECISIONS.md` on GitHub, and your playtest clip.
- Record the screen at 1920 × 1080 with OBS (or the Xbox Game Bar, Win + Alt + R) and your own voice. Export as MP4.
- Speak slowly. About 130 words per minute is fine.

| Time | On screen | What to say |
|------|-----------|-------------|
| 0:00 – 0:15 | Main menu, then press PLAY. | "Hi, I'm Daulet. This is Dungeon Knight, my 2D mobile game. For Assignment 10 I added three new enemy types. Each one thinks in a different way, and each one tests a different skill." |
| 0:15 – 0:40 | Walk near the top Patroller. Let it see you, then step out of the red circle. | "The first enemy is the Patroller. It walks between waypoints. When it sees me, it shows an exclamation mark and chases me. Before it hits, it stops, turns red, and a red circle fills for about half a second. If I step out of the circle, I am safe. This enemy tests my positioning." |
| 0:40 – 1:05 | Walk into the Shooter's view. Wait for the line, then step aside. | "The second enemy is the Shooter. It stays at a distance. When it aims, a red line follows me. Then the line turns yellow and locks. That is my signal to step aside. Walls stop its bolts. This enemy tests my reaction." |
| 1:05 – 1:30 | Stand in front of the Charger. Step out of the lane, then hit it while it is blue. | "The third enemy is the Charger. It shows a wide red lane, shakes, and then dashes in a straight line. It cannot turn during the dash. After the dash it is stunned and blue for almost a second, so I can attack it. This enemy tests my timing." |
| 1:30 – 1:50 | Hide behind a pillar from a Patroller, then shoot an enemy from behind. | "To see me, every enemy checks three things: the distance, the angle, and a raycast. Walls block the raycast, so I can hide behind pillars. If I shoot an enemy from behind, it turns to the shot and shows a question mark, but it still cannot see through walls." |
| 1:50 – 2:15 | Show `RK2_Script_Map.png`. Point at each column, then at the bottom box. | "This is my script map. Each enemy has its own script and its own state machine with five states. They share only the basic parts: the state machine, vision, movement, and the attack warning. When all five enemies die, the level goal tells the game manager, and I see the Victory screen." |
| 2:15 – 2:35 | Show your playtest clip or photos, then `DECISIONS.md`. | "Two new players tested the game on my phone. [Tester 1] needed [N] tries and [Tester 2] needed [N] tries. After the test I changed [parameter 1] from [old value] to [new value], and [parameter 2] from [old value] to [new value], because [short reason]. My decision log is in DECISIONS.md." |
| 2:35 – 2:45 | Back to the game. Kill the last enemy and show the Victory screen. | "That's my work for Assignment 10. Thank you for watching." |

## If you need to make it shorter

Cut the 1:30 – 1:50 part (vision) to one sentence: "Every enemy checks distance, angle and a raycast, so walls block its sight." Keep the three enemies, the script map and the playtest part, because they carry most of the grade.

## Words that may be hard to say

- **telegraph**: a warning before an attack
- **raycast**: an invisible line from the enemy to the player
- **state machine**: a list of states and the rules to move between them
