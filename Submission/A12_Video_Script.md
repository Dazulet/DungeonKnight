# Assignment 12 - Video Script (2-3 minutes)

Record the game with sound. On the PC: Unity Game view with the Audio Mixer window open
(Window > Audio > Audio Mixer). On the phone: screen recorder with internal audio ON.
Speak slowly, in simple English. Times are a guide.

## What to record before you edit

| Clip | What is on screen | Where |
|---|---|---|
| A | Main menu with menu music, click SETTINGS, move MUSIC and SFX sliders | PC or phone |
| B | Audio Mixer window: groups Master, Music, Music Pause, SFX; snapshots Normal and Paused | PC |
| C | PLAY > Level 1: music crossfades from menu to level | PC or phone |
| D | Walk, shoot, hit and kill an enemy, take a coin | PC or phone |
| E | Stand near a Patroller, a Shooter and a Charger: warning sound, then attack | PC or phone |
| F | Pause and Resume during a fight (music becomes quiet and muffled, then normal) | PC or phone |
| G | RESTART: the level music does not start again from the beginning | PC or phone |
| H | Win a level (victory jingle) and lose a level (defeat jingle) | PC or phone |
| I | Close and open the game again: SETTINGS shows the same slider values | phone |
| J | `Submission/A12_Script_Map.png` | PC |

## Script

**0:00 - 0:15 - Intro (clip A)**
"Hi, I am Daulet. This is Dungeon Knight. This video is Assignment 12, the sound pass.
All sounds and both music tracks are my own files. I made them with a Python script, so there is no licence problem."

**0:15 - 0:35 - Mixer (clip B)**
"All audio goes through a Unity AudioMixer. Master has two groups: Music and SFX.
Music has a child group, Music Pause, for the pause effect. There are two snapshots: Normal and Paused."

**0:35 - 0:55 - Settings (clip A)**
"In Settings I have a Music slider and an SFX slider. The slider value goes to the mixer in decibels:
twenty times log ten of the value. Zero is minus eighty decibels, so it is silent.
The values are saved in my save file with the other settings."

**0:55 - 1:15 - Music crossfade (clip C)**
"The menu has a calm track. When I start a level, the level track fades in and the menu track fades out
in one and a half seconds, so there is no hard cut and no click."

**1:15 - 1:40 - Game sounds (clips D and E)**
"Steps, my magic shot, enemy hits, enemy death and coins all have sounds.
Repeated sounds change pitch a little and use two or three versions, so they do not sound like a machine gun.
Every enemy attack has a warning sound first. The Patroller growls, the Shooter charges with a rising sound,
and the Charger snorts. Then comes the slam, the shot or the dash. So I can hear the attack before it hits me."

**1:40 - 2:00 - Pause and restart (clips F and G)**
"When I pause, the Paused snapshot makes the music quieter and muffled. When I resume, it comes back.
When I restart the level, the music keeps playing. It does not start again."

**2:00 - 2:20 - Win, lose, save (clips H and I)**
"Winning plays a short jingle, losing plays a sad one. After I close and open the game,
my slider values are still the same."

**2:20 - 2:40 - Script map and end (clip J)**
"Here is the script map. Gameplay scripts only call GameAudio with a sound name.
GameAudio uses a fixed pool of audio sources, and every sound has a voice limit. Thank you for watching."

## Notes

- Before recording on the phone, turn the phone volume to about 50% and check there is no clipping on loud fights.
- Do not say that something works on Android unless you tested it on the phone in this recording.
