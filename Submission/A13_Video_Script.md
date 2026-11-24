# Assignment 13 - Video Script (2-3 minutes)

Record on the real phone (screen recorder) and on the PC (Unity Profiler, Script Map).
Speak slowly. The text is simple B1 English. Times are a guide.

## What to record before you edit

| Clip | What is on screen | Where |
|---|---|---|
| A | Level 1, EFFECTS: OFF, 15-20 s: kill 2 enemies, take 1 hit, take 1 coin | phone |
| B | Restart Level 1, EFFECTS: ON, the same moves | phone |
| C | Pause at a fight moment, switch EFFECTS ON / OFF / ON while paused | phone |
| D | Level 2: hit a Patroller, a Shooter and a Charger (white flash) | phone |
| E | Kill the last enemy of a level: hit stop, confetti, VICTORY | phone |
| F | Level 3 with FPS: ON and EFFECTS: ON (counter visible) | phone |
| G | Unity Profiler connected to the phone during a fight | PC |
| H | `Submission/A13_Script_Map.png` | PC |
| I | Optional: the SpriteHitFlash Shader Graph open in Unity | PC |

## Script

**0:00 - 0:15 - Intro (clip A starts)**
"Hi, I am Daulet. This is Dungeon Knight, my 2D top-down game for Android.
This week was Assignment 13, the juice pass. I want to show the same fight without effects and with effects."

**0:15 - 0:40 - Before and after (clips A and B)**
"This is Level 1 with effects OFF. The game works, but hits feel flat.
Now the same fight with effects ON. Enemies flash white, sparks fly, the screen stops for a very short moment
when an enemy dies, and the camera shakes a little."

**0:40 - 1:05 - Five particle events (clips B and E)**
"I added real Unity Particle Systems for five events:
an enemy hit, an enemy death, player damage, a coin pickup and the level victory.
They are small square pixels, so they match the pixel art. They live less than half a second
and destroy themselves, so they do not hide enemies, bolts or my buttons.
The victory confetti uses unscaled time, so it still moves on the victory screen."

**1:05 - 1:25 - Shader Graph (clips D and I)**
"This white flash is a Shader Graph. It is a Sprite Unlit graph. It multiplies the sprite by the vertex colour
and then mixes it to white with a value called Flash Amount.
My HitFlash script sets this value with a Material Property Block, so no new material is created on a hit.
The enemy keeps its red wind-up colour while it flashes."

**1:25 - 1:45 - Post-processing, hit stop, shake (clip C)**
"I moved the project from the Built-in pipeline to URP with the 2D Renderer.
A Global Volume adds soft Bloom, a little more contrast and colour, and a weak Vignette.
I removed my old sprite vignette when effects are on, so the corners are not dark twice.
Hit stop is 0.05 seconds. It only happens on a kill or when I take damage, not on every shot,
and it never breaks pause, victory, game over or restart."

**1:45 - 2:05 - The switch (clip C)**
"In the pause menu there is an EFFECTS button. It turns off only the new Assignment 13 effects.
Old things like the death puff and screen shake still work. Here I switch it while the game is paused,
so you see the same moment with and without effects."

**2:05 - 2:30 - Performance (clips F and G)**
"Here is the FPS counter on my phone, model ______, with all effects on in Level 3:
it shows about ___ FPS, and the lowest frame is about ___.
And here is the Unity Profiler connected to the phone during a fight."
(Say the real numbers from your screenshot. Do not guess.)

**2:30 - 2:50 - Script map and AI help (clip H)**
"This is my script map. Game events call small helpers: JuiceFx for particles, HitStop, CameraShake and HitFlash.
The pause menu saves the setting, and JuiceView turns the Volume on or off.
I used Claude Code as an assistant. It moved the project to URP through my builder scripts and wrote 87 automatic checks:
for example, pause during a hit stop, restart during a hit stop, and an old save file without the new settings.
All my old Assignment 10 and 11 tests still pass. The phone results in this video are my own measurements."

**2:50 - 3:00 - End**
"Thank you for watching."

## Notes

- If a number on the phone is different from this script, say the real number.
- Keep the video under 3 minutes. Cut long walking parts.
- Show the FPS counter and the Profiler clearly for 3-5 seconds each.
