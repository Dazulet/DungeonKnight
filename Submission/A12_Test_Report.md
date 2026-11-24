# Assignment 12 - Test Report

Date: 2026-11-25. Branch: `feature/sound-pass` (from `feature/juice-pass`). Unity 6000.6.0f1.

**Where the tests ran:** all tests ran in the Unity Editor on my Windows PC, in batch mode.
**Nothing here is a result from a real Android phone.** Phone tests are in section 4 and are not done yet.

## 1. New sound tests (SoundProbe, Editor)

| Run | What it checks | Result |
|---|---|---|
| Prepare (no Play mode) | mixer groups and hierarchy, snapshots, exposed parameters, effects, unscaled time, AudioLibrary cues and clips, pitch/clip variation for repeated sounds, import settings of all 23 clips, no unrouted AudioSource in prefabs and scenes, exactly 1 AudioListener in MainMenu, Dungeon and Endless | 73/73 |
| Session 1 (Play mode) | see the list below | 78/78 |
| Session 2 (new Unity process = app restart, save kept) | volumes loaded from save.json, mixer values, sliders and % labels, slider 0 = -80 dB | 8/8 |

Session 1 checks (main ones, measured values):

- 1 GameAudio, 1 AudioListener per scene, 14 AudioSources (12 SFX + 2 music), all routed: music to Master/Music/Music Pause, SFX to Master/SFX. Still 14 at the end of the run, so no AudioSource is created per event.
- dB formula: 1.0 -> 0 dB, 0.5 -> -6.02 dB, 0 -> -80 dB, values below 0.0001 -> -80 dB.
- Settings: sliders start at 80%. Music slider 0.25 -> -12.04 dB, SFX 0.5 -> -6.02 dB, labels show 25% / 50%, values are written to save.json when the panel closes.
- Old save without volume fields -> 80% / 80%. Volumes outside 0-1 are clamped.
- 40 EnemyHit calls in one frame -> 1 sound (min interval). Many hits over 0.6 s -> at most 3 EnemyHit voices.
- Pitches seen: 0.956-1.029, all inside 0.92-1.08. The coin sound uses both of its clips.
- Menu -> Level 1: level music starts once; in the middle of the crossfade both tracks are audible (0.58 and 0.39); after 2 s the level track is at full volume and the menu track is at 0.
- Pause (timeScale 0): Paused snapshot reaches 900 Hz low-pass and -10 dB; the user music volume stays. Resume: back to 22000 Hz and 0 dB. UI click is heard while paused.
- Restart: music start count does not change and the level track stays at full volume (the music is not restarted).
- Warning before attack: Patroller wind-up 0.55 s before the slam, Shooter aim 0.70 s before the shot, Charger wind-up 0.65 s before the dash. A killed Patroller's wind-up sound is stopped.
- Every SoundId was heard once or more: footstep, shot, enemy hit, enemy death, player hurt, coin, 3 warnings, 3 attacks, Slime/Bat bite, UI click, victory, defeat.
- Victory screen uses the Paused snapshot; restart goes back to Normal.
- 0 errors or exceptions.

## 2. Old tests (regression)

All old tests were run again after the sound pass and the MainMenu rebuild:

| Test | Result |
|---|---|
| Assignment 10: Patroller 22, Shooter 29, Charger 24, Fairness 14, Reaction 15, Level 34, Touch 18 | 156/156 |
| GameplayProbe (40 s / clear) | 0 errors / Victory |
| StageSmoke Menu / Dungeon / Endless | 0 / 0 / 0 errors |
| Assignment 11: Data 27, LevelFlow 65, Save unit 29, corrupt 6, session 1 6, session 2 18, UiShots 25 | 176/176 checks + LevelClear 1/2/3 Victory (12.3 s, 33.0 s, 53.0 s) |
| Assignment 13: JuiceProbe | 87/87 |

UiShots also took screenshots of the new Settings panel at 1440, 1920, 2340 and 2400 x 1080; the sliders fit.
Screenshot: `Submission/Screenshots/A12/a12_settings_sliders.png`.

## 3. Audio files

- 21 SFX (44.1 kHz mono) and 2 music loops (32 kHz mono), all made by `Tools/generate_audio.py`.
- Peak levels in the files: SFX -14 to -6 dBFS, music -7 dBFS. A Compressor on Master (threshold -6 dB) is a safety net.
- Music loop seam: the jump between the last and first sample is 0.0045 (menu) and 0.0003 (level), smaller than a normal step inside the music, so the loop has no click in the file.
- Import: SFX = Decompress On Load + ADPCM; music = Compressed In Memory + Vorbis (quality 0.6), load in background.

## 4. Not tested yet (must do on the phone)

- [ ] Real loudness and clipping on the phone speaker and on headphones in a big fight.
- [ ] Android audio latency of the warning sounds (are they still clearly before the hit?).
- [ ] Music loop has no click on the phone (Vorbis encoding).
- [ ] App to background and back: music and pause snapshot.
- [ ] Slider values after closing the app from the task list.
- [ ] I did not listen to the sounds through the automated tests; I must listen to every sound in the Editor before the video.
